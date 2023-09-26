using System.Net;
using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.ViewModels;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.Extensions;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Web.Articles.Controllers;

public class CommentsController : Controller
{
    private readonly IAuthService _authService;
    private readonly IMediator _mediator;

    public CommentsController(IAuthService authService, IMediator mediator)
    {
        _authService = authService;
        _mediator = mediator;
    }

    /// <summary>
    /// Add comment to an article
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints#add-comments-to-an-article">Conduit spec for Add Comments to an Article</a>
    /// </remarks>
    /// <param name="slug">Article slug</param>
    /// <param name="comment">Body of the comment</param>
    /// <returns>A view of the created comment</returns>
    /// <response code="200">Successful created, returns the created Comment</response>
    /// <response code="401">Unauthorized, likely because credentials are incorrect</response>
    /// <response code="404">Article wasn't found</response>
    /// <response code="422">Article was unable to be created</response>
    /// <response code="500">Something went wrong while creating the article</response>
    [HttpPost]
    [Route("/api/articles/{slug}/comments")]
    [Authorize]
    public async Task<IActionResult> AddComment([FromRoute] string slug, [FromBody] CommentBodyModel comment)
    {
        var claims = _authService.GetAllAuthInfo(Request.Headers["Authorization"]);
        var username  = claims.Username.Value;

        var addCommentRequest = new AddCommentRequest(username, comment, slug);
        var addCommentResponse = await _mediator.Send(addCommentRequest);

        if (addCommentResponse.ValidationErrors?.Any() ?? false)
            return UnprocessableEntity(addCommentResponse.ValidationErrors.ToCsv());
        if (addCommentResponse.IsArticleNotFound)
            return NotFound($"Article {slug} not found.");
        if (addCommentResponse.IsFailed)
            return StatusCode(500, "There was a problem adding that comment.");

        // get the author profile
        var authorProfile = await _mediator.Send(new GetProfileRequest(addCommentResponse.Comment.AuthorUsername, claims.BearerToken));
        if (authorProfile.ValidationErrors?.Any() ?? false)
            return UnprocessableEntity(authorProfile.ValidationErrors.ToCsv());

        var viewModel = new CommentViewModel
        {
            Id = addCommentResponse.Comment.Id,
            CreatedAt = addCommentResponse.Comment.CreatedAt,
            UpdatedAt = addCommentResponse.Comment.UpdatedAt,
            Body = addCommentResponse.Comment.Body,
            Author = authorProfile.ProfileView
        };

        return Ok(new { comment = viewModel });
    }

    /// <summary>
    /// Get all comments for a given article
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints/#get-comments-from-an-article">Conduit spec for Get All Comments for and Article</a>
    /// </remarks>
    /// <param name="slug">Article slug (required)</param>
    /// <returns>Comments array</returns>
    [HttpGet]
    [AllowAnonymous]
    [Route("/api/articles/{slug}/comments")]
    public async Task<IActionResult> GetComments([FromRoute] string slug)
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

        // make request to mediator
        var request = new GetCommentsRequest(slug, username);
        var response = await _mediator.Send(request);

        if (response.IsArticleNotFound)
            return NotFound($"Article {slug} not found.");
        if (response.IsFailed)
            return StatusCode(500, "There was a problem getting comments.");

        return Ok(new { comments = response.CommentsView });
    }
}