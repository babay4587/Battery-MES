using System.Text.Json;
using BatteryMes.Api;
using SqlSugar;

namespace BatteryMes.Api.Equipment;

public sealed class EquipmentStatusService(ISqlSugarClient db, ILogger<EquipmentStatusService> logger)
{
    public CommandResult ClearEquipHeartBeatStatus(HeartBeatRequest? request)
    {
        var result = new CommandResult { successed = true };
        var equipmentCode = request?.EquipmentInfo?.FirstOrDefault()?.EquipmentCode;
        if (string.IsNullOrWhiteSpace(equipmentCode))
        {
            result.successed = false;
            result.Message = "未提供设备信息";
            return result;
        }

        try
        {
            db.Ado.BeginTran();
            MarkFinished<EmDiExecParam>(equipmentCode);
            MarkFinished<EmDiMaterialLoad>(equipmentCode);
            MarkFinished<EmDiBatchDelete>(equipmentCode);
            MarkFinished<EmDiMaterialUnload>(equipmentCode);
            MarkFinished<EmDiPartLoad>(equipmentCode);
            MarkFinished<EmDiPartUnload>(equipmentCode);
            db.Ado.CommitTran();
        }
        catch (Exception ex)
        {
            db.Ado.RollbackTran();
            logger.LogError(ex, "清理设备心跳状态失败");
            result.successed = false;
            result.Message = ex.Message;
        }

        WriteLog("ClearEquipHeartBeatStatus", request, result, equipmentCode, result.successed, result.Message);
        return result;
    }

    public EquipmentInfoListResponse EquipStatus(EquipStatusRequest? request)
    {
        var response = new EquipmentInfoListResponse();
        if (request?.EquipmentInfo is null || request.EquipmentInfo.Count == 0)
        {
            response.EquipmentInfo.Add(Fail(null, "设备信息EquipmentInfo为空"));
            WriteLog("EquipStatus", request, response, null, false, "设备信息EquipmentInfo为空");
            return response;
        }

        try
        {
            foreach (var equip in request.EquipmentInfo)
            {
                var item = new EquipmentInfoResult
                {
                    ResultFlag = true,
                    EquipmentCode = equip.EquipmentCode,
                };
                var now = DateTime.Now;
                var running = equip.EquipmentModel == "RUN";
                var devStatus = running ? "Running" : "Stopping";
                UpsertDevStatus(equip.EquipmentCode, devStatus);
                var modifyTime = now;
                if (!DateTime.TryParse(equip.ModifyTime, out modifyTime))
                {
                    modifyTime = now;
                }

                db.Insertable(new EamEcEquipStatus
                {
                    Nid = Guid.NewGuid().ToString(),
                    DeleteFlag = 0,
                    CreateTime = now,
                    EquipmentCode = equip.EquipmentCode,
                    EmployeeNo = equip.EmployeeNo,
                    EquipmentModel = equip.EquipmentModel,
                    EquipmentStatusId = equip.EquipmentStatusID,
                    ReasonCode = equip.ReasonCode,
                    Description = equip.Description,
                    ModifyTime = modifyTime,
                }).ExecuteCommand();
                WriteDowntime(equip.EquipmentCode, equip.EmployeeNo, equip.ReasonCode, running ? 1 : 0, now);
                response.EquipmentInfo.Add(item);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "设备状态上报失败");
            response.EquipmentInfo.Add(Fail(null, ex.Message));
        }

        var ok = response.EquipmentInfo.All(item => item.ResultFlag);
        WriteLog("EquipStatus", request, response, response.EquipmentInfo.FirstOrDefault()?.EquipmentCode, ok, JoinMessages(response));
        return response;
    }

