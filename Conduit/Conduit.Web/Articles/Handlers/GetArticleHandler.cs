using Conduit.Web.Articles.Services;
using Conduit.Web.Follows.Services;
using Conduit.Web.Users.Services;
using FluentValidation.Results;
using MediatR;

namespace Conduit.Web.Articles.Handlers;

public class GetArticleHandler : IRequestHandler<GetArticleRequest, GetArticleResponse>
{
    private readonly IArticlesDataService _articlesDataService;
    private readonly IUserDataService _userDataService;
    private readonly IFollowDataService _followDataService;

    public GetArticleHandler(IArticlesDataService articlesDataService, IUserDataService userDataService, IFollowDataService followDataService)
    {
        _articlesDataService = articlesDataService;
        _userDataService = userDataService;
        _followDataService = followDataService;
    }

    public async Task<GetArticleResponse> Handle(GetArticleRequest request, CancellationToken cancellationToken)
    {
        var articleExists = await _articlesDataService.Exists(request.Slug);
        if (!articleExists)
        {
            return new GetArticleResponse
            {
                ValidationErrors = new List<ValidationFailure> { new ValidationFailure("", "Article not found.") }
            };
        }

        // build up response view: article, profile
        var article = await _articlesDataService.Get(request.Slug);
        var authorProfile = await _userDataService.GetProfileByUsername(article.DataResult.AuthorUsername);
        var response = new GetArticleResponse(article.DataResult, authorProfile.DataResult);

        // check for profile following and article favoriting if the current user is logged in
        if (request.IsUserAuthenticated)
        {
            response.ArticleView.Author.Following = await _followDataService.IsCurrentUserFollowing(request.CurrentUser, authorProfile.DataResult.Username);
            response.ArticleView.Favorited = await _articlesDataService.IsFavorited(request.Slug, request.CurrentUser);
        }
        else
        {
            response.ArticleView.Author.Following = false;
            response.ArticleView.Favorited = false;
        }

        return response;
    }
}