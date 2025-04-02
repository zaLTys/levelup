using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Duende.IDP.Apis.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HeartbeatController : ControllerBase
{
    [HttpGet]
    [Route("")]
    [AllowAnonymous]
    public IActionResult Heartbeat(CancellationToken cancellationToken)
    {
        return Ok("Thump thump");
    }
}