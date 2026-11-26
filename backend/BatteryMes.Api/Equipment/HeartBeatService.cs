using System.Text.Json;
using SqlSugar;

namespace BatteryMes.Api.Equipment;

public sealed class HeartBeatService(ISqlSugarClient db, ILogger<HeartBeatService> logger)
{
    public const string IdleOperationId = "AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA";

    private readonly List<Action> pending = [];

    public HeartBeatResponse Handle(HeartBeatRequest? request)
    {
        var response = new HeartBeatResponse();
        try
        {
            if (request?.EquipmentInfo is null)
            {
                throw new InvalidOperationException("未提供设备信息");
            }

            foreach (var equipment in request.EquipmentInfo)
            {
                response.EquipmentInfo.Add(HandleOne(request, equipment?.EquipmentCode));
            }

            foreach (var action in pending)
            {
                action();
            }

            response.ServerDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "设备心跳失败");
            return new HeartBeatResponse
            {
                EquipmentInfo =
                [
                    new EquipmentHeartbeat { ResultFlag = false, Message = ex.Message },
                ],
            };
        }
    }

    private EquipmentHeartbeat HandleOne(HeartBeatRequest request, string? equipmentCode)
    {
        var item = new EquipmentHeartbeat
        {
            ResultFlag = true,
            EquipmentCode = equipmentCode,
            ReceiptNumber = Guid.NewGuid().ToString(),
        };

        try
        {
            var now = DateTime.Now;
            var claimed = db.Updateable<EquipmentDiFlags>()
                .SetColumns(row => row.OperationId == item.ReceiptNumber)
                .SetColumns(row => row.OperationStatus == 1)
                .Where(row => row.EquipmentCode == equipmentCode && row.DeleteFlag == 0 && row.OperationId == IdleOperationId)
                .ExecuteCommand() > 0;

            if (!claimed)
            {
                item.Message = "暂无数据或上次心跳正在处理...";
                return item;
            }

            var flags = db.Queryable<EquipmentDiFlags>().Where(row => row.OperationId == item.ReceiptNumber).Take(1).ToList().FirstOrDefault();
            if (flags is null)
            {
                return item;
            }

            db.Updateable<EmDiExecParam>()
                .SetColumns(row => row.OperationId == item.ReceiptNumber)
                .SetColumns(row => row.OperationStatus == 1)
                .SetColumns(row => row.OperationTime == now)
                .Where(row => row.EquipmentCode == equipmentCode && row.DeleteFlag == 0 && row.OperationId == IdleOperationId)
                .ExecuteCommand();

            FillControls(item, equipmentCode);
            FillMaterialLoad(item, equipmentCode);
            FillMaterialUnload(item, equipmentCode);
            FillPartLoad(item, equipmentCode);
            FillPartUnload(item, equipmentCode);
            item.EqptGetParaFlag = flags.GetParaFlag > 0 ? 1 : 0;
            item.EqptGetBatchFlag = flags.GetBatchFlag > 0 ? 1 : 0;
            item.RecipeGetFlag = flags.GetRecipeFlag > 0 ? 1 : 0;
            item.EQReasonCodeFlag = flags.GetReasonFlag > 0 ? 1 : 0;
            FillBatchDelete(item, equipmentCode);
            FillOrderSend(item, equipmentCode);
            FillOrderDelete(item, equipmentCode);
            FillChannel(item, equipmentCode);
            QueueFlagClose(flags, now);
            WriteLog(request, item, equipmentCode);
            return item;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "设备 {EquipmentCode} 心跳异常", equipmentCode);
            item.ResultFlag = false;
            item.Message = ex.Message;
            return item;
        }
    }

    private void FillControls(EquipmentHeartbeat item, string? equipmentCode)
    {
        var flags = CurrentFlags(item.ReceiptNumber);
        if (flags.EquipmentControl <= 0)
        {
            return;
        }

        if (!Take(out List<EmDiControls> rows, equipmentCode, item.ReceiptNumber))
        {
            item.EqptCtrlFlag = 0;
            return;
        }

        item.EqptCtrlFlag = 1;
        item.EqptCtrlParam = rows.Select(row => new EqptCtrlParam
        {
            ChannelCode = row.ChannelCode,
            Cmd = row.Cmd,
            ReasonCode = row.ReasonCode,
            Description = row.Description,
        }).ToList();
        QueueSent<EmDiControls>(item.ReceiptNumber);
    }

    private void FillMaterialLoad(EquipmentHeartbeat item, string? equipmentCode)
    {
        var flags = CurrentFlags(item.ReceiptNumber);
        if (flags.MaterialLoad <= 0)
        {
            return;
        }

        if (!Take(out List<EmDiMaterialLoad> rows, equipmentCode, item.ReceiptNumber))
        {
            item.MaterialLoadFlag = 0;
            return;
        }

        item.MaterialLoadFlag = 1;
        item.MaterialLoadParam = rows.Select(row => new MaterialLoadParam
        {
            BasicUom = row.BasicUom,
            LabelNo = row.LabelNo,
            Location = row.Location,
            Quantity = row.Quantity,
            UomCode = row.Uom,
            UomCoefficient = row.UomCoefficient,
        }).ToList();
        QueueSent<EmDiMaterialLoad>(item.ReceiptNumber);
    }

    private void FillMaterialUnload(EquipmentHeartbeat item, string? equipmentCode)
    {
        var flags = CurrentFlags(item.ReceiptNumber);
        if (flags.MaterialUnload <= 0)
        {
            return;
        }

        if (!Take(out List<EmDiMaterialUnload> rows, equipmentCode, item.ReceiptNumber))
        {
            item.MaterialUnloadFlag = 0;
            return;
        }

        item.MaterialUnloadFlag = 1;
        item.MaterialUnloadParam = rows.Select(row => new MaterialUnloadParam
        {
            LabelNo = row.LabelNo,
            Location = row.Location,
        }).ToList();
        QueueSent<EmDiMaterialUnload>(item.ReceiptNumber);
    }

    private void FillPartLoad(EquipmentHeartbeat item, string? equipmentCode)
    {
        var flags = CurrentFlags(item.ReceiptNumber);
        if (flags.PartLoad <= 0)
        {
            return;
        }

        if (!Take(out List<EmDiPartLoad> rows, equipmentCode, item.ReceiptNumber))
        {
            item.PartLoadFlag = 0;
            return;
        }

        item.PartLoadFlag = 1;
        item.PartLoadParam = rows.Select(row => new PartLoadParam
        {
            PartDesc = row.PartDesc,
            Location = row.Location,
            SpartID = row.SpartId,
            UseLifetime = row.UseLifetime,
            WarningLife = row.WarningLife,
        }).ToList();
        QueueSent<EmDiPartLoad>(item.ReceiptNumber);
    }

    private void FillPartUnload(EquipmentHeartbeat item, string? equipmentCode)
    {
        var flags = CurrentFlags(item.ReceiptNumber);
        if (flags.PartUnload <= 0)
        {
            return;
        }

        if (!Take(out List<EmDiPartUnload> rows, equipmentCode, item.ReceiptNumber))
        {
            item.PartUnLoadFlag = 0;
            return;
        }

        item.PartUnLoadFlag = 1;
        item.PartUnLoadParam = rows.Select(row => new PartUnLoadParam
        {
            Location = row.Location,
            SpartID = row.SpartId,
        }).ToList();
        QueueSent<EmDiPartUnload>(item.ReceiptNumber);
    }

    private void FillBatchDelete(EquipmentHeartbeat item, string? equipmentCode)
    {
        var flags = CurrentFlags(item.ReceiptNumber);
        if (flags.DeleteBatchFlag <= 0)
        {
            return;
        }

        if (!Take(out List<EmDiBatchDelete> rows, equipmentCode, item.ReceiptNumber))
        {
            item.BatchDelFlag = 0;
            return;
        }

        item.BatchDelFlag = 1;
        item.BatchDelParam = rows.Select(row => new BatchDelParam { CreateBatchNo = row.CreateBatchNo }).ToList();
        QueueSent<EmDiBatchDelete>(item.ReceiptNumber);
    }

    private void FillOrderSend(EquipmentHeartbeat item, string? equipmentCode)
    {
        var flags = CurrentFlags(item.ReceiptNumber);
        if (flags.SendOrder <= 0)
        {
            return;
        }

        if (!Take(out List<EmDiPackOrderSend> rows, equipmentCode, item.ReceiptNumber))
        {
            item.OrderSendFlag = 0;
            return;
        }

        item.OrderSendFlag = 1;
        item.OrderSendParam = rows.Select(row => new OrderSendParam
        {
            ContainerNo = row.ContainerNo,
            OrderNo = row.OrderNo,
            PlanFeedQuantity = row.PlanFeedQuantity,
            PlanQuantity = row.PlanQuantity,
            Polarity = row.Polarity,
            ProcessRouteNo = row.ProcessRouteNo,
            ProductName = row.ProductName,
            ProductNo = row.ProductNo,
            Sort = row.Sequence,
            VersionNo = row.VersionNo,
            WorkOrderCreateTime = row.WorkOrderCreateTime,
            WorkOrderNo = row.WorkOrderNo,
            WorkOrderType = row.WorkOrderType,
        }).ToList();
        QueueSent<EmDiPackOrderSend>(item.ReceiptNumber);
    }

    private void FillOrderDelete(EquipmentHeartbeat item, string? equipmentCode)
    {
        var flags = CurrentFlags(item.ReceiptNumber);
        if (flags.DeleteOrder <= 0)
        {
            return;
        }

        if (!Take(out List<EmDiPackOrderDelete> rows, equipmentCode, item.ReceiptNumber))
        {
            item.OrderDelFlag = 0;
            return;
        }

        item.OrderDelFlag = 1;
        item.OrderDelParam = rows.Select(row => new OrderDelParam
        {
            Status = row.Status ?? 0,
            WorkOrderNo = row.WorkOrderNo,
        }).ToList();
        QueueSent<EmDiPackOrderDelete>(item.ReceiptNumber);
    }

    private void FillChannel(EquipmentHeartbeat item, string? equipmentCode)
    {
        var flags = CurrentFlags(item.ReceiptNumber);
        if (flags.ChannelFlag <= 0)
        {
            return;
        }

        if (!Take(out List<EmDiEquipChannel> rows, equipmentCode, item.ReceiptNumber))
        {
            item.ChannelCtrlFlag = 0;
            return;
        }

        item.ChannelCtrlFlag = 1;
        item.ChannelCtrlParam = rows.Select(row => new ChannelCtrlParam
        {
            ChannelCode = row.ChannelCode,
            SlotID = row.SlotId,
            OPFlag = row.OpFlag,
        }).ToList();
        QueueSent<EmDiEquipChannel>(item.ReceiptNumber);
    }

    private EquipmentDiFlags CurrentFlags(string? receipt)
    {
        return db.Queryable<EquipmentDiFlags>().Where(row => row.OperationId == receipt).Take(1).ToList().First();
    }

    private bool Take<T>(out List<T> rows, string? equipmentCode, string? receipt) where T : HeartbeatQueueRow, new()
    {
        var hit = db.Updateable<T>()
            .SetColumns(row => row.OperationId == receipt)
            .SetColumns(row => row.OperationStatus == 1)
            .Where(row => row.EquipmentCode == equipmentCode && row.DeleteFlag == 0 && row.OperationId == IdleOperationId)
            .ExecuteCommand() > 0;
        rows = hit
            ? db.Queryable<T>().Where(row => row.OperationId == receipt).ToList()
            : [];
        return hit;
    }

    private void QueueSent<T>(string? receipt) where T : HeartbeatQueueRow, new()
    {
        pending.Add(() => db.Updateable<T>()
            .SetColumns(row => row.OperationStatus == 2)
            .Where(row => row.OperationId == receipt)
            .ExecuteCommand());
    }

    private void QueueFlagClose(EquipmentDiFlags flags, DateTime now)
    {
        pending.Add(() => db.Ado.ExecuteCommand(
            """
            UPDATE EQUIPMENT_DI_FLAGS
            SET OPERATION_STATUS = 2,
                EQUIPMENT_CONTROL = EQUIPMENT_CONTROL - @equipmentControl,
                MATERIAL_LOAD = MATERIAL_LOAD - @materialLoad,
                MATERIAL_UNLOAD = MATERIAL_UNLOAD - @materialUnload,
                PART_LAOD = PART_LAOD - @partLoad,
                PART_UNLAOD = PART_UNLAOD - @partUnload,
                GET_PARA_FLAG = GET_PARA_FLAG - @getPara,
                GET_BATCH_FLAG = GET_BATCH_FLAG - @getBatch,
                GET_RECIPE_FLAG = GET_RECIPE_FLAG - @getRecipe,
                GET_REASON_FLAG = GET_REASON_FLAG - @getReason,
                DEL_BATCH_FLAG = DEL_BATCH_FLAG - @deleteBatch,
                SEND_ORDER = SEND_ORDER - @sendOrder,
                DEL_ORDER = DEL_ORDER - @deleteOrder,
                GET_BATTERY_CAPACITY_RULE_FLAG = GET_BATTERY_CAPACITY_RULE_FLAG - @capacityRule,
                GET_CAPACITY_PARA_FLAG = GET_CAPACITY_PARA_FLAG - @capacityPara,
                SEND_EQUIPMENT_CHANNEL_FLAG = SEND_EQUIPMENT_CHANNEL_FLAG - @channel,
                MODIFY_TIME = @now,
                OPERATION_TIME = @now
            WHERE OPERATION_ID = @receipt
            """,
            new
            {
                equipmentControl = flags.EquipmentControl,
                materialLoad = flags.MaterialLoad,
                materialUnload = flags.MaterialUnload,
                partLoad = flags.PartLoad,
                partUnload = flags.PartUnload,
                getPara = flags.GetParaFlag,
                getBatch = flags.GetBatchFlag,
                getRecipe = flags.GetRecipeFlag,
                getReason = flags.GetReasonFlag,
                deleteBatch = flags.DeleteBatchFlag,
                sendOrder = flags.SendOrder,
                deleteOrder = flags.DeleteOrder,
                capacityRule = flags.BatteryCapacityRuleFlag,
                capacityPara = flags.CapacityParaFlag,
                channel = flags.ChannelFlag,
                now,
                receipt = flags.OperationId,
            }));
    }

    private void WriteLog(HeartBeatRequest request, EquipmentHeartbeat item, string? equipmentCode)
    {
        try
        {
            var now = DateTime.Now;
            db.Insertable(new MessageInMesEc
            {
                Nid = Guid.NewGuid().ToString(),
                MessageId = "",
                MessageType = "HeartBeat",
                Content = JsonSerializer.Serialize(request, HeartbeatJson.Options),
                ReturnContent = JsonSerializer.Serialize(item, HeartbeatJson.Options),
                ReceiveTime = now,
                DoTime = now,
                RetryNum = 0,
                System = equipmentCode,
                DoStatus = 1,
                ErrorMessage = "",
            }).ExecuteCommand();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "心跳日志写入失败");
        }
    }
}
