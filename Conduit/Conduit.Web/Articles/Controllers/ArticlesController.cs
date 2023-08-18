using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.ViewModels;
using Conduit.Web.Extensions;
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

    /// <summary>
    /// Create article.
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints/#create-article">Conduit Spec for create article endpoint</a>
    /// </remarks>
    /// <param name="articleSubmission"></param>
    /// <returns>Article (with profile of author embedded)</returns>
    [Authorize]
    [HttpPost("/api/articles")]
    public async Task<IActionResult> CreateArticle([FromBody] CreateArticleSubmitModel articleSubmission)
    {
        // TODO: make these 3 lines into a convenience method in authservice
        var authHeader = Request.Headers["Authorization"];
        var bearerToken = _authService.GetTokenFromHeader(authHeader);
        var authorUsername = _authService.GetUsernameClaim(bearerToken).Value;

        // get the author profile first
        var authorProfile = await _mediator.Send(new GetProfileRequest(authorUsername, bearerToken));
        if (authorProfile.ValidationErrors?.Any() ?? false)
            return UnprocessableEntity(authorProfile.ValidationErrors.ToCsv());

        // process the creation of the article
        var request = new CreateArticleRequest(articleSubmission, authorUsername);
        var articleView = await _mediator.Send(request);
        if (articleView.ValidationErrors?.Any() ?? false)
            return UnprocessableEntity(articleView.ValidationErrors.ToCsv());

        // return the combined view of article+author
        articleView.Article.Author = authorProfile.ProfileView;
        return Ok(articleView);
    }
}
