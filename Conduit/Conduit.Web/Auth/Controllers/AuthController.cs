using Conduit.Web.Auth.Handlers;
using Conduit.Web.Auth.Services;
using Conduit.Web.Auth.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Web.Auth.Controllers;

public class AuthController : Controller
{
    private readonly IMediator _mediator;
    private readonly IAuthService _authService;

    public AuthController(IMediator mediator, IAuthService authService)
    {
        _mediator = mediator;
        _authService = authService;
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
            return Forbid("User already exists.");

        if (registrationResult.ValidationErrors?.Any() ?? false)
        {
            // TODO: turn all this string.join noise into an extension method
            return UnprocessableEntity(string.Join(",",registrationResult.ValidationErrors.Select(e => e.ErrorMessage)));
        }
        
        return Ok(new { user = registrationResult.UserView });
    }

    [HttpGet("api/user")]
    [Authorize]
    public async Task<IActionResult> GetUser([FromHeader(Name = "Authorization")] string bearerTokenHeader)
    {
        var bearerToken = _authService.GetTokenFromHeader(bearerTokenHeader);

        var result = await _mediator.Send(new GetUserRequest(bearerToken));

        if (result.IsInvalidToken)
            return UnprocessableEntity();

        return Ok(result.UserView);
    }
}