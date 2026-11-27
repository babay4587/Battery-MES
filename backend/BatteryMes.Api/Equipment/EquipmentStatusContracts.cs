namespace BatteryMes.Api.Equipment;

public sealed class EquipStatusRequest
{
    public string? Software { get; set; }
    public List<EquipStatusEquipment>? EquipmentInfo { get; set; }
}

public sealed class EquipStatusEquipment
{
    public string? EquipmentCode { get; set; }
    public string? EmployeeNo { get; set; }
    public string? EquipmentModel { get; set; }
    public string? EquipmentStatusID { get; set; }
    public string? ReasonCode { get; set; }
    public string? Description { get; set; }
    public string? ModifyTime { get; set; }
}

public sealed class EquipAlertRequest
{
    public string? Software { get; set; }
    public List<EquipAlertEquipment>? EquipmentInfo { get; set; }
}

public sealed class EquipAlertEquipment
{
    public string? EquipmentCode { get; set; }
    public string? EmployeeNo { get; set; }
    public List<AlertInfoItem>? AlertInfo { get; set; }
}

public sealed class AlertInfoItem
{
    public string? AlertCode { get; set; }
    public int AlertReset { get; set; }
    public string? AlertTime { get; set; }
    public string? AlertDescription { get; set; }
    public string? AlertLevel { get; set; }
}

public sealed class EquipParaChangeRequest
{
    public string? Software { get; set; }
    public List<EquipParaChangeEquipment>? EquipmentInfo { get; set; }
}

public sealed class EquipParaChangeEquipment
{
    public string? EquipmentCode { get; set; }
    public string? EmployeeNo { get; set; }
    public string? ProductNo { get; set; }
    public string? OprSequenceNo { get; set; }
    public string? ProcessID { get; set; }
    public string? Version { get; set; }
    public List<ParameterInfoItem>? ParameterInfo { get; set; }
}

public sealed class ParameterInfoItem
{
    public string? StepID { get; set; }
    public string? ParameterCode { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Description { get; set; }
}

public sealed class EquipRunRequest
{
    public string? Software { get; set; }
    public List<EquipRunEquipment>? EquipmentInfo { get; set; }
}

public sealed class EquipRunEquipment
{
    public string? EquipmentCode { get; set; }
    public string? EmployeeNo { get; set; }
}

public sealed class EqptCtrlRequest
{
    public string? Software { get; set; }
    public List<EqptCtrlEquipment>? EquipmentInfo { get; set; }
}

public sealed class EqptCtrlEquipment
{
    public string? EquipmentCode { get; set; }
    public string? ChannelCode { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? Cmd { get; set; }
    public string? ReasonCode { get; set; }
    public string? Description { get; set; }
    public bool ResultFlag { get; set; }
    public string? Message { get; set; }
}

public sealed class ChannelCtrlFeedbackRequest
{
    public string? Software { get; set; }
    public List<ChannelCtrlFeedbackEquipment>? EquipmentInfo { get; set; }
}

public sealed class ChannelCtrlFeedbackEquipment
{
    public string? EquipmentCode { get; set; }
    public string? ChannelCode { get; set; }
    public bool ResultFlag { get; set; }
    public string? Message { get; set; }
    public string? ReceiptNumber { get; set; }
    public List<ChannelCtrlSlot>? OrderSendParam { get; set; }
}

public sealed class ChannelCtrlSlot
{
    public string? SlotID { get; set; }
    public string? OPFlag { get; set; }
}

public sealed class EquipControlItem
{
    public string? EquipmentCode { get; set; }
    public string? Cmd { get; set; }
    public string? ReasonCode { get; set; }
    public string? Description { get; set; }
}

public sealed class EquipmentInfoResult
{
    public string? EquipmentCode { get; set; }
    public bool ResultFlag { get; set; }
    public string? Message { get; set; }
}

public sealed class EquipmentInfoListResponse
{
    public List<EquipmentInfoResult> EquipmentInfo { get; set; } = [];
}

public sealed class EqptCtrlResponse
{
    public List<EquipmentInfoResult> EquipmentInfo { get; set; } = [];
}

public sealed class CommandResult
{
    public bool successed { get; set; }
    public int ErrorCode { get; set; }
    public string Message { get; set; } = "";
    public string ActionCode { get; set; } = "";
    public object? data { get; set; }
}

public sealed class SeedMaterialLoadRequest
{
    public string? EquipmentCode { get; set; }
}
