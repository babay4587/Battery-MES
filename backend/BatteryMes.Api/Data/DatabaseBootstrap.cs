using BatteryMes.Api.Auth;
using MySqlConnector;
using SqlSugar;

namespace BatteryMes.Api.Data;

public static class DatabaseBootstrap
{
    private static readonly (string Nid, string Name, int Level)[] Levels =
    [
        ("ENTERPRISE", "企业", 1),
        ("SITE", "基地", 2),
        ("WORKSHOP", "车间", 3),
        ("SECTION", "工段", 4),
        ("EQUIPMENT", "设备", 5),
        ("STATION", "工位", 6),
    ];

    public static void Ensure(ISqlSugarClient db, IConfiguration configuration, ILogger logger)
    {
        var connectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("缺少 ConnectionStrings:MySql");
        CreateDatabase(connectionString);

        var sqlDirectory = Path.Combine(AppContext.BaseDirectory, "Sql");
        if (!Directory.Exists(sqlDirectory))
        {
            throw new InvalidOperationException($"找不到建表脚本目录 {sqlDirectory}");
        }

        foreach (var sqlPath in Directory.GetFiles(sqlDirectory, "*.sql").OrderBy(Path.GetFileName, StringComparer.Ordinal))
        {
            foreach (var statement in SplitStatements(File.ReadAllText(sqlPath)))
            {
                db.Ado.ExecuteCommand(statement);
            }
        }

        SeedLevels(db);
        SeedAdmin(db, configuration, logger);
        logger.LogInformation("MySQL 已就绪，登录表和工厂模型表已检查");
    }

    private static void CreateDatabase(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);
        var database = builder.Database;
        if (string.IsNullOrWhiteSpace(database))
        {
            throw new InvalidOperationException("连接串里缺少 Database");
        }

        builder.Database = "";
        using var connection = new MySqlConnection(builder.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE IF NOT EXISTS `{database}` CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci";
        command.ExecuteNonQuery();
    }

    private static void SeedLevels(ISqlSugarClient db)
    {
        foreach (var (nid, name, level) in Levels)
        {
            if (db.Queryable<FmLevel>().Any(item => item.Nid == nid))
            {
                continue;
            }

            db.Insertable(new FmLevel
            {
                Nid = nid,
                Name = name,
                Description = name,
                Level = level,
            }).ExecuteCommand();
        }
    }

    private static void SeedAdmin(ISqlSugarClient db, IConfiguration configuration, ILogger logger)
    {
        const string roleId = "role-mes-admin";
        if (!db.Queryable<SysRole>().Any(item => item.RoleId == roleId))
        {
            db.Insertable(new SysRole
            {
                RoleId = roleId,
                RoleCode = "MES_ADMIN",
                RoleName = "系统管理员",
            }).ExecuteCommand();
        }

        var loginName = configuration["Seed:AdminLoginName"] ?? "admin";
        if (db.Queryable<SysUser>().Any(item => item.LoginName == loginName))
        {
            return;
        }

        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("没有配置 Seed:AdminPassword，跳过初始管理员");
            return;
        }

        var userId = "user-admin";
        db.Insertable(new SysUser
        {
            UserId = userId,
            LoginName = loginName,
            DisplayName = configuration["Seed:AdminDisplayName"] ?? "系统管理员",
            PasswordHash = PasswordHasher.Hash(password),
            IsEnabled = 1,
            CreateOn = DateTime.Now,
        }).ExecuteCommand();
        db.Insertable(new SysUserRole { UserId = userId, RoleId = roleId }).ExecuteCommand();
        logger.LogInformation("已创建初始管理员 {LoginName}", loginName);
    }

    private static IEnumerable<string> SplitStatements(string script)
    {
        var lines = script.Split('\n').Select(line =>
        {
            var trimmed = line.Trim();
            return trimmed.StartsWith("--", StringComparison.Ordinal) ? "" : line;
        });
        var withoutComments = string.Join('\n', lines);
        return withoutComments
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(statement => statement.Length > 0);
    }
}
