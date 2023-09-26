using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Dto;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class DeleteCommentHandler : IRequestHandler<DeleteCommentRequest, DeleteCommentResponse>
{
    private readonly IValidator<DeleteCommentRequest> _validator;
    private readonly IArticlesDataService _articlesDataService;
    private readonly ICommentsDataService _commentsDataService;

    public DeleteCommentHandler(IValidator<DeleteCommentRequest> validator, IArticlesDataService articlesDataService, ICommentsDataService commentsDataService)
    {
        _validator = validator;
        _articlesDataService = articlesDataService;
        _commentsDataService = commentsDataService;
    }

    public async Task<DeleteCommentResponse> Handle(DeleteCommentRequest request, CancellationToken cancellationToken)
    {
        // validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return new DeleteCommentResponse { ValidationErrors = validationResult.Errors };

        // return error if article doesn't exist
        var doesArticleExist = await _articlesDataService.Exists(request.Slug);
        if (!doesArticleExist)
            return new DeleteCommentResponse { IsArticleNotFound = true };

        var dataResponse = await _commentsDataService.Delete(request.CommentId, request.Slug, request.Username);

        if (dataResponse == DataResultStatus.NotFound)
            return new DeleteCommentResponse { IsCommentNotFound = true };
        if (dataResponse == DataResultStatus.Unauthorized)
            return new DeleteCommentResponse { IsNotAuthorized = true };
        if (dataResponse == DataResultStatus.Error)
            return new DeleteCommentResponse { IsFailed = true };

        return new DeleteCommentResponse();
    }
}