using Conduit.Web.Articles.Services;
using FluentValidation.Results;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class UnfavoriteArticleHandler : IRequestHandler<UnfavoriteArticleRequest, UnfavoriteArticleResponse>
{
    private readonly IArticlesDataService _articlesDataService;

    public UnfavoriteArticleHandler(IArticlesDataService articlesDataService)
    {
        _articlesDataService = articlesDataService;
    }

    public async Task<UnfavoriteArticleResponse> Handle(UnfavoriteArticleRequest request, CancellationToken cancellationToken)
    {
        var articleExists = await _articlesDataService.Exists(request.Slug);
        if (!articleExists)
        {
            return new UnfavoriteArticleResponse
            {
                ValidationErrors = new List<ValidationFailure> { new ValidationFailure("", "Article not found.") }
            };
        }

        await _articlesDataService.Unfavorite(request.Slug, request.Username);

        return new UnfavoriteArticleResponse();
    }
}