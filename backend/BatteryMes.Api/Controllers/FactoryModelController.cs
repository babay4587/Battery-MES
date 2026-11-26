using BatteryMes.Api.Factory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BatteryMes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/factory-model")]
public sealed class FactoryModelController(FactoryModelService factory) : ControllerBase
{
    [HttpGet("tree")]
    public ActionResult<object> Tree()
    {
        return new { nodes = factory.GetTree() };
    }

    [HttpPost("nodes")]
    public ActionResult<FactoryNodeDto> Create([FromBody] SaveFactoryNodeRequest request)
    {
        return factory.Create(request);
    }

    [HttpPut("nodes/{nid}")]
    public ActionResult<FactoryNodeDto> Update(string nid, [FromBody] SaveFactoryNodeRequest request)
    {
        return factory.Update(nid, request);
    }

    [HttpDelete("nodes/{nid}")]
    public IActionResult Delete(string nid)
    {
        factory.Delete(nid);
        return NoContent();
    }
}
