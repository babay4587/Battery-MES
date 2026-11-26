using SqlSugar;

namespace BatteryMes.Api.Data;

[SugarTable("SYS_USER")]
public class SysUser
{
    [SugarColumn(ColumnName = "USER_ID", IsPrimaryKey = true)]
    public string UserId { get; set; } = "";

    [SugarColumn(ColumnName = "LOGIN_NAME")]
    public string LoginName { get; set; } = "";

    [SugarColumn(ColumnName = "DISPLAY_NAME")]
    public string DisplayName { get; set; } = "";

    [SugarColumn(ColumnName = "PASSWORD_HASH")]
    public string PasswordHash { get; set; } = "";

    [SugarColumn(ColumnName = "IS_ENABLED")]
    public int IsEnabled { get; set; }

    [SugarColumn(ColumnName = "CREATE_ON")]
    public DateTime CreateOn { get; set; }
}

[SugarTable("SYS_ROLE")]
public class SysRole
{
    [SugarColumn(ColumnName = "ROLE_ID", IsPrimaryKey = true)]
    public string RoleId { get; set; } = "";

    [SugarColumn(ColumnName = "ROLE_CODE")]
    public string RoleCode { get; set; } = "";

    [SugarColumn(ColumnName = "ROLE_NAME")]
    public string RoleName { get; set; } = "";
}

[SugarTable("SYS_USER_ROLE")]
public class SysUserRole
{
    [SugarColumn(ColumnName = "USER_ID", IsPrimaryKey = true)]
    public string UserId { get; set; } = "";

    [SugarColumn(ColumnName = "ROLE_ID", IsPrimaryKey = true)]
    public string RoleId { get; set; } = "";
}

[SugarTable("FM_LEVEL")]
public class FmLevel
{
    [SugarColumn(ColumnName = "NID", IsPrimaryKey = true)]
    public string Nid { get; set; } = "";

    [SugarColumn(ColumnName = "NAME")]
    public string Name { get; set; } = "";

    [SugarColumn(ColumnName = "DESCRIPTION", IsNullable = true)]
    public string? Description { get; set; }

    [SugarColumn(ColumnName = "LEVEL")]
    public int Level { get; set; }
}

[SugarTable("FM_NODE")]
public class FmNode
{
    [SugarColumn(ColumnName = "NID", IsPrimaryKey = true)]
    public string Nid { get; set; } = "";

    [SugarColumn(ColumnName = "NAME")]
    public string Name { get; set; } = "";

    [SugarColumn(ColumnName = "DESCRIPTION", IsNullable = true)]
    public string? Description { get; set; }

    [SugarColumn(ColumnName = "LEVEL_NID")]
    public string LevelNid { get; set; } = "";

    [SugarColumn(ColumnName = "PNID", IsNullable = true)]
    public string? Pnid { get; set; }

    [SugarColumn(ColumnName = "SORT_NO")]
    public int SortNo { get; set; }

    [SugarColumn(ColumnName = "IS_DELETED")]
    public int IsDeleted { get; set; }

    [SugarColumn(ColumnName = "CREATE_ON")]
    public DateTime CreateOn { get; set; }

    [SugarColumn(ColumnName = "LAST_UPDATE_ON")]
    public DateTime LastUpdateOn { get; set; }
}
