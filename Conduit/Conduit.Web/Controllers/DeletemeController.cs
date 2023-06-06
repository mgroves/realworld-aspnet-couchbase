using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Web.Controllers;

public class DeletemeController : Controller
{
    [HttpGet]
    [Route("deleteme")]
    [Authorize]
    public async Task<IActionResult> Hello()
    {
        return Ok("it worked");
    }
}