using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Dto;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class ArticleDeleteHandler : IRequestHandler<ArticleDeleteRequest, ArticleDeleteResponse>
{
    private readonly IArticlesDataService _articlesDataService;
    private readonly IValidator<ArticleDeleteRequest> _validator;

    public ArticleDeleteHandler(IArticlesDataService articlesDataService, IValidator<ArticleDeleteRequest> validator)
    {
        _articlesDataService = articlesDataService;
        _validator = validator;
    }

    public async Task<ArticleDeleteResponse> Handle(ArticleDeleteRequest request, CancellationToken cancellationToken)
    {
        // validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new ArticleDeleteResponse
            {
                ValidationErrors = validationResult.Errors
            };
        }

        // make sure only the author can delete their article
        var allowed = await _articlesDataService.IsArticleAuthor(request.Slug, request.Username);
        if (!allowed)
            return new ArticleDeleteResponse { IsUnauthorized = true };

        // try deleting
        var result = await _articlesDataService.DeleteArticle(request.Slug);
        if (result.Status == DataResultStatus.NotFound)
            return new ArticleDeleteResponse() { ArticleNotFound = true };

        return new ArticleDeleteResponse();
    }
}