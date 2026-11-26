using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BatteryMes.Api.Equipment;

public static class HeartbeatJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}

public sealed class HeartBeatRequest
{
    public string? Software { get; set; }
    public List<HeartBeatEquipment>? EquipmentInfo { get; set; }
}

public sealed class HeartBeatEquipment
{
    public string? EquipmentCode { get; set; }
}

public sealed class HeartBeatResponse
{
    public string? ServerDateTime { get; set; }
    public List<EquipmentHeartbeat> EquipmentInfo { get; set; } = [];
}

public sealed class EquipmentHeartbeat
{
    public string? EquipmentCode { get; set; }
    public bool ResultFlag { get; set; }
    public string? Message { get; set; }
    public string? ReceiptNumber { get; set; }
    public int EqptCtrlFlag { get; set; }
    public List<EqptCtrlParam>? EqptCtrlParam { get; set; }
    public int MaterialLoadFlag { get; set; }
    public List<MaterialLoadParam>? MaterialLoadParam { get; set; }
    public int MaterialUnloadFlag { get; set; }
    public List<MaterialUnloadParam>? MaterialUnloadParam { get; set; }
    public int PartLoadFlag { get; set; }
    public List<PartLoadParam>? PartLoadParam { get; set; }
    public int PartUnLoadFlag { get; set; }
    public List<PartUnLoadParam>? PartUnLoadParam { get; set; }
    public int EqptGetParaFlag { get; set; }
    public int EqptGetBatchFlag { get; set; }
    public int RecipeGetFlag { get; set; }
    public int EQReasonCodeFlag { get; set; }
    public int BatchDelFlag { get; set; }
    public List<BatchDelParam>? BatchDelParam { get; set; }
    public int OrderSendFlag { get; set; }
    public List<OrderSendParam>? OrderSendParam { get; set; }
    public int OrderDelFlag { get; set; }
    public List<OrderDelParam>? OrderDelParam { get; set; }
    public int BatteryCapacityRuleGet { get; set; }
    public int CapacityParameterGet { get; set; }
    public int ChannelCtrlFlag { get; set; }
    public List<ChannelCtrlParam>? ChannelCtrlParam { get; set; }
}

public sealed class EqptCtrlParam
{
    public string? ChannelCode { get; set; }
    public string? Cmd { get; set; }
    public string? ReasonCode { get; set; }
    public string? Description { get; set; }
}

public sealed class MaterialLoadParam
{
    public string? LabelNo { get; set; }
    public string? Location { get; set; }
    public string? Quantity { get; set; }
    public string? UomCode { get; set; }
    public string? BasicUom { get; set; }
    public string? UomCoefficient { get; set; }
}

public sealed class MaterialUnloadParam
{
    public string? LabelNo { get; set; }
    public string? Location { get; set; }
}

public sealed class PartLoadParam
{
    public string? SpartID { get; set; }
    public string? PartDesc { get; set; }
    public string? Location { get; set; }
    public string? UseLifetime { get; set; }
    public string? WarningLife { get; set; }
}

public sealed class PartUnLoadParam
{
    public string? SpartID { get; set; }
    public string? Location { get; set; }
}

public sealed class BatchDelParam
{
    public string? CreateBatchNo { get; set; }
}

public sealed class OrderSendParam
{
    public string? OrderNo { get; set; }
    public string? WorkOrderNo { get; set; }
    public string? WorkOrderType { get; set; }
    public string? ProductNo { get; set; }
    public string? ProductName { get; set; }
    public string? Polarity { get; set; }
    public string? Sort { get; set; }
    public string? ProcessRouteNo { get; set; }
    public string? VersionNo { get; set; }
    public string? ContainerNo { get; set; }
    public string? PlanQuantity { get; set; }
    public string? PlanFeedQuantity { get; set; }
    public string? WorkOrderCreateTime { get; set; }
}

public sealed class OrderDelParam
{
    public string? WorkOrderNo { get; set; }
    public int Status { get; set; }
}

public sealed class ChannelCtrlParam
{
    public string? ChannelCode { get; set; }
    public string? SlotID { get; set; }
    public string? OPFlag { get; set; }
}
