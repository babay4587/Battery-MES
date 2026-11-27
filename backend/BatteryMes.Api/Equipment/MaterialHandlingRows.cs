using SqlSugar;

namespace BatteryMes.Api.Equipment;

[SugarTable("MM_MATERIAL_STOCK_DETAIL")]
public sealed class MmMaterialStockDetail
{
    [SugarColumn(ColumnName = "NID", IsPrimaryKey = true)]
    public string Nid { get; set; } = "";

    [SugarColumn(ColumnName = "MATERIAL_BATCH")]
    public string? MaterialBatch { get; set; }

    [SugarColumn(ColumnName = "MATERIAL_CODE")]
    public string? MaterialCode { get; set; }

    [SugarColumn(ColumnName = "REMAIN_QTY")]
    public decimal RemainQty { get; set; }

    [SugarColumn(ColumnName = "FEED_QTY")]
    public decimal FeedQty { get; set; }

    [SugarColumn(ColumnName = "LOAD_QTY")]
    public decimal LoadQty { get; set; }

    [SugarColumn(ColumnName = "DELETE_FLAG")]
    public int DeleteFlag { get; set; }

    [SugarColumn(ColumnName = "MODIFY_BY")]
    public string? ModifyBy { get; set; }

    [SugarColumn(ColumnName = "MODIFY_TIME")]
    public DateTime? ModifyTime { get; set; }
}

[SugarTable("PRD_RESOURCE_CONTENT")]
public sealed class PrdResourceContent
{
    [SugarColumn(ColumnName = "NID", IsPrimaryKey = true)]
    public string Nid { get; set; } = "";

    [SugarColumn(ColumnName = "EQUIPMENT_CODE")]
    public string? EquipmentCode { get; set; }

    [SugarColumn(ColumnName = "ORDER_NO")]
    public string? OrderNo { get; set; }

    [SugarColumn(ColumnName = "DELETE_FLAG")]
    public int DeleteFlag { get; set; }
}

[SugarTable("POM_ORDER_BOM")]
public sealed class PomOrderBom
{
    [SugarColumn(ColumnName = "NID", IsPrimaryKey = true)]
    public string Nid { get; set; } = "";

    [SugarColumn(ColumnName = "ORDER_NO")]
    public string? OrderNo { get; set; }

    [SugarColumn(ColumnName = "MATERIAL_CODE")]
    public string? MaterialCode { get; set; }

    [SugarColumn(ColumnName = "OPERATION_CODE")]
    public string? OperationCode { get; set; }

    [SugarColumn(ColumnName = "BOM_UOM")]
    public string? BomUom { get; set; }

    [SugarColumn(ColumnName = "DELETE_FLAG")]
    public int DeleteFlag { get; set; }
}

[SugarTable("PRD_RESOURCE_LOADING")]
public sealed class PrdResourceLoading
{
    [SugarColumn(ColumnName = "NID", IsPrimaryKey = true)]
    public string Nid { get; set; } = "";

    [SugarColumn(ColumnName = "EQUIPMENT_CODE")]
    public string? EquipmentCode { get; set; }

    [SugarColumn(ColumnName = "MATERIAL_CODE")]
    public string? MaterialCode { get; set; }

    [SugarColumn(ColumnName = "MATERIAL_SEQ_NO")]
    public long MaterialSeqNo { get; set; }

    [SugarColumn(ColumnName = "ORDER_NO")]
    public string? OrderNo { get; set; }

    [SugarColumn(ColumnName = "QUANTITY")]
    public decimal Quantity { get; set; }

    [SugarColumn(ColumnName = "TOTAL_QUANTITY")]
    public decimal TotalQuantity { get; set; }

    [SugarColumn(ColumnName = "MATERIAL_BATCH")]
    public string? MaterialBatch { get; set; }

    [SugarColumn(ColumnName = "FEED_LOCATION")]
    public string? FeedLocation { get; set; }

    [SugarColumn(ColumnName = "CONSUMED_QTY")]
    public decimal ConsumedQty { get; set; }

    [SugarColumn(ColumnName = "UOM")]
    public string? Uom { get; set; }

    [SugarColumn(ColumnName = "DELETE_FLAG")]
    public int DeleteFlag { get; set; }

    [SugarColumn(ColumnName = "ARCHIVE_FLAG")]
    public int ArchiveFlag { get; set; }

    [SugarColumn(ColumnName = "CREATE_BY")]
    public string? CreateBy { get; set; }

    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime? CreateTime { get; set; }
}

[SugarTable("PRD_RESOURCE_LOADING_HIS")]
public sealed class PrdResourceLoadingHis
{
    [SugarColumn(ColumnName = "NID", IsPrimaryKey = true)]
    public string Nid { get; set; } = "";

