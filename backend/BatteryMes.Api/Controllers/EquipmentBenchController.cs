using BatteryMes.Api.Equipment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BatteryMes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/equipment-bench")]
public sealed class EquipmentBenchController(EquipmentStatusService equipment, MaterialHandlingService material) : ControllerBase
{
    [HttpGet("snapshot")]
    public ActionResult<object> Snapshot([FromQuery] string? equipmentCode, [FromQuery] string? topic)
    {
        return new JsonResult(material.TrySnapshot(equipmentCode, topic) ?? equipment.Snapshot(equipmentCode, topic), HeartbeatJson.Options);
    }

    [HttpPost("seed-material-load")]
    public ActionResult<object> SeedMaterialLoad([FromBody] SeedMaterialLoadRequest? request)
    {
        return new JsonResult(equipment.SeedOpenMaterialLoad(request?.EquipmentCode), HeartbeatJson.Options);
    }

    [HttpPost("seed-material-load-feedback")]
    public ActionResult<object> SeedMaterialLoadFeedback([FromBody] SeedReceiptRequest? request)
    {
        return new JsonResult(material.SeedMaterialLoad(request?.EquipmentCode, request?.ReceiptNumber), HeartbeatJson.Options);
    }

    [HttpPost("seed-material-unload")]
    public ActionResult<object> SeedMaterialUnload([FromBody] SeedReceiptRequest? request)
    {
        return new JsonResult(material.SeedMaterialUnload(request?.EquipmentCode, request?.ReceiptNumber), HeartbeatJson.Options);
    }

    [HttpPost("seed-part-load")]
    public ActionResult<object> SeedPartLoad([FromBody] SeedReceiptRequest? request)
    {
        return new JsonResult(material.SeedPartLoad(request?.EquipmentCode, request?.ReceiptNumber), HeartbeatJson.Options);
    }

    [HttpPost("seed-part-unload")]
    public ActionResult<object> SeedPartUnload([FromBody] SeedReceiptRequest? request)
    {
        return new JsonResult(material.SeedPartUnload(request?.EquipmentCode, request?.ReceiptNumber), HeartbeatJson.Options);
    }

    [HttpPost("seed-material-check")]
    public ActionResult<object> SeedMaterialCheck([FromBody] SeedMaterialCheckRequest? request)
    {
        return new JsonResult(material.SeedMaterialCheck(request?.EquipmentCode, request?.LabelNo, request?.ProcessNo), HeartbeatJson.Options);
    }

    [HttpPost("seed-part-life")]
    public ActionResult<object> SeedPartLife([FromBody] SeedPartLifeRequest? request)
    {
        return new JsonResult(material.SeedPartLife(request?.EquipmentCode, request?.PartNo), HeartbeatJson.Options);
    }
}
