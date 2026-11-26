using BatteryMes.Api.Equipment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BatteryMes.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/CZ2Mes")]
public sealed class CZ2MesController(HeartBeatService heartbeats) : ControllerBase
{
    [HttpPost("HeartBeat")]
    public IActionResult HeartBeat([FromBody] HeartBeatRequest? request)
    {
        return new JsonResult(heartbeats.Handle(request), HeartbeatJson.Options);
    }
}
