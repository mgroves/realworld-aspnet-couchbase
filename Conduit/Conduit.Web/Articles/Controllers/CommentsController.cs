﻿using System.Net;
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

    [HttpPost]
    [Route("/api/articles/{slug}/comments")]
    [Authorize]
    public async Task<IActionResult> AddComment([FromRoute] string slug, [FromBody] CommentBodyModel body)
    {
        var claims = _authService.GetAllAuthInfo(Request.Headers["Authorization"]);
        var username  = claims.Username.Value;

        var addCommentRequest = new AddCommentRequest(username, body, slug);
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
}