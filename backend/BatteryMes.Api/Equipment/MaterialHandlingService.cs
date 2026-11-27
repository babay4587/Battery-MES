using System.Globalization;
using System.Text.Json;
using BatteryMes.Api;
using SqlSugar;

namespace BatteryMes.Api.Equipment;

public sealed class MaterialHandlingService(ISqlSugarClient db, ILogger<MaterialHandlingService> logger)
{
    private const string CheckMaterialCode = "MAT-01";
    private const string CheckOrderNo = "MO-TEST";

    public EquipmentInfoListResponse MaterialLoad(MaterialLoadRequest? request)
    {
        return AcceptReceipt(
            "MaterialLoad",
            request?.EquipmentInfo?.Select(item => new ReceiptItem(item.EquipmentCode, item.ReceiptNumber, item.ResultFlag, RequireCode: false, HonorResultFlag: true)).ToList(),
            request,
            MarkQueue<EmDiMaterialLoad>);
    }

    public EquipmentInfoListResponse MaterialUnload(MaterialUnloadRequest? request)
    {
        return AcceptReceipt(
            "MaterialUnLoad",
            request?.EquipmentInfo?.Select(item => new ReceiptItem(item.EquipmentCode, item.ReceiptNumber, item.ResultFlag, RequireCode: false, HonorResultFlag: false)).ToList(),
            request,
            MarkQueue<EmDiMaterialUnload>);
    }

    public EquipmentInfoListResponse PartLoad(PartLoadRequest? request)
    {
        return AcceptReceipt(
            "PartLoad",
            request?.EquipmentInfo?.Select(item => new ReceiptItem(item.EquipmentCode, item.ReceiptNumber, item.ResultFlag, RequireCode: true, HonorResultFlag: true)).ToList(),
            request,
            MarkQueue<EmDiPartLoad>);
    }

    public EquipmentInfoListResponse PartUnload(PartUnloadRequest? request)
    {
        return AcceptReceipt(
            "PartUnLoad",
            request?.EquipmentInfo?.Select(item => new ReceiptItem(item.EquipmentCode, item.ReceiptNumber, ResultFlag: true, RequireCode: true, HonorResultFlag: false)).ToList(),
            request,
            MarkQueue<EmDiPartUnload>);
    }

