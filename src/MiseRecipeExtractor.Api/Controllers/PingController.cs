using Microsoft.AspNetCore.Mvc;

namespace MiseRecipeExtractor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    // GET
    [HttpGet]
    public IActionResult Index()
    {
        return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
    }
}