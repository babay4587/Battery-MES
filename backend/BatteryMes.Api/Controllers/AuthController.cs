using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BatteryMes.Api.Auth;
using BatteryMes.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace BatteryMes.Api.Controllers;

public sealed record LoginRequest(string LoginName, string Password);

public sealed record LoginResponse(string Token, string DisplayName, string LoginName, IReadOnlyList<string> Roles);

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISqlSugarClient db, JwtTokenService tokens) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var loginName = request.LoginName?.Trim() ?? "";
        var user = db.Queryable<SysUser>().Where(item => item.LoginName == loginName).Take(1).ToList().FirstOrDefault();
        if (user is null || user.IsEnabled != 1 || !PasswordHasher.Verify(request.Password ?? "", user.PasswordHash))
        {
            throw new AppException(401, "账号或密码不正确");
        }

        var roleIds = db.Queryable<SysUserRole>().Where(item => item.UserId == user.UserId).Select(item => item.RoleId).ToList();
        var roles = roleIds.Count == 0
            ? []
            : db.Queryable<SysRole>().Where(item => roleIds.Contains(item.RoleId)).Select(item => item.RoleCode).ToList();
        var token = tokens.CreateToken(user.UserId, user.LoginName, user.DisplayName, roles);
        return new LoginResponse(token, user.DisplayName, user.LoginName, roles);
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<LoginResponse> Me()
    {
        var loginName = User.FindFirstValue(JwtRegisteredClaimNames.UniqueName) ?? "";
        var displayName = User.Identity?.Name ?? loginName;
        var roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToList();
        return new LoginResponse("", displayName, loginName, roles);
    }
}
