using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Models;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class AddCommentHandler : IRequestHandler<AddCommentRequest, AddCommentResponse>
{
    private readonly IValidator<AddCommentRequest> _validator;
    private readonly IArticlesDataService _articlesDataService;
    private readonly ICommentsDataService _commentsDataService;
    private readonly Random _random;

    public AddCommentHandler(IValidator<AddCommentRequest> validator, IArticlesDataService articlesDataService, ICommentsDataService commentsDataService)
    {
        _validator = validator;
        _articlesDataService = articlesDataService;
        _commentsDataService = commentsDataService;
        _random = new Random();
    }

    public async Task<AddCommentResponse> Handle(AddCommentRequest request, CancellationToken cancellationToken)
    {
        // validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new AddCommentResponse()
            {
                ValidationErrors = validationResult.Errors
            };
        }

        var doesArticleExist = await _articlesDataService.Exists(request.Slug);
        if (!doesArticleExist)
        {
            return new AddCommentResponse
            {
                IsArticleNotFound = true
            };
        }

        var newComment = new Comment();
        newComment.CreatedAt = DateTimeOffset.Now;
        newComment.Body = request.Body.Trim();
        newComment.AuthorUsername = request.Username;

        var result = await _commentsDataService.Add(newComment, request.Slug);
        if (result.Status != DataResultStatus.Ok)
        {
            return new AddCommentResponse
            {
                IsFailed = true
            };
        }

        return new AddCommentResponse
        {
            Comment = result.DataResult
        };
    }
}