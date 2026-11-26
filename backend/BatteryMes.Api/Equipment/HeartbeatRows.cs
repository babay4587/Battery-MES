using SqlSugar;

namespace BatteryMes.Api.Equipment;

public class HeartbeatQueueRow
{
    [SugarColumn(ColumnName = "NID", IsPrimaryKey = true)]
    public string Nid { get; set; } = "";

    [SugarColumn(ColumnName = "EQUIPMENT_CODE")]
    public string? EquipmentCode { get; set; }

    [SugarColumn(ColumnName = "OPERATION_ID")]
    public string? OperationId { get; set; }

    [SugarColumn(ColumnName = "OPERATION_STATUS")]
    public int OperationStatus { get; set; }

    [SugarColumn(ColumnName = "DELETE_FLAG")]
    public int DeleteFlag { get; set; }
}

[SugarTable("EQUIPMENT_DI_FLAGS")]
public sealed class EquipmentDiFlags : HeartbeatQueueRow
{
    [SugarColumn(ColumnName = "EQUIPMENT_CONTROL")]
    public int EquipmentControl { get; set; }

    [SugarColumn(ColumnName = "MATERIAL_LOAD")]
    public int MaterialLoad { get; set; }

    [SugarColumn(ColumnName = "MATERIAL_UNLOAD")]
    public int MaterialUnload { get; set; }

    [SugarColumn(ColumnName = "PART_LAOD")]
    public int PartLoad { get; set; }

    [SugarColumn(ColumnName = "PART_UNLAOD")]
    public int PartUnload { get; set; }

    [SugarColumn(ColumnName = "GET_PARA_FLAG")]
    public int GetParaFlag { get; set; }

    [SugarColumn(ColumnName = "GET_BATCH_FLAG")]
    public int GetBatchFlag { get; set; }

    [SugarColumn(ColumnName = "GET_RECIPE_FLAG")]
    public int GetRecipeFlag { get; set; }

    [SugarColumn(ColumnName = "GET_REASON_FLAG")]
    public int GetReasonFlag { get; set; }

    [SugarColumn(ColumnName = "DEL_BATCH_FLAG")]
    public int DeleteBatchFlag { get; set; }

    [SugarColumn(ColumnName = "SEND_ORDER")]
    public int SendOrder { get; set; }

    [SugarColumn(ColumnName = "DEL_ORDER")]
    public int DeleteOrder { get; set; }

    [SugarColumn(ColumnName = "GET_BATTERY_CAPACITY_RULE_FLAG")]
    public int BatteryCapacityRuleFlag { get; set; }

    [SugarColumn(ColumnName = "GET_CAPACITY_PARA_FLAG")]
    public int CapacityParaFlag { get; set; }

    [SugarColumn(ColumnName = "SEND_EQUIPMENT_CHANNEL_FLAG")]
    public int ChannelFlag { get; set; }

    [SugarColumn(ColumnName = "MODIFY_TIME")]
    public DateTime? ModifyTime { get; set; }

    [SugarColumn(ColumnName = "OPERATION_TIME")]
    public DateTime? OperationTime { get; set; }
}

[SugarTable("EM_DI_EXEC_PARAM")]
public sealed class EmDiExecParam : HeartbeatQueueRow
{
    [SugarColumn(ColumnName = "OPERATION_TIME")]
    public DateTime? OperationTime { get; set; }
}

[SugarTable("EM_DI_CONTROLS")]
public sealed class EmDiControls : HeartbeatQueueRow
{
    [SugarColumn(ColumnName = "CHANNEL_CODE")]
    public string? ChannelCode { get; set; }

    [SugarColumn(ColumnName = "CMD")]
    public string? Cmd { get; set; }

    [SugarColumn(ColumnName = "REASON_CODE")]
    public string? ReasonCode { get; set; }

    [SugarColumn(ColumnName = "DESCRIPTION")]
    public string? Description { get; set; }
}

[SugarTable("EM_DI_MATERIAL_LOAD")]
public sealed class EmDiMaterialLoad : HeartbeatQueueRow
{
    [SugarColumn(ColumnName = "LABEL_NO")]
    public string? LabelNo { get; set; }

    [SugarColumn(ColumnName = "LOCATION")]
    public string? Location { get; set; }

    [SugarColumn(ColumnName = "QUANTITY")]
    public string? Quantity { get; set; }

    [SugarColumn(ColumnName = "UOM")]
    public string? Uom { get; set; }

    [SugarColumn(ColumnName = "BASIC_UOM")]
    public string? BasicUom { get; set; }

    [SugarColumn(ColumnName = "UOM_COEFFICIENT")]
    public string? UomCoefficient { get; set; }
}

[SugarTable("EM_DI_MATERIAL_UNLOAD")]
public sealed class EmDiMaterialUnload : HeartbeatQueueRow
{
    [SugarColumn(ColumnName = "LABEL_NO")]
    public string? LabelNo { get; set; }

    [SugarColumn(ColumnName = "LOCATION")]
    public string? Location { get; set; }
}

