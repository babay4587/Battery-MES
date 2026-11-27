using BatteryMes.Api.Equipment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BatteryMes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/equipment-bench")]
public sealed class EquipmentBenchController(EquipmentStatusService equipment) : ControllerBase
{
    [HttpGet("snapshot")]
    public ActionResult<object> Snapshot([FromQuery] string? equipmentCode, [FromQuery] string? topic)
    {
        return new JsonResult(equipment.Snapshot(equipmentCode, topic), HeartbeatJson.Options);
    }

    [HttpPost("seed-material-load")]
    public ActionResult<object> SeedMaterialLoad([FromBody] SeedMaterialLoadRequest? request)
    {
        return new JsonResult(equipment.SeedOpenMaterialLoad(request?.EquipmentCode), HeartbeatJson.Options);
    }
}