    public EquipmentInfoListResponse PartLife(PartLifeRequest? request)
    {
        var response = new EquipmentInfoListResponse();
        if (request?.EquipmentInfo is null || request.EquipmentInfo.Count == 0)
        {
            response.EquipmentInfo.Add(Fail(null, "未提供设备信息"));
            WriteLog("PartLife", request, response, null, false, "未提供设备信息");
            return response;
        }

        try
        {
            foreach (var equip in request.EquipmentInfo)
            {
                var dev = FindDev(equip.EquipmentCode);
                if (dev is null)
                {
                    response.EquipmentInfo.Add(Fail(equip.EquipmentCode, "设备不存在"));
                    continue;
                }

                var now = DateTime.Now;
                foreach (var part in equip.Partlifetimeinfo ?? [])
                {
                    var partDev = FindDev(part.PartNo);
                    if (partDev is null)
                    {
                        continue;
                    }

                    var lives = db.Queryable<EamLifeManagement>()
                        .Where(row => row.DeleteFlag == 0 && row.DevId == partDev.Nid)
                        .ToList();
                    var reported = ParseLife(part.UseLifetime);
                    foreach (var life in lives)
                    {
                        db.Insertable(new EamLifeRecord
                        {
                            Id = Guid.NewGuid().ToString(),
                            DevId = dev.Nid,
                            ParentId = life.DevId,
                            AlertLife = life.AlertLife,
                            LeftLifeBefore = life.LeftLife,
                            LeftLifeAfter = reported,
                            LifeUnit = life.LifeUnit,
                            OpType = 1,
                            OpReason = string.IsNullOrWhiteSpace(life.PartName) ? part.PartNo : life.PartName,
                            CreateBy = equip.EmployeeNo,
                            CreateTime = now,
                        }).ExecuteCommand();
                        db.Updateable<EamLifeManagement>()
                            .SetColumns(row => row.LeftLife == reported)
                            .SetColumns(row => row.ModifyBy == equip.EmployeeNo)
                            .SetColumns(row => row.ModifyTime == now)
                            .Where(row => row.Id == life.Id)
                            .ExecuteCommand();
                    }
                }

                response.EquipmentInfo.Add(new EquipmentInfoResult
                {
                    EquipmentCode = equip.EquipmentCode,
                    ResultFlag = true,
                    Message = "处理成功",
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "零部件寿命上传失败");
            response.EquipmentInfo.Add(Fail(null, "PartLife处理异常:" + ex.Message));
        }

        var ok = response.EquipmentInfo.Count > 0 && response.EquipmentInfo.All(item => item.ResultFlag);
        WriteLog("PartLife", request, response, response.EquipmentInfo.FirstOrDefault()?.EquipmentCode, ok, JoinMessages(response));
        return response;
    }

    public EquipmentInfoListResponse MaterialCheck(MaterialCheckRequest? request)
    {
        var response = new EquipmentInfoListResponse();
        var item = new EquipmentInfoResult { ResultFlag = true, Message = "" };
        if (request?.EquipmentInfo is null || request.EquipmentInfo.Count == 0)
        {
            item.ResultFlag = false;
            item.Message = "设备信息EquipmentInfo为空";
            response.EquipmentInfo.Add(item);
            WriteLog("MaterialCheck", request, response, null, false, item.Message);
            return response;
        }

        var equip = request.EquipmentInfo[0];
        item.EquipmentCode = equip.EquipmentCode;
        try
        {
            if (FindDev(equip.EquipmentCode) is null)
            {
                item.ResultFlag = false;
                item.Message = "MES未找到设备" + equip.EquipmentCode;
            }
            else
            {
                var orders = db.Queryable<PrdResourceContent>()
                    .Where(row => row.EquipmentCode == equip.EquipmentCode && row.OrderNo != null)
                    .Select(row => row.OrderNo)
                    .ToList();
                var bomList = orders.Count == 0
                    ? []
                    : db.Queryable<PomOrderBom>().Where(row => row.DeleteFlag == 0 && orders.Contains(row.OrderNo)).ToList();
                var pending = new List<PrdResourceLoading>();
                foreach (var material in equip.MaterialInfo ?? [])
                {
                    var stock = db.Queryable<MmMaterialStockDetail>()
                        .Where(row => row.MaterialBatch == material.LabelNo)
                        .Take(1)
                        .ToList()
                        .FirstOrDefault();
                    if (stock is null)
                    {
                        item.ResultFlag = false;
                        item.Message = "未找到物料信息" + material.LabelNo;
                        pending.Clear();
                        break;
                    }

                    var bom = bomList.FirstOrDefault(row => row.MaterialCode == stock.MaterialCode && row.OperationCode == equip.ProcessNo);
                    if (bom is null)
                    {
                        item.ResultFlag = false;
                        item.Message = $"物料：{stock.MaterialCode} 与工序：{equip.ProcessNo} 不匹配";
                        pending.Clear();
                        break;
                    }

                    pending.Add(new PrdResourceLoading
                    {
                        Nid = Guid.NewGuid().ToString(),
                        EquipmentCode = equip.EquipmentCode,
                        MaterialCode = stock.MaterialCode,
                        OrderNo = bom.OrderNo,
                        Quantity = 1,
                        TotalQuantity = 0,
                        MaterialBatch = material.LabelNo,
                        FeedLocation = equip.EquipmentCode,
                        ConsumedQty = 1,
                        Uom = bom.BomUom,
                        DeleteFlag = 0,
                        ArchiveFlag = 0,
                        CreateBy = "MaterialCheck",
                        CreateTime = DateTime.Now,
                    });
                }

                if (item.ResultFlag && pending.Count > 0)
                {
                    db.Ado.BeginTran();
                    try
                    {
                        foreach (var loading in pending)
                        {
                            ApplyLoad(loading);
                        }

                        db.Ado.CommitTran();
                    }
                    catch
                    {
                        db.Ado.RollbackTran();
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "物料校验失败");
            item.ResultFlag = false;
            item.Message = ex.Message;
        }

        response.EquipmentInfo.Add(item);
        WriteLog("MaterialCheck", request, response, item.EquipmentCode, item.ResultFlag, item.Message);
        return response;
    }

    public object SeedMaterialLoad(string? equipmentCode, string? receiptNumber)
    {
        ResetQueue(new EmDiMaterialLoad
        {
            EquipmentCode = RequireCode(equipmentCode),
            OperationId = RequireReceipt(receiptNumber),
            LabelNo = "LB-TEST",
            Location = "A1",
            Quantity = "1",
        });
        return Seeded("上料");
    }

    public object SeedMaterialUnload(string? equipmentCode, string? receiptNumber)
    {
        ResetQueue(new EmDiMaterialUnload
        {
            EquipmentCode = RequireCode(equipmentCode),
            OperationId = RequireReceipt(receiptNumber),
            LabelNo = "LB-TEST",
            Location = "A1",
        });
        return Seeded("下料");
    }

    public object SeedPartLoad(string? equipmentCode, string? receiptNumber)
    {
        ResetQueue(new EmDiPartLoad
        {
            EquipmentCode = RequireCode(equipmentCode),
            OperationId = RequireReceipt(receiptNumber),
            SpartId = "PART-01",
            PartDesc = "试验零部件",
            Location = "A1",
            UseLifetime = "80",
            WarningLife = "20",
        });
        return Seeded("零部件上机");
    }

    public object SeedPartUnload(string? equipmentCode, string? receiptNumber)
    {
        ResetQueue(new EmDiPartUnload
        {
            EquipmentCode = RequireCode(equipmentCode),
            OperationId = RequireReceipt(receiptNumber),
            SpartId = "PART-01",
            Location = "A1",
        });
        return Seeded("零部件下机");
    }

    public object SeedMaterialCheck(string? equipmentCode, string? labelNo, string? processNo)
    {
        var code = RequireCode(equipmentCode);
        var label = labelNo?.Trim() ?? "";
        var process = string.IsNullOrWhiteSpace(processNo) ? "OP10" : processNo.Trim();
        if (label.Length == 0)
        {
            throw new AppException(400, "请填写标签号");
        }

        EnsureDev(code, "StandBy");
        if (!db.Queryable<MmMaterialStockDetail>().Any(row => row.MaterialBatch == label))
        {
            db.Insertable(new MmMaterialStockDetail
            {
                Nid = Guid.NewGuid().ToString(),
                MaterialBatch = label,
                MaterialCode = CheckMaterialCode,
                RemainQty = 10,
                FeedQty = 0,
                LoadQty = 0,
                DeleteFlag = 0,
            }).ExecuteCommand();
        }

        var content = db.Queryable<PrdResourceContent>()
            .Where(row => row.DeleteFlag == 0 && row.EquipmentCode == code && row.OrderNo != null)
            .Take(1)
            .ToList()
            .FirstOrDefault();
        var orderNo = content?.OrderNo ?? CheckOrderNo;
        if (content is null)
        {
            db.Insertable(new PrdResourceContent
            {
                Nid = Guid.NewGuid().ToString(),
                EquipmentCode = code,
                OrderNo = orderNo,
                DeleteFlag = 0,
            }).ExecuteCommand();
        }

        if (!db.Queryable<PomOrderBom>().Any(row => row.DeleteFlag == 0 && row.OrderNo == orderNo && row.MaterialCode == CheckMaterialCode && row.OperationCode == process))
        {
            db.Insertable(new PomOrderBom
            {
                Nid = Guid.NewGuid().ToString(),
                OrderNo = orderNo,
                MaterialCode = CheckMaterialCode,
                OperationCode = process,
                BomUom = "EA",
                DeleteFlag = 0,
            }).ExecuteCommand();
        }

        return new { message = "已放设备、标签库存和工序 BOM。再发送应成功，并多一条上料记录。" };
    }

    public object SeedPartLife(string? equipmentCode, string? partNo)
    {
        var code = RequireCode(equipmentCode);
        var part = string.IsNullOrWhiteSpace(partNo) ? "PART-01" : partNo.Trim();
        EnsureDev(code, "StandBy");
        var partId = EnsureDev(part, null);
        var now = DateTime.Now;
        var life = db.Queryable<EamLifeManagement>()
            .Where(row => row.DeleteFlag == 0 && row.DevId == partId)
            .Take(1)
            .ToList()
            .FirstOrDefault();
        if (life is null)
        {
            db.Insertable(new EamLifeManagement
            {
                Id = Guid.NewGuid().ToString(),
                DevId = partId,
                PartCode = part,
                PartName = "试验零部件",
                AlertLife = 20,
                LeftLife = 80,
                LifeUnit = "次",
                DeleteFlag = 0,
                ModifyTime = now,
            }).ExecuteCommand();
        }
        else
        {
            db.Updateable<EamLifeManagement>()
                .SetColumns(row => row.PartCode == part)
                .SetColumns(row => row.PartName == "试验零部件")
                .SetColumns(row => row.AlertLife == 20)
                .SetColumns(row => row.LeftLife == 80)
                .SetColumns(row => row.LifeUnit == "次")
                .SetColumns(row => row.ModifyTime == now)
                .Where(row => row.Id == life.Id)
                .ExecuteCommand();
        }

        return new { message = "已放设备和零部件寿命，剩余寿命是 80。发送后应改成页面上填的剩余寿命。" };
    }

    public object? TrySnapshot(string? equipmentCode, string? topic)
    {
        var code = equipmentCode?.Trim() ?? "";
        return topic switch
        {
            "material-load" => QueueSnapshot<EmDiMaterialLoad>(code),
            "material-unload" => QueueSnapshot<EmDiMaterialUnload>(code),
            "part-load" => QueueSnapshot<EmDiPartLoad>(code),
            "part-unload" => QueueSnapshot<EmDiPartUnload>(code),
            "material-check" => new
            {
                devStatus = FindDev(code)?.DevStatus,
                stock = db.Queryable<MmMaterialStockDetail>().OrderByDescending(row => row.ModifyTime).Take(5).ToList(),
                loading = db.Queryable<PrdResourceLoading>().Where(row => row.EquipmentCode == code && row.DeleteFlag == 0).OrderByDescending(row => row.CreateTime).Take(5).ToList(),
            },
            "part-life" => PartLifeSnapshot(code),
            _ => null,
        };
    }

    private EquipmentInfoListResponse AcceptReceipt(string messageType, List<ReceiptItem>? items, object? request, Action<string?> markQueue)
    {
        var response = new EquipmentInfoListResponse();
        if (items is null)
        {
            response.EquipmentInfo.Add(Fail(null, "未提供设备信息"));
            WriteLog(messageType, request, response, null, false, "未提供设备信息");
            return response;
        }

        try
        {
            foreach (var item in items)
            {
                var result = new EquipmentInfoResult
                {
                    EquipmentCode = item.EquipmentCode,
                    ResultFlag = true,
                    Message = "",
                };
                response.EquipmentInfo.Add(result);
                if (item.RequireCode && string.IsNullOrEmpty(item.EquipmentCode))
                {
                    result.ResultFlag = false;
                    result.Message = "存在设备号为空的数据，请排查;";
                    break;
                }

                if (!item.HonorResultFlag || item.ResultFlag)
                {
                    CompleteExec(item.EquipmentCode, item.ReceiptNumber);
                }
                else
                {
                    FailExec(item.EquipmentCode, item.ReceiptNumber);
                }

                markQueue(item.ReceiptNumber);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{MessageType} 失败", messageType);
            response.EquipmentInfo.Add(Fail(null, ex.Message));
        }

        var ok = response.EquipmentInfo.All(item => item.ResultFlag);
        WriteLog(messageType, request, response, response.EquipmentInfo.FirstOrDefault()?.EquipmentCode, ok, JoinMessages(response));
        return response;
    }

    private void ApplyLoad(PrdResourceLoading loading)
    {
        var stock = db.Queryable<MmMaterialStockDetail>()
            .Where(row => row.MaterialBatch == loading.MaterialBatch)
            .Take(1)
            .ToList()
            .FirstOrDefault() ?? throw new InvalidOperationException("物料在MES库存中不存在.");
        var now = DateTime.Now;
        var previous = db.Queryable<PrdResourceLoading>()
            .Where(row => row.DeleteFlag == 0 && row.EquipmentCode == loading.EquipmentCode && row.MaterialCode == loading.MaterialCode)
            .OrderByDescending(row => row.MaterialSeqNo)
            .Take(1)
            .ToList()
            .FirstOrDefault();
        loading.MaterialSeqNo = (previous?.MaterialSeqNo ?? 0) + 1;
        loading.CreateTime = now;
        db.Insertable(loading).ExecuteCommand();
        db.Insertable(new PrdResourceLoadingHis
        {
            Nid = Guid.NewGuid().ToString(),
            EquipmentCode = loading.EquipmentCode,
            MaterialCode = loading.MaterialCode,
            OrderNo = loading.OrderNo,
            Quantity = loading.Quantity,
            MaterialBatch = loading.MaterialBatch,
            ConsumedQty = loading.Quantity,
            Type = 1,
            DeleteFlag = 0,
            CreateBy = loading.CreateBy,
            CreateTime = now,
        }).ExecuteCommand();
        var remain = stock.RemainQty;
        var feed = stock.FeedQty + remain;
        db.Updateable<MmMaterialStockDetail>()
            .SetColumns(row => row.FeedQty == feed)
            .SetColumns(row => row.LoadQty == remain)
            .SetColumns(row => row.ModifyBy == loading.CreateBy)
            .SetColumns(row => row.ModifyTime == now)
            .Where(row => row.Nid == stock.Nid)
            .ExecuteCommand();
        db.Insertable(new MmMaterialStockLog
        {
            Nid = Guid.NewGuid().ToString(),
            Type = "6",
            LogQty = remain,
            DeleteFlag = 0,
            MaterialBatch = loading.MaterialBatch,
            MaterialCode = loading.MaterialCode,
            ArchiveFlag = 0,
            SendCount = 0,
        }).ExecuteCommand();
    }

    private void CompleteExec(string? equipmentCode, string? receiptNumber)
    {
        var now = DateTime.Now;
        db.Updateable<EmDiExecParam>()
            .SetColumns(row => row.OperationStatus == 3)
            .SetColumns(row => row.OperationTime == now)
            .Where(row => row.DeleteFlag == 0 && row.EquipmentCode == equipmentCode && row.OperationId == receiptNumber && row.OperationStatus == 1)
            .ExecuteCommand();
    }

    private void FailExec(string? equipmentCode, string? receiptNumber)
    {
        db.Updateable<EmDiExecParam>()
            .SetColumns(row => row.OperationStatus == 4)
            .Where(row => row.EquipmentCode == equipmentCode && row.OperationId == receiptNumber)
            .ExecuteCommand();
    }

    private void MarkQueue<T>(string? receiptNumber) where T : HeartbeatQueueRow, new()
    {
        db.Updateable<T>()
            .SetColumns(row => row.OperationStatus == 3)
            .Where(row => row.DeleteFlag == 0 && row.OperationId == receiptNumber)
            .ExecuteCommand();
    }

    private object Seeded(string name)
    {
        ResetExec(RequireFromLast());
        return new { message = $"已放一条待反馈{name}。队列状态是 2，执行参数状态是 1。同一回执再放一次会改回这个状态。" };
    }

    private string? _seedCode;
    private string? _seedReceipt;

    private void ResetQueue<T>(T template) where T : HeartbeatQueueRow, new()
    {
        _seedCode = template.EquipmentCode;
        _seedReceipt = template.OperationId;
        template.OperationStatus = 2;
        template.DeleteFlag = 0;
        var exists = db.Queryable<T>().Any(row => row.DeleteFlag == 0 && row.EquipmentCode == template.EquipmentCode && row.OperationId == template.OperationId);
        if (!exists)
        {
            template.Nid = Guid.NewGuid().ToString();
            db.Insertable(template).ExecuteCommand();
            return;
        }

        db.Updateable<T>()
            .SetColumns(row => row.OperationStatus == 2)
            .Where(row => row.DeleteFlag == 0 && row.EquipmentCode == template.EquipmentCode && row.OperationId == template.OperationId)
            .ExecuteCommand();
    }

    private (string? Code, string? Receipt) RequireFromLast()
    {
        return (_seedCode, _seedReceipt);
    }

    private void ResetExec((string? Code, string? Receipt) seed)
    {
        var exists = db.Queryable<EmDiExecParam>().Any(row => row.DeleteFlag == 0 && row.EquipmentCode == seed.Code && row.OperationId == seed.Receipt);
        if (!exists)
        {
            db.Insertable(new EmDiExecParam
            {
                Nid = Guid.NewGuid().ToString(),
                EquipmentCode = seed.Code,
                OperationId = seed.Receipt,
                OperationStatus = 1,
                DeleteFlag = 0,
            }).ExecuteCommand();
            return;
        }

        db.Updateable<EmDiExecParam>()
            .SetColumns(row => row.OperationStatus == 1)
            .Where(row => row.DeleteFlag == 0 && row.EquipmentCode == seed.Code && row.OperationId == seed.Receipt)
            .ExecuteCommand();
    }

    private object QueueSnapshot<T>(string equipmentCode) where T : HeartbeatQueueRow, new()
    {
        return new
        {
            queue = db.Queryable<T>().Where(row => row.EquipmentCode == equipmentCode && row.DeleteFlag == 0).Take(5).ToList(),
            exec = db.Queryable<EmDiExecParam>().Where(row => row.EquipmentCode == equipmentCode && row.DeleteFlag == 0).Take(5).ToList(),
        };
    }

    private object PartLifeSnapshot(string equipmentCode)
    {
        var dev = FindDev(equipmentCode);
        return new
        {
            equipment = dev,
            life = db.Queryable<EamLifeManagement>().Where(row => row.DeleteFlag == 0).OrderByDescending(row => row.ModifyTime).Take(5).ToList(),
            records = dev is null
                ? new List<EamLifeRecord>()
                : db.Queryable<EamLifeRecord>().Where(row => row.DevId == dev.Nid).OrderByDescending(row => row.CreateTime).Take(5).ToList(),
        };
    }

    private EamDev? FindDev(string? equipmentCode)
    {
        return db.Queryable<EamDev>()
            .Where(row => row.DeleteFlag == 0 && row.DevCode == equipmentCode)
            .Take(1)
            .ToList()
            .FirstOrDefault();
    }

    private string EnsureDev(string code, string? statusIfNew)
    {
        var existing = FindDev(code);
        if (existing is not null)
        {
            return existing.Nid;
        }

        var nid = Guid.NewGuid().ToString();
        db.Insertable(new EamDev
        {
            Nid = nid,
            DevCode = code,
            DevStatus = statusIfNew,
            DeleteFlag = 0,
        }).ExecuteCommand();
        return nid;
    }

    private static string RequireCode(string? equipmentCode)
    {
        var code = equipmentCode?.Trim() ?? "";
        if (code.Length == 0)
        {
            throw new AppException(400, "请填写设备号");
        }

        return code;
    }

    private static string RequireReceipt(string? receiptNumber)
    {
        var receipt = receiptNumber?.Trim() ?? "";
        if (receipt.Length == 0)
        {
            throw new AppException(400, "请填写回执");
        }

        return receipt;
    }

    private static decimal ParseLife(string? text)
    {
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : 0m;
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

    private sealed record ReceiptItem(string? EquipmentCode, string? ReceiptNumber, bool ResultFlag, bool RequireCode, bool HonorResultFlag);
}