[SugarTable("EM_DI_PART_LOAD")]
public sealed class EmDiPartLoad : HeartbeatQueueRow
{
    [SugarColumn(ColumnName = "SPART_ID")]
    public string? SpartId { get; set; }

    [SugarColumn(ColumnName = "PART_DESC")]
    public string? PartDesc { get; set; }

    [SugarColumn(ColumnName = "LOCATION")]
    public string? Location { get; set; }

    [SugarColumn(ColumnName = "USE_LIFE_TIME")]
    public string? UseLifetime { get; set; }

    [SugarColumn(ColumnName = "WARNING_LIFE")]
    public string? WarningLife { get; set; }
}

[SugarTable("EM_DI_PART_UNLOAD")]
public sealed class EmDiPartUnload : HeartbeatQueueRow
{
    [SugarColumn(ColumnName = "SPART_ID")]
    public string? SpartId { get; set; }

    [SugarColumn(ColumnName = "LOCATION")]
    public string? Location { get; set; }
}

[SugarTable("EM_DI_BATCH_DELETE")]
public sealed class EmDiBatchDelete : HeartbeatQueueRow
{
    [SugarColumn(ColumnName = "CREATE_BATCH_NO")]
    public string? CreateBatchNo { get; set; }
}

[SugarTable("EM_DI_PACK_ORDER_SEND")]
public sealed class EmDiPackOrderSend : HeartbeatQueueRow
{
    [SugarColumn(ColumnName = "ORDER_NO")]
    public string? OrderNo { get; set; }

    [SugarColumn(ColumnName = "WORK_ORDER_NO")]
    public string? WorkOrderNo { get; set; }

    [SugarColumn(ColumnName = "WORK_ORDER_TYPE")]
    public string? WorkOrderType { get; set; }

    [SugarColumn(ColumnName = "PRODUCT_NO")]
    public string? ProductNo { get; set; }

    [SugarColumn(ColumnName = "PRODUCT_NAME")]
    public string? ProductName { get; set; }

    [SugarColumn(ColumnName = "POLARITY")]
    public string? Polarity { get; set; }

    [SugarColumn(ColumnName = "SEQUENCE")]
    public string? Sequence { get; set; }

    [SugarColumn(ColumnName = "PROCESS_ROUTE_NO")]
    public string? ProcessRouteNo { get; set; }

    [SugarColumn(ColumnName = "VERSION_NO")]
    public string? VersionNo { get; set; }

    [SugarColumn(ColumnName = "CONTAINER_NO")]
    public string? ContainerNo { get; set; }

    [SugarColumn(ColumnName = "PLAN_QUANTITY")]
    public string? PlanQuantity { get; set; }

    [SugarColumn(ColumnName = "PLAN_FEE_QUANTITY")]
    public string? PlanFeedQuantity { get; set; }

    [SugarColumn(ColumnName = "WORK_ORDER_CREATE_TIME")]
    public string? WorkOrderCreateTime { get; set; }
}

[SugarTable("EM_DI_PACK_ORDER_DELETE")]
public sealed class EmDiPackOrderDelete : HeartbeatQueueRow
{
    [SugarColumn(ColumnName = "WORK_ORDER_NO")]
    public string? WorkOrderNo { get; set; }

    [SugarColumn(ColumnName = "STATUS")]
    public int? Status { get; set; }
}

[SugarTable("EM_DI_EQUIP_CHANNEL")]
public sealed class EmDiEquipChannel : HeartbeatQueueRow
{
    [SugarColumn(ColumnName = "CHANNEL_CODE")]
    public string? ChannelCode { get; set; }

    [SugarColumn(ColumnName = "SLOT_ID")]
    public string? SlotId { get; set; }

    [SugarColumn(ColumnName = "OP_FLAG")]
    public string? OpFlag { get; set; }
}

[SugarTable("MESSAGE_IN_MES_EC")]
public sealed class MessageInMesEc
{
    [SugarColumn(ColumnName = "NID", IsPrimaryKey = true)]
    public string Nid { get; set; } = "";

    [SugarColumn(ColumnName = "MESSAGE_ID")]
    public string? MessageId { get; set; }

    [SugarColumn(ColumnName = "MESSAGE_TYPE")]
    public string? MessageType { get; set; }

    [SugarColumn(ColumnName = "CONTENT")]
    public string? Content { get; set; }

    [SugarColumn(ColumnName = "RETURN_CONTENT")]
    public string? ReturnContent { get; set; }

    [SugarColumn(ColumnName = "RECEIVE_TIME")]
    public DateTime? ReceiveTime { get; set; }

    [SugarColumn(ColumnName = "DO_TIME")]
    public DateTime? DoTime { get; set; }

    [SugarColumn(ColumnName = "RETRY_NUM")]
    public int RetryNum { get; set; }

    [SugarColumn(ColumnName = "SYSTEM")]
    public string? System { get; set; }

    [SugarColumn(ColumnName = "DO_STATUS")]
    public int DoStatus { get; set; }

    [SugarColumn(ColumnName = "ERROR_MESSAGE")]
    public string? ErrorMessage { get; set; }
}