    [SugarColumn(ColumnName = "EQUIPMENT_CODE")]
    public string? EquipmentCode { get; set; }

    [SugarColumn(ColumnName = "MATERIAL_CODE")]
    public string? MaterialCode { get; set; }

    [SugarColumn(ColumnName = "ORDER_NO")]
    public string? OrderNo { get; set; }

    [SugarColumn(ColumnName = "QUANTITY")]
    public decimal Quantity { get; set; }

    [SugarColumn(ColumnName = "MATERIAL_BATCH")]
    public string? MaterialBatch { get; set; }

    [SugarColumn(ColumnName = "CONSUMED_QTY")]
    public decimal ConsumedQty { get; set; }

    [SugarColumn(ColumnName = "TYPE")]
    public int Type { get; set; }

    [SugarColumn(ColumnName = "DELETE_FLAG")]
    public int DeleteFlag { get; set; }

    [SugarColumn(ColumnName = "CREATE_BY")]
    public string? CreateBy { get; set; }

    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime? CreateTime { get; set; }
}

[SugarTable("MM_MATERIAL_STOCK_LOG")]
public sealed class MmMaterialStockLog
{
    [SugarColumn(ColumnName = "NID", IsPrimaryKey = true)]
    public string Nid { get; set; } = "";

    [SugarColumn(ColumnName = "TYPE")]
    public string? Type { get; set; }

    [SugarColumn(ColumnName = "LOG_QTY")]
    public decimal LogQty { get; set; }

    [SugarColumn(ColumnName = "DELETE_FLAG")]
    public int DeleteFlag { get; set; }

    [SugarColumn(ColumnName = "MATERIAL_BATCH")]
    public string? MaterialBatch { get; set; }

    [SugarColumn(ColumnName = "MATERIAL_CODE")]
    public string? MaterialCode { get; set; }

    [SugarColumn(ColumnName = "ARCHIVE_FLAG")]
    public int ArchiveFlag { get; set; }

    [SugarColumn(ColumnName = "SEND_COUNT")]
    public int SendCount { get; set; }
}

[SugarTable("EAM_LIFE_MANAGEMENT")]
public sealed class EamLifeManagement
{
    [SugarColumn(ColumnName = "ID", IsPrimaryKey = true)]
    public string Id { get; set; } = "";

    [SugarColumn(ColumnName = "DEV_ID")]
    public string? DevId { get; set; }

    [SugarColumn(ColumnName = "PART_CODE")]
    public string? PartCode { get; set; }

    [SugarColumn(ColumnName = "PART_NAME")]
    public string? PartName { get; set; }

    [SugarColumn(ColumnName = "ALERT_LIFE")]
    public decimal? AlertLife { get; set; }

    [SugarColumn(ColumnName = "LEFT_LIFE")]
    public decimal? LeftLife { get; set; }

    [SugarColumn(ColumnName = "LIFE_UNIT")]
    public string? LifeUnit { get; set; }

    [SugarColumn(ColumnName = "DELETE_FLAG")]
    public int DeleteFlag { get; set; }

    [SugarColumn(ColumnName = "MODIFY_BY")]
    public string? ModifyBy { get; set; }

    [SugarColumn(ColumnName = "MODIFY_TIME")]
    public DateTime? ModifyTime { get; set; }
}

[SugarTable("EAM_LIFE_RECORD")]
public sealed class EamLifeRecord
{
    [SugarColumn(ColumnName = "ID", IsPrimaryKey = true)]
    public string Id { get; set; } = "";

    [SugarColumn(ColumnName = "DEV_ID")]
    public string? DevId { get; set; }

    [SugarColumn(ColumnName = "PARENT_ID")]
    public string? ParentId { get; set; }

    [SugarColumn(ColumnName = "ALERT_LIFE")]
    public decimal? AlertLife { get; set; }

    [SugarColumn(ColumnName = "LEFT_LIFE_BEFORE")]
    public decimal? LeftLifeBefore { get; set; }

    [SugarColumn(ColumnName = "LEFT_LIFE_AFTER")]
    public decimal? LeftLifeAfter { get; set; }

    [SugarColumn(ColumnName = "LIFE_UNIT")]
    public string? LifeUnit { get; set; }

    [SugarColumn(ColumnName = "OP_TYPE")]
    public int OpType { get; set; }

    [SugarColumn(ColumnName = "OP_REASON")]
    public string? OpReason { get; set; }

    [SugarColumn(ColumnName = "CREATE_BY")]
    public string? CreateBy { get; set; }

    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime? CreateTime { get; set; }
}