    public EquipmentInfoListResponse EquipAlert(EquipAlertRequest? request)
    {
        var response = new EquipmentInfoListResponse();
        var item = new EquipmentInfoResult { ResultFlag = true };
        if (request?.EquipmentInfo is null || request.EquipmentInfo.Count == 0)
        {
            item.ResultFlag = false;
            item.Message = "设备信息EquipmentInfo为空";
            response.EquipmentInfo.Add(item);
            WriteLog("EquipAlert", request, response, null, false, item.Message);
            return response;
        }

        var equip = request.EquipmentInfo[0];
        item.EquipmentCode = equip.EquipmentCode;
        try
        {
            var now = DateTime.Now;
            foreach (var alert in equip.AlertInfo ?? [])
            {
                if (alert.AlertReset == 1)
                {
                    var open = db.Queryable<EamFaultRecord>().Any(row =>
                        row.DevCode == equip.EquipmentCode
                        && row.FaultCode == alert.AlertCode
                        && row.Status == 0
                        && row.DeleteFlag == 0
                        && row.CreateTime > now.AddDays(-2));
                    if (open)
                    {
                        continue;
                    }

                    var start = now;
                    if (!DateTime.TryParse(alert.AlertTime, out start))
                    {
                        start = now;
                    }

                    db.Insertable(new EamFaultRecord
                    {
                        Id = Guid.NewGuid().ToString(),
                        CreateBy = equip.EmployeeNo,
                        CreateTime = now,
                        DeleteFlag = 0,
                        Status = 0,
                        StartTime = start,
                        DevCode = equip.EquipmentCode,
                        FaultCode = alert.AlertCode,
                        FaultDesc = alert.AlertDescription,
                        FaultLevel = alert.AlertLevel,
                    }).ExecuteCommand();
                }
                else if (alert.AlertReset == 0)
                {
                    var record = db.Queryable<EamFaultRecord>()
                        .Where(row => row.DeleteFlag == 0 && row.DevCode == equip.EquipmentCode && row.FaultCode == alert.AlertCode && row.CreateTime > now.AddMonths(-3))
                        .Take(1)
                        .ToList()
                        .FirstOrDefault();
                    if (record is null)
                    {
                        item.ResultFlag = false;
                        item.Message = "复位的报警信息不存在";
                        break;
                    }

                    var end = now;
                    if (!DateTime.TryParse(alert.AlertTime, out end))
                    {
                        end = now;
                    }

                    db.Updateable<EamFaultRecord>()
                        .SetColumns(row => row.EndTime == end)
                        .SetColumns(row => row.Status == 1)
                        .SetColumns(row => row.ModifyTime == now)
                        .SetColumns(row => row.ModifyBy == equip.EmployeeNo)
                        .Where(row => row.Id == record.Id)
                        .ExecuteCommand();
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "设备报警上报失败");
            item.ResultFlag = false;
            item.Message = ex.Message;
        }

        response.EquipmentInfo.Add(item);
        WriteLog("EquipAlert", request, response, item.EquipmentCode, item.ResultFlag, item.Message);
        return response;
    }

    public EquipmentInfoListResponse EqptParaCHNG(EquipParaChangeRequest? request)
    {
        var response = new EquipmentInfoListResponse();
        var item = new EquipmentInfoResult { ResultFlag = true };
        if (request?.EquipmentInfo is null || request.EquipmentInfo.Count == 0)
        {
            item.ResultFlag = false;
            item.Message = "EquipmentInfo为空EquipmentInfo";
            response.EquipmentInfo.Add(item);
            WriteLog("EqptParaCHNG", request, response, null, false, item.Message);
            return response;
        }

        var equip = request.EquipmentInfo[0];
        item.EquipmentCode = equip.EquipmentCode;
        if (equip.ParameterInfo is null || equip.ParameterInfo.Count == 0)
        {
            item.ResultFlag = false;
            item.Message = "ParameterInfo为空ParameterInfo";
        }
        else if (string.IsNullOrEmpty(equip.EquipmentCode))
        {
            item.ResultFlag = false;
            item.Message = "EquipmentCode为空";
        }
        else
        {
            try
            {
                var now = DateTime.Now;
                var rows = new List<PrdEqpParaHis>();
                foreach (var para in equip.ParameterInfo)
                {
                    if (string.IsNullOrEmpty(para.ParameterCode))
                    {
                        continue;
                    }

                    rows.Add(new PrdEqpParaHis
                    {
                        Nid = Guid.NewGuid().ToString(),
                        CreateBy = equip.EmployeeNo,
                        CreateTime = now,
                        DeleteFlag = 0,
                        EquipmentCode = equip.EquipmentCode,
                        StepId = para.StepID,
                        OrderNo = "",
                        MaterialCode = equip.ProductNo,
                        OperationCode = equip.OprSequenceNo,
                        ParamCode = para.ParameterCode,
                        ParamName = para.Description,
                        CollectedValue = para.NewValue,
                        TargetValue = para.OldValue,
                    });
                }

                if (rows.Count > 0)
                {
                    db.Insertable(rows).ExecuteCommand();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "设备参数变更失败");
                item.ResultFlag = false;
                item.Message = "EqptParaCHNG异常：" + ex.Message;
            }
        }

        response.EquipmentInfo.Add(item);
        WriteLog("EqptParaCHNG", request, response, item.EquipmentCode, item.ResultFlag, item.Message);
        return response;
    }

    public EquipmentInfoListResponse EquipRunRequest(EquipRunRequest? request)
    {
        var response = new EquipmentInfoListResponse();
        var item = new EquipmentInfoResult { ResultFlag = true };
        if (request?.EquipmentInfo is null || request.EquipmentInfo.Count == 0)
        {
            item.ResultFlag = false;
            item.Message = "设备信息为空EquipmentInfo";
        }
        else
        {
            var equip = request.EquipmentInfo[0];
            if (string.IsNullOrEmpty(equip.EquipmentCode))
            {
                item.ResultFlag = false;
                item.Message = "设备编码为空EquipmentCode";
            }
            else
            {
                item.EquipmentCode = equip.EquipmentCode;
                var dev = db.Queryable<EamDev>()
                    .Where(row => row.DeleteFlag == 0 && row.DevCode == equip.EquipmentCode)
                    .Take(1)
                    .ToList()
                    .FirstOrDefault();
                if (dev is null)
                {
                    item.ResultFlag = false;
                    item.Message = $"设备不存在{equip.EquipmentCode}";
                }
                else
                {
                    item.ResultFlag = dev.DevStatus is "Running" or "StandBy" or "Stopping";
                    item.Message = "设备状态为" + dev.DevStatus;
                }
            }
        }

        response.EquipmentInfo.Add(item);
        WriteLog("EquipRunRequest", request, response, item.EquipmentCode, item.ResultFlag, item.Message);
        return response;
    }

    public EqptCtrlResponse EqptCtrl(EqptCtrlRequest? request)
    {
        var response = new EqptCtrlResponse();
        if (request?.EquipmentInfo is null)
        {
            response.EquipmentInfo.Add(Fail(null, "未提供设备信息"));
            WriteLog("EqptCtrl", request, response, null, false, "未提供设备信息");
            return response;
        }

        foreach (var item in request.EquipmentInfo)
        {
            response.EquipmentInfo.Add(new EquipmentInfoResult
            {
                EquipmentCode = item.EquipmentCode,
                ResultFlag = true,
                Message = "",
            });
            if (!string.IsNullOrEmpty(item.ReceiptNumber))
            {
                db.Updateable<EmDiControls>()
                    .SetColumns(row => row.OperationStatus == 3)
                    .Where(row => row.OperationId == item.ReceiptNumber)
                    .ExecuteCommand();
            }
        }

        WriteLog("EqptCtrl", request, response, response.EquipmentInfo.FirstOrDefault()?.EquipmentCode, true, "");
        return response;
    }

    public EqptCtrlResponse ChannelCtrlFeedback(ChannelCtrlFeedbackRequest? request)
    {
        var response = new EqptCtrlResponse();
        if (request?.EquipmentInfo is null)
        {
            response.EquipmentInfo.Add(Fail(null, "未提供设备信息"));
            WriteLog("ChannelCtrlFeedback", request, response, null, false, "未提供设备信息");
            return response;
        }

        foreach (var item in request.EquipmentInfo)
        {
            response.EquipmentInfo.Add(new EquipmentInfoResult
            {
                EquipmentCode = item.EquipmentCode,
                ResultFlag = true,
                Message = "",
            });
        }

        WriteLog("ChannelCtrlFeedback", request, response, response.EquipmentInfo.FirstOrDefault()?.EquipmentCode, true, "");
        return response;
    }

    public CommandResult EquipControl(List<EquipControlItem>? equipmentInfos)
    {
        var result = new CommandResult { successed = true, Message = "MES控机消息保存成功" };
        if (equipmentInfos is null || equipmentInfos.Count == 0)
        {
            result.successed = false;
            result.Message = "保存失败";
            WriteLog("EquipControl", equipmentInfos, result, null, false, result.Message);
            return result;
        }

        try
        {
            foreach (var equipmentInfo in equipmentInfos)
            {
                db.Insertable(new EmDiControls
                {
                    Nid = Guid.NewGuid().ToString(),
                    EquipmentCode = equipmentInfo.EquipmentCode,
                    ChannelCode = null,
                    Cmd = equipmentInfo.Cmd,
                    ReasonCode = equipmentInfo.ReasonCode,
                    Description = equipmentInfo.Description,
                    OperationId = HeartBeatService.IdleOperationId,
                    OperationStatus = 0,
                    DeleteFlag = 0,
                }).ExecuteCommand();
                SaveControlFlag(equipmentInfo.EquipmentCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "控机入队失败");
            result.successed = false;
            result.Message = $"控机消息保存失败：{ex.Message}";
        }

        WriteLog("EquipControl", equipmentInfos, result, equipmentInfos[0].EquipmentCode, result.successed, result.Message);
        return result;
    }

    public object SeedOpenMaterialLoad(string? equipmentCode)
    {
        var code = equipmentCode?.Trim() ?? "";
        if (code.Length == 0)
        {
            throw new AppException(400, "请填写设备号");
        }

        db.Insertable(new EmDiMaterialLoad
        {
            Nid = Guid.NewGuid().ToString(),
            EquipmentCode = code,
            LabelNo = "LB-TEST",
            Location = "A1",
            Quantity = "1",
            OperationId = HeartBeatService.IdleOperationId,
            OperationStatus = 0,
            DeleteFlag = 0,
        }).ExecuteCommand();
        return new { message = "已放一条待处理上料，状态是 0。点清理后，未完成条数应变成 0。" };
    }

    public object Snapshot(string? equipmentCode, string? topic)
    {
        var code = equipmentCode?.Trim() ?? "";
        return topic switch
        {
            "clear" => new
            {
                execParam = OpenCount<EmDiExecParam>(code),
                materialLoad = OpenCount<EmDiMaterialLoad>(code),
                materialUnload = OpenCount<EmDiMaterialUnload>(code),
                partLoad = OpenCount<EmDiPartLoad>(code),
                partUnload = OpenCount<EmDiPartUnload>(code),
                batchDelete = OpenCount<EmDiBatchDelete>(code),
            },
            "status" => new
            {
                devStatus = db.Queryable<EamDev>().Where(row => row.DeleteFlag == 0 && row.DevCode == code).Take(1).ToList().FirstOrDefault()?.DevStatus,
                latest = db.Queryable<EamEcEquipStatus>().Where(row => row.EquipmentCode == code).OrderByDescending(row => row.CreateTime).Take(3).ToList(),
                downtime = db.Queryable<EamOeeDowntimeRecord>().Where(row => row.DevCode == code && row.DeleteFlag == 0).OrderByDescending(row => row.CreateTime).Take(3).ToList(),
            },
            "alert" => db.Queryable<EamFaultRecord>().Where(row => row.DevCode == code && row.DeleteFlag == 0).OrderByDescending(row => row.CreateTime).Take(5).ToList(),
            "para" => db.Queryable<PrdEqpParaHis>().Where(row => row.EquipmentCode == code && row.DeleteFlag == 0).OrderByDescending(row => row.CreateTime).Take(5).ToList(),
            "control" => db.Queryable<EmDiControls>().Where(row => row.EquipmentCode == code && row.DeleteFlag == 0).OrderByDescending(row => row.Nid).Take(5).ToList(),
            _ => new { message = "未知核对项" },
        };
    }

    private void SaveControlFlag(string? equipmentCode)
    {
        var now = DateTime.Now;
        var updated = db.Ado.ExecuteCommand(
            """
            UPDATE EQUIPMENT_DI_FLAGS
            SET OPERATION_ID = @idle,
                OPERATION_STATUS = 0,
                EQUIPMENT_CONTROL = EQUIPMENT_CONTROL + 1,
                MODIFY_TIME = @now,
                OPERATION_TIME = NULL
            WHERE EQUIPMENT_CODE = @equipmentCode AND DELETE_FLAG = 0
            """,
            new { idle = HeartBeatService.IdleOperationId, now, equipmentCode });
        if (updated == 0)
        {
            db.Insertable(new EquipmentDiFlags
            {
                Nid = Guid.NewGuid().ToString(),
                EquipmentCode = equipmentCode,
                EquipmentControl = 1,
                OperationId = HeartBeatService.IdleOperationId,
                OperationStatus = 0,
                DeleteFlag = 0,
                ModifyTime = now,
            }).ExecuteCommand();
        }
    }

    private void UpsertDevStatus(string? equipmentCode, string devStatus)
    {
        var updated = db.Updateable<EamDev>()
            .SetColumns(row => row.DevStatus == devStatus)
            .Where(row => row.DeleteFlag == 0 && row.DevCode == equipmentCode)
            .ExecuteCommand();
        if (updated == 0)
        {
            db.Insertable(new EamDev
            {
                Nid = Guid.NewGuid().ToString(),
                DevCode = equipmentCode,
                DevStatus = devStatus,
                DeleteFlag = 0,
            }).ExecuteCommand();
        }
    }

    private void WriteDowntime(string? equipmentCode, string? employeeNo, string? reasonCode, int newStatus, DateTime now)
    {
        var created = NewDowntime(equipmentCode, employeeNo, reasonCode, newStatus, now);
        var record = db.Queryable<EamOeeDowntimeRecord>()
            .Where(row => row.DeleteFlag == 0 && row.DevCode == equipmentCode && row.CreateTime >= now.AddDays(-10))
            .OrderByDescending(row => row.CreateTime)
            .Take(1)
            .ToList()
            .FirstOrDefault();
        if (record is null)
        {
            db.Insertable(created).ExecuteCommand();
            return;
        }

        if (record.Status != newStatus)
        {
            db.Updateable<EamOeeDowntimeRecord>()
                .SetColumns(row => row.EndTime == now)
                .Where(row => row.Id == record.Id)
                .ExecuteCommand();
            db.Insertable(created).ExecuteCommand();
            return;
        }

        if (record.Status == 0)
        {
            db.Updateable<EamOeeDowntimeRecord>()
                .SetColumns(row => row.Field1 == created.Field1)
                .SetColumns(row => row.Level1LossCode == created.Level1LossCode)
                .SetColumns(row => row.Level2LossCode == created.Level2LossCode)
                .SetColumns(row => row.Level3LossCode == created.Level3LossCode)
                .SetColumns(row => row.Level4LossCode == created.Level4LossCode)
                .Where(row => row.Id == record.Id)
                .ExecuteCommand();
        }
    }

    private static EamOeeDowntimeRecord NewDowntime(string? equipmentCode, string? employeeNo, string? reasonCode, int newStatus, DateTime now)
    {
        var record = new EamOeeDowntimeRecord
        {
            Id = Guid.NewGuid().ToString(),
            DeleteFlag = 0,
            CreateTime = now,
            DevCode = equipmentCode,
            StartTime = now,
            Status = newStatus,
            CreateBy = employeeNo,
        };
        if (newStatus != 0 || string.IsNullOrEmpty(reasonCode))
        {
            return record;
        }

        record.Field1 = reasonCode;
        var parts = reasonCode.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length >= 1) record.Level1LossCode = parts[0];
        if (parts.Length >= 2) record.Level2LossCode = parts[0] + "-" + parts[1];
        if (parts.Length >= 3) record.Level3LossCode = parts[0] + "-" + parts[1] + "-" + parts[2];
        if (parts.Length >= 4) record.Level4LossCode = parts[0] + "-" + parts[1] + "-" + parts[2] + "-" + parts[3];
        return record;
    }

    private void MarkFinished<T>(string equipmentCode) where T : HeartbeatQueueRow, new()
    {
        db.Updateable<T>()
            .SetColumns(row => row.OperationStatus == 3)
            .Where(row => row.OperationStatus < 3 && row.EquipmentCode == equipmentCode)
            .ExecuteCommand();
    }

    private int OpenCount<T>(string equipmentCode) where T : HeartbeatQueueRow, new()
    {
        return db.Queryable<T>().Count(row => row.EquipmentCode == equipmentCode && row.OperationStatus < 3);
    }

    private static EquipmentInfoResult Fail(string? equipmentCode, string message)
    {
        return new EquipmentInfoResult
        {
            EquipmentCode = equipmentCode,
            ResultFlag = false,
            Message = message,
        };
    }

    private static string JoinMessages(EquipmentInfoListResponse response)
    {
        return string.Concat(response.EquipmentInfo.Select(item => item.Message));
    }

    private void WriteLog(string messageType, object? request, object response, string? equipmentCode, bool success, string? error)
    {
        try
        {
            var now = DateTime.Now;
            db.Insertable(new MessageInMesEc
            {
                Nid = Guid.NewGuid().ToString(),
                MessageId = "",
                MessageType = messageType,
                Content = JsonSerializer.Serialize(request, HeartbeatJson.Options),
                ReturnContent = JsonSerializer.Serialize(response, HeartbeatJson.Options),
                ReceiveTime = now,
                DoTime = now,
                RetryNum = 0,
                System = equipmentCode,
                DoStatus = success ? 1 : -1,
                ErrorMessage = error ?? "",
            }).ExecuteCommand();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{MessageType} 日志写入失败", messageType);
        }
    }
}
