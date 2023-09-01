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
        var claims = _authService.GetAllAuthInfo(Request.Headers["Authorization"]);
        var authorUsername = claims.Username.Value;

        // get the author profile first
        var authorProfile = await _mediator.Send(new GetProfileRequest(authorUsername, claims.BearerToken));
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

    /// <summary>
    /// Mark an article as a favorite of the logged-in user's
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints/#favorite-article">Conduit spec for favorite article endpoint</a>
    /// </remarks>
    /// <param name="slug">Article slug</param>
    /// <returns>Article (with profile of author embedded)</returns>
    [HttpPost]
    [Route("/api/article/{slug}/favorite")]
    [Authorize]
    public async Task<IActionResult> FavoriteArticle(string slug)
    {
        // get auth info
        var claims = _authService.GetAllAuthInfo(Request.Headers["Authorization"]);

        // send request to favorite the article
        var favoriteRequest = new FavoriteArticleRequest();
        favoriteRequest.Username = claims.Username.Value;
        favoriteRequest.Slug = slug;
        var favoriteResponse = await _mediator.Send(favoriteRequest);
        if (favoriteResponse.ValidationErrors?.Any() ?? false)
            return UnprocessableEntity(favoriteResponse.ValidationErrors.ToCsv());

        // ask handler for the article view
        var getRequest = new GetArticleRequest(slug, claims.Username.Value);
        var getResponse = await _mediator.Send(getRequest);
        if (getResponse.ValidationErrors?.Any() ?? false)
            return UnprocessableEntity(getResponse.ValidationErrors.ToCsv());

        return Ok(getResponse.ArticleView);
    }

    /// <summary>
    /// Get an article
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints/#get-article">Conduit spec for Get Article endpoint</a>
    /// </remarks>
    /// <param name="slug">Article slug</param>
    /// <returns>Article (with profile of author embedded)</returns>
    [HttpGet]
    [Route("/api/article/{slug}")]
    public async Task<IActionResult> Get(string slug)
    {
        // get (optional) auth info
        string username = null;
        var headers = Request.Headers["Authorization"];
        var isUserAnonymous = headers.All(string.IsNullOrEmpty);

        if (!isUserAnonymous)
        {
            var claims = _authService.GetAllAuthInfo(Request.Headers["Authorization"]);
            username = claims.Username.Value;
        }

        // send request to get article
        var getRequest = new GetArticleRequest(slug, username);
        var getResponse = await _mediator.Send(getRequest);
        if (getResponse.ValidationErrors?.Any() ?? false)
            return UnprocessableEntity(getResponse.ValidationErrors.ToCsv());

        return Ok(getResponse.ArticleView);
    }
}
