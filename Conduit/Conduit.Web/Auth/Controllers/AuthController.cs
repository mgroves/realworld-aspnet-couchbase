using Conduit.Web.Auth.Handlers;
using Conduit.Web.Auth.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Web.Auth.Controllers;

public class AuthController : Controller
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("api/users/login")]
    public async Task<IActionResult> Login([FromBody] LoginSubmitModel model)
    {
        var loginResult = await _mediator.Send(new LoginRequest(model));

        if (loginResult.IsUnauthorized)
            return Unauthorized();

        return Ok(new { user = loginResult.UserView });
    }

    [HttpPost("api/users")]
    public async Task<IActionResult> Registration([FromBody] RegistrationSubmitModel model)
    {
        var registrationResult = await _mediator.Send(new RegistrationRequest(model));

        if (registrationResult.UserAlreadyExists)
            return Forbid();

        return Ok(new { user = registrationResult.UserView });
    }
}