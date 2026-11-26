namespace BatteryMes.Api.Equipment;

public sealed class EnqueueControlRequest
{
    public string? EquipmentCode { get; set; }
    public string? ChannelCode { get; set; }
    public string? Cmd { get; set; }
    public string? ReasonCode { get; set; }
    public string? Description { get; set; }
}

public sealed class EnqueueControlResult
{
    public string EquipmentCode { get; set; } = "";
    public string ChannelCode { get; set; } = "";
    public string Cmd { get; set; } = "";
    public string ReasonCode { get; set; } = "";
    public string Description { get; set; } = "";
    public string Message { get; set; } = "";
}

public sealed class HeartbeatBenchService(SqlSugar.ISqlSugarClient db)
{
    public EnqueueControlResult EnqueueControl(EnqueueControlRequest? request)
    {
        var equipmentCode = Require(request?.EquipmentCode, "设备号", 64);
        var channelCode = Require(request?.ChannelCode, "通道", 64);
        var cmd = Require(request?.Cmd, "命令", 64);
        var reasonCode = Optional(request?.ReasonCode, "原因", 64);
        var description = Optional(request?.Description, "说明", 500);
        var now = DateTime.Now;

        db.Insertable(new EmDiControls
        {
            Nid = Guid.NewGuid().ToString(),
            EquipmentCode = equipmentCode,
            ChannelCode = channelCode,
            Cmd = cmd,
            ReasonCode = reasonCode,
            Description = description,
            OperationId = HeartBeatService.IdleOperationId,
            OperationStatus = 0,
            DeleteFlag = 0,
        }).ExecuteCommand();

        var updated = db.Ado.ExecuteCommand(
            """
            UPDATE EQUIPMENT_DI_FLAGS
            SET OPERATION_ID = @idle,
                OPERATION_STATUS = 0,
                EQUIPMENT_CONTROL = EQUIPMENT_CONTROL + 1,
                MODIFY_TIME = @now
            WHERE EQUIPMENT_CODE = @equipmentCode AND DELETE_FLAG = 0
            """,
            new
            {
                idle = HeartBeatService.IdleOperationId,
                now,
                equipmentCode,
            });

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
                OperationTime = now,
            }).ExecuteCommand();
        }

        return new EnqueueControlResult
        {
            EquipmentCode = equipmentCode,
            ChannelCode = channelCode,
            Cmd = cmd,
            ReasonCode = reasonCode,
            Description = description,
            Message = "已入队。下一拍心跳会把这条控机带走，凭证会从空闲号换成新的回执。",
        };
    }

    private static string Require(string? value, string label, int max)
    {
        var text = value?.Trim() ?? "";
        if (text.Length == 0)
        {
            throw new AppException(400, $"请填写{label}");
        }

        if (text.Length > max)
        {
            throw new AppException(400, $"{label}不超过 {max} 个字");
        }

        return text;
    }

    private static string Optional(string? value, string label, int max)
    {
        var text = value?.Trim() ?? "";
        if (text.Length > max)
        {
            throw new AppException(400, $"{label}不超过 {max} 个字");
        }

        return text;
    }
}
