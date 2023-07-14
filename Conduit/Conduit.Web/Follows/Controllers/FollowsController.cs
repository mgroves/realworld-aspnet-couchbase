using Conduit.Web.Follows.Handlers;
using Conduit.Web.Users.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Web.Follows.Controllers;

public class FollowsController : Controller
{
    private readonly IMediator _mediator;
    private readonly IAuthService _authService;

    public FollowsController(IMediator mediator, IAuthService authService)
    {
        _mediator = mediator;
        _authService = authService;
    }

    /// <summary>
    /// Follow a user
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints/#follow-user">Conduit Spec for follow user endpoint</a>
    /// </remarks>
    /// <param name="username">Username of the user to be followed</param>
    /// <returns>Profile</returns>
    /// <response code="200">Returns the followed user's detailed profile information</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">User not found</response>
    [Authorize]
    [HttpPost("api/profiles/{username}/follow")]
    public async Task<IActionResult> FollowUser(string username)
    {
        var authHeader = Request.Headers["Authorization"];
        var bearerToken = _authService.GetTokenFromHeader(authHeader);
        var currentUsername = _authService.GetUsernameClaim(bearerToken);

        var result = await _mediator.Send(new FollowUserRequest(username, currentUsername.Value));
        if (result.UserNotFound)
            return NotFound("User not found");

        return Ok(result.Profile);
    }
}