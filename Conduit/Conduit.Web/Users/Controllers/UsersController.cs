using Conduit.Web.Extensions;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Web.Users.Controllers;

public class UsersController : Controller
{
    private readonly IMediator _mediator;
    private readonly IAuthService _authService;

    public UsersController(IMediator mediator, IAuthService authService)
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
            return UnprocessableEntity(registrationResult.ValidationErrors.ToCsv());
        
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

    [Authorize]
    [HttpPut("api/user")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserSubmitModel updateUser)
    {
        var result = await _mediator.Send(new UpdateUserRequest(updateUser));

        if (result.ValidationErrors?.Any() ?? false)
            return UnprocessableEntity(result.ValidationErrors.ToCsv());

        return Ok(result.UserView);
    }
}