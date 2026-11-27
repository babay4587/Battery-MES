using SqlSugar;

namespace BatteryMes.Api.Equipment;

[SugarTable("EAM_DEV")]
public sealed class EamDev
{
    [SugarColumn(ColumnName = "NID", IsPrimaryKey = true)]
    public string Nid { get; set; } = "";

    [SugarColumn(ColumnName = "DEV_CODE")]
    public string? DevCode { get; set; }

    [SugarColumn(ColumnName = "DEV_STATUS")]
    public string? DevStatus { get; set; }

    [SugarColumn(ColumnName = "DELETE_FLAG")]
    public int DeleteFlag { get; set; }
}

[SugarTable("EAM_EC_EQUIP_STATUS")]
public sealed class EamEcEquipStatus
{
    [SugarColumn(ColumnName = "NID", IsPrimaryKey = true)]
    public string Nid { get; set; } = "";

    [SugarColumn(ColumnName = "DELETE_FLAG")]
    public int DeleteFlag { get; set; }

    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime? CreateTime { get; set; }

    [SugarColumn(ColumnName = "EQUIPMENT_CODE")]
    public string? EquipmentCode { get; set; }

    [SugarColumn(ColumnName = "EMPLOYEE_NO")]
    public string? EmployeeNo { get; set; }

    [SugarColumn(ColumnName = "EQUIPMENT_MODEL")]
    public string? EquipmentModel { get; set; }

    [SugarColumn(ColumnName = "EQUIPMENT_STATUS_ID")]
    public string? EquipmentStatusId { get; set; }

    [SugarColumn(ColumnName = "REASON_CODE")]
    public string? ReasonCode { get; set; }

    [SugarColumn(ColumnName = "DESCRIPTION")]
    public string? Description { get; set; }

    [SugarColumn(ColumnName = "MODIFY_TIME")]
    public DateTime? ModifyTime { get; set; }
}

[SugarTable("EAM_OEE_DOWNTIME_REACORD")]
public sealed class EamOeeDowntimeRecord
{
    [SugarColumn(ColumnName = "ID", IsPrimaryKey = true)]
    public string Id { get; set; } = "";

    [SugarColumn(ColumnName = "DELETE_FLAG")]
    public int DeleteFlag { get; set; }

    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime? CreateTime { get; set; }

    [SugarColumn(ColumnName = "DEV_CODE")]
    public string? DevCode { get; set; }

    [SugarColumn(ColumnName = "START_TIME")]
    public DateTime? StartTime { get; set; }

    [SugarColumn(ColumnName = "END_TIME")]
    public DateTime? EndTime { get; set; }

    [SugarColumn(ColumnName = "STATUS")]
    public int Status { get; set; }

    [SugarColumn(ColumnName = "CREATE_BY")]
    public string? CreateBy { get; set; }

    [SugarColumn(ColumnName = "FIELD1")]
    public string? Field1 { get; set; }

    [SugarColumn(ColumnName = "LEVEL1_LOSS_CODE")]
    public string? Level1LossCode { get; set; }

    [SugarColumn(ColumnName = "LEVEL2_LOSS_CODE")]
    public string? Level2LossCode { get; set; }

    [SugarColumn(ColumnName = "LEVEL3_LOSS_CODE")]
    public string? Level3LossCode { get; set; }

    [SugarColumn(ColumnName = "LEVEL4_LOSS_CODE")]
    public string? Level4LossCode { get; set; }
}

[SugarTable("EAM_FAULT_RECORD")]
public sealed class EamFaultRecord
{
    [SugarColumn(ColumnName = "ID", IsPrimaryKey = true)]
    public string Id { get; set; } = "";

    [SugarColumn(ColumnName = "CREATE_BY")]
    public string? CreateBy { get; set; }

    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime? CreateTime { get; set; }

    [SugarColumn(ColumnName = "DELETE_FLAG")]
    public int DeleteFlag { get; set; }

    [SugarColumn(ColumnName = "STATUS")]
    public int Status { get; set; }

    [SugarColumn(ColumnName = "START_TIME")]
    public DateTime? StartTime { get; set; }

    [SugarColumn(ColumnName = "END_TIME")]
    public DateTime? EndTime { get; set; }

    [SugarColumn(ColumnName = "DEV_CODE")]
    public string? DevCode { get; set; }

    [SugarColumn(ColumnName = "FAULT_CODE")]
    public string? FaultCode { get; set; }

    [SugarColumn(ColumnName = "FAULT_DESC")]
    public string? FaultDesc { get; set; }

    [SugarColumn(ColumnName = "FAULT_LEVEL")]
    public string? FaultLevel { get; set; }

    [SugarColumn(ColumnName = "MODIFY_TIME")]
    public DateTime? ModifyTime { get; set; }

    [SugarColumn(ColumnName = "MODIFY_BY")]
    public string? ModifyBy { get; set; }
}

[SugarTable("PRD_EQP_PARA_HIS")]
public sealed class PrdEqpParaHis
{
    [SugarColumn(ColumnName = "NID", IsPrimaryKey = true)]
    public string Nid { get; set; } = "";

    [SugarColumn(ColumnName = "CREATE_BY")]
    public string? CreateBy { get; set; }

    [SugarColumn(ColumnName = "CREATE_TIME")]
    public DateTime? CreateTime { get; set; }

    [SugarColumn(ColumnName = "DELETE_FLAG")]
    public int DeleteFlag { get; set; }

    [SugarColumn(ColumnName = "EQUIPMENT_CODE")]
    public string? EquipmentCode { get; set; }

    [SugarColumn(ColumnName = "STEP_ID")]
    public string? StepId { get; set; }

    [SugarColumn(ColumnName = "ORDER_NO")]
    public string? OrderNo { get; set; }

    [SugarColumn(ColumnName = "MATERIAL_CODE")]
    public string? MaterialCode { get; set; }

    [SugarColumn(ColumnName = "OPERATION_CODE")]
    public string? OperationCode { get; set; }

    [SugarColumn(ColumnName = "PARAM_CODE")]
    public string? ParamCode { get; set; }

    [SugarColumn(ColumnName = "PARAM_NAME")]
    public string? ParamName { get; set; }

    [SugarColumn(ColumnName = "COLLECTED_VALUE")]
    public string? CollectedValue { get; set; }

    [SugarColumn(ColumnName = "TARGET_VALUE")]
    public string? TargetValue { get; set; }
}
