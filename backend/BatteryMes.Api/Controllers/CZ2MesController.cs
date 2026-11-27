using BatteryMes.Api.Equipment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BatteryMes.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/CZ2Mes")]
public sealed class CZ2MesController(HeartBeatService heartbeats, EquipmentStatusService equipment, MaterialHandlingService material) : ControllerBase
{
    [HttpPost("HeartBeat")]
    public IActionResult HeartBeat([FromBody] HeartBeatRequest? request)
    {
        return new JsonResult(heartbeats.Handle(request), HeartbeatJson.Options);
    }

    [HttpPost("ClearEquipHeartBeatStatus")]
    public IActionResult ClearEquipHeartBeatStatus([FromBody] HeartBeatRequest? request)
    {
        return new JsonResult(equipment.ClearEquipHeartBeatStatus(request), HeartbeatJson.Options);
    }

    [HttpPost("EquipStatus")]
    public IActionResult EquipStatus([FromBody] EquipStatusRequest? request)
    {
        return new JsonResult(equipment.EquipStatus(request), HeartbeatJson.Options);
    }

    [HttpPost("EquipAlert")]
    public IActionResult EquipAlert([FromBody] EquipAlertRequest? request)
    {
        return new JsonResult(equipment.EquipAlert(request), HeartbeatJson.Options);
    }

    [HttpPost("EquipRunRequest")]
    public IActionResult EquipRunRequest([FromBody] EquipRunRequest? request)
    {
        return new JsonResult(equipment.EquipRunRequest(request), HeartbeatJson.Options);
    }

    [HttpPost("EqptParaCHNG")]
    public IActionResult EqptParaCHNG([FromBody] EquipParaChangeRequest? request)
    {
        return new JsonResult(equipment.EqptParaCHNG(request), HeartbeatJson.Options);
    }

    [HttpPost("EqptCtrl")]
    public IActionResult EqptCtrl([FromBody] EqptCtrlRequest? request)
    {
        return new JsonResult(equipment.EqptCtrl(request), HeartbeatJson.Options);
    }

    [HttpPost("ChannelCtrlFeedback")]
    public IActionResult ChannelCtrlFeedback([FromBody] ChannelCtrlFeedbackRequest? request)
    {
        return new JsonResult(equipment.ChannelCtrlFeedback(request), HeartbeatJson.Options);
    }

    [HttpPost("EquipControl")]
    public IActionResult EquipControl([FromBody] List<EquipControlItem>? equipmentInfos)
    {
        return new JsonResult(equipment.EquipControl(equipmentInfos), HeartbeatJson.Options);
    }

    [HttpPost("MaterialLoad")]
    public IActionResult MaterialLoad([FromBody] MaterialLoadRequest? request)
    {
        return new JsonResult(material.MaterialLoad(request), HeartbeatJson.Options);
    }

    [HttpPost("MaterialUnload")]
    public IActionResult MaterialUnload([FromBody] MaterialUnloadRequest? request)
    {
        return new JsonResult(material.MaterialUnload(request), HeartbeatJson.Options);
    }

    [HttpPost("PartLoad")]
    public IActionResult PartLoad([FromBody] PartLoadRequest? request)
    {
        return new JsonResult(material.PartLoad(request), HeartbeatJson.Options);
    }

    [HttpPost("PartUnLoad")]
    public IActionResult PartUnLoad([FromBody] PartUnloadRequest? request)
    {
        return new JsonResult(material.PartUnload(request), HeartbeatJson.Options);
    }

    [HttpPost("PartLife")]
    public IActionResult PartLife([FromBody] PartLifeRequest? request)
    {
        return new JsonResult(material.PartLife(request), HeartbeatJson.Options);
    }

    [HttpPost("MaterialCheck")]
    public IActionResult MaterialCheck([FromBody] MaterialCheckRequest? request)
    {
        return new JsonResult(material.MaterialCheck(request), HeartbeatJson.Options);
    }
}
