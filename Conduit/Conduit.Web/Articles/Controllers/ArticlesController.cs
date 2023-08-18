using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.ViewModels;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Web.Articles.Controllers;

[ApiController]
[Authorize]
public class ArticlesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAuthService _authService;

    public ArticlesController(IMediator mediator, IAuthService authService)
    {
        _mediator = mediator;
        _authService = authService;
    }

    [Authorize]
    [HttpPost("/api/articles")]
    public async Task<IActionResult> CreateArticle([FromBody] CreateArticleSubmitModel articleSubmission)
    {
        // TODO: make these 3 lines into a convenience method in authservice
        var authHeader = Request.Headers["Authorization"];
        var bearerToken = _authService.GetTokenFromHeader(authHeader);
        var authorUsername = _authService.GetUsernameClaim(bearerToken).Value;
        
        var request = new CreateArticleRequest(articleSubmission, authorUsername);

        var article = await _mediator.Send(request);

        // TODO: check validation errors for article

        var authorProfile = await _mediator.Send(new GetProfileRequest(authorUsername, bearerToken));

        // TODO: check validation errors for profile

        // using dynamic here to meet requirements of Conduit
        // spec without creating yet another DTO class
        dynamic returnArticle = new
        {
            article = article.Article
        };
        returnArticle.article.author = authorProfile.ProfileView;
        return Ok(returnArticle);
    }
}
