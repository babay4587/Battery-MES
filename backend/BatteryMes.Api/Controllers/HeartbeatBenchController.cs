using BatteryMes.Api.Equipment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BatteryMes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/heartbeat-bench")]
public sealed class HeartbeatBenchController(HeartbeatBenchService bench) : ControllerBase
{
    [HttpPost("controls")]
    public ActionResult<EnqueueControlResult> EnqueueControl([FromBody] EnqueueControlRequest? request)
    {
        return bench.EnqueueControl(request);
    }
}
