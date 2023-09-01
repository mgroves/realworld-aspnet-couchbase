using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Dto;
using Conduit.Web.Extensions;
using FluentValidation.Results;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class FavoriteArticleRequest : IRequest<FavoriteArticleResponse>
{
    public string Username { get; set; }
    public string Slug { get; set; }
}

public class FavoriteArticleResponse
{
    public List<ValidationFailure> ValidationErrors { get; set; }
}

public class FavoriteArticleHandler : IRequestHandler<FavoriteArticleRequest, FavoriteArticleResponse>
{
    private readonly IArticlesDataService _articlesDataService;

    public FavoriteArticleHandler(IArticlesDataService articlesDataService)
    {
        _articlesDataService = articlesDataService;
    }

    public async Task<FavoriteArticleResponse> Handle(FavoriteArticleRequest request, CancellationToken cancellationToken)
    {
        var articleExists = await _articlesDataService.Exists(request.Slug.GetArticleKey());
        if (!articleExists)
        {
            return new FavoriteArticleResponse
            {
                ValidationErrors = new List<ValidationFailure> { new ValidationFailure("", "Article not found.") }
            };
        }

        await _articlesDataService.Favorite(request.Slug, request.Username);

        return new FavoriteArticleResponse();
    }
}