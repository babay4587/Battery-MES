using System.Text;
using BatteryMes.Api;
using Microsoft.Extensions.FileProviders;
using BatteryMes.Api.Auth;
using BatteryMes.Api.Data;
using BatteryMes.Api.Equipment;
using BatteryMes.Api.Factory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using SqlSugar;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MySql");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("缺少 ConnectionStrings:MySql");
}

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Jwt:Key 至少需要 32 个字节");
}

builder.Services.AddSingleton<ISqlSugarClient>(_ => new SqlSugarClient(new ConnectionConfig
{
    ConnectionString = connectionString,
    DbType = DbType.MySql,
    IsAutoCloseConnection = true,
    InitKeyType = InitKeyType.Attribute,
}));
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<FactoryModelService>();
builder.Services.AddScoped<HeartBeatService>();
builder.Services.AddScoped<HeartbeatBenchService>();
builder.Services.AddScoped<EquipmentStatusService>();
builder.Services.AddScoped<MaterialHandlingService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    DatabaseBootstrap.Ensure(
        scope.ServiceProvider.GetRequiredService<ISqlSugarClient>(),
        app.Configuration,
        app.Logger);
}

app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var status = error is AppException appError ? appError.StatusCode : StatusCodes.Status500InternalServerError;
        if (status == StatusCodes.Status500InternalServerError)
        {
            app.Logger.LogError(error, "未处理的异常");
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            message = status == StatusCodes.Status500InternalServerError ? "服务器内部错误" : error?.Message ?? "请求失败",
        });
    });
});
app.UseCors("frontend");

var frontendDist = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", "frontend", "dist"));
PhysicalFileProvider? frontendFiles = null;
if (Directory.Exists(frontendDist))
{
    frontendFiles = new PhysicalFileProvider(frontendDist);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = frontendFiles });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = frontendFiles });
}
else
{
    app.Logger.LogWarning("没有找到前端构建目录 {FrontendDist}，根地址暂不提供页面", frontendDist);
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
if (frontendFiles is not null)
{
    app.MapFallback(async context =>
    {
        var path = context.Request.Path;
        if (path.StartsWithSegments("/api") || path.StartsWithSegments("/health"))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync(Path.Combine(frontendDist, "index.html"));
    });
}

app.Run();
