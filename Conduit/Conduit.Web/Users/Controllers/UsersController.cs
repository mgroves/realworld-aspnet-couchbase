﻿using Conduit.Web.Extensions;
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
    /// <summary>
    /// Login a user, providing a JWT token
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints#authentication">Conduit Spec for login endpoint</a>
    /// </remarks>
    /// <returns>User</returns>
    /// <param name="model">Login credentials</param>
    /// <response code="200">Successful login, returns the User that is logged in</response>
    /// <response code="401">Unauthorized, likely because credentials are incorrect</response>
    [HttpPost("api/users/login")]
    public async Task<IActionResult> Login([FromBody] LoginSubmitModel model)
    {
        var loginResult = await _mediator.Send(new LoginRequest(model));

        if (loginResult.IsUnauthorized)
            return Unauthorized();

        if (loginResult.ValidationErrors?.Any() ?? false)
            return Unauthorized(loginResult.ValidationErrors.ToCsv());

        return Ok(new { user = loginResult.UserView });
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints/#registration">Conduit Spec for registration endpoint</a>
    /// </remarks>
    /// <param name="model">Registration information</param>
    /// <returns>User</returns>
    /// <response code="200">Returns the newly registered User</response>
    /// <response code="403">User with this email address already exists</response>
    /// <response code="422">The registration information was not valid (e.g. invalid email, weak password, etc)</response>
    [HttpPost("api/users")]
    public async Task<IActionResult> Registration([FromBody] RegistrationSubmitModel model)
    {
        var registrationResult = await _mediator.Send(new RegistrationRequest(model));

        if (registrationResult.UserAlreadyExists)
            return new ObjectResult(new { message = "User already exists." }) { StatusCode = 403 };

        if (registrationResult.ValidationErrors?.Any() ?? false)
            return UnprocessableEntity(registrationResult.ValidationErrors.ToCsv());
        
        return Ok(new { user = registrationResult.UserView });
    }

    /// <summary>
    /// Get the current user (by JWT token)
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints#get-current-user">Conduit spec for Get Current User endpoint</a>
    /// </remarks>
    /// <returns>User</returns>
    /// <response code="200">Returns the currently logged in User</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Not Found (weird edge case, but possible)</response>
    /// <response code="422">The given token was not valid (weird edge case, even possible?)</response>
    [HttpGet("api/user")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var authHeader = Request.Headers["Authorization"];

        var bearerToken = _authService.GetTokenFromHeader(authHeader);

        var result = await _mediator.Send(new GetCurrentUserRequest(bearerToken));

        if (result.UserNotFound)
            return NotFound("User not found");

        if (result.IsInvalidToken)
            return UnprocessableEntity("JWT not valid");

        return Ok(new { user = result.UserView });
    }

    /// <summary>
    /// Make changes to one or more parts of a user's information
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints#update-user">Conduit spec for Update User</a>
    /// </remarks>
    /// <param name="updateUser"></param>
    /// <returns>User</returns>
    /// <response code="200">Return the user after update</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="422">The update request was invalid</response>
    [Authorize]
    [HttpPut("api/user")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserSubmitModel updateUser)
    {
        var authHeader = Request.Headers["Authorization"];
        var bearerToken = _authService.GetTokenFromHeader(authHeader);
        var username = _authService.GetUsernameClaim(bearerToken);
        if (username.IsNotFound)
            return UnprocessableEntity("User not found.");

        updateUser.User.Username = username.Value;

        var result = await _mediator.Send(new UpdateUserRequest(updateUser));

        if (result.IsNotFound)
            return NotFound("User not found");

        if (result.ValidationErrors?.Any() ?? false)
            return UnprocessableEntity(result.ValidationErrors.ToCsv());

        return Ok(new { user = result.UserView });
    }

    /// <summary>
    /// Get the public profile for a given user.
    /// If a JWT token is in the headers, the "following" status will also be returned
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints/#get-profile">Conduit spec for Get Profile</a>
    /// </remarks>
    /// <param name="username">Username (required)</param>
    /// <returns>Profile</returns>
    /// <response code="200">Return the public profile</response>
    /// <response code="422">The update request was invalid</response>
    [HttpGet("api/profiles/{username}")]
    public async Task<IActionResult> GetProfile(string username)
    {
        var authHeader = Request.Headers["Authorization"];

        var bearerToken = _authService.GetTokenFromHeader(authHeader);

        var result = await _mediator.Send(new GetProfileRequest(username, bearerToken));

        if (result.UserNotFound)
            return UnprocessableEntity("Username not found.");

        if (result.ValidationErrors?.Any() ?? false)
            return UnprocessableEntity(result.ValidationErrors.ToCsv());

        return Ok(new { profile = result.ProfileView });
    }
}