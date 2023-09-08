using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Articles.Handlers;

public class FavoriteArticleHandlerTests : CouchbaseIntegrationTest
{
    private IConduitArticlesCollectionProvider _articleCollectionProvider;
    private FavoriteArticleHandler _handler;
    private IConduitFavoritesCollectionProvider _favoritesCollectionProvider;
    private ArticlesDataService _articleDataService;
    private IConduitUsersCollectionProvider _usersCollectionProvider;

    public override async Task Setup()
    {
        await base.Setup();

        _articleCollectionProvider = ServiceProvider.GetRequiredService<IConduitArticlesCollectionProvider>();
        _favoritesCollectionProvider = ServiceProvider.GetRequiredService<IConduitFavoritesCollectionProvider>();
        _usersCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();
        _articleDataService = new ArticlesDataService(_articleCollectionProvider, _favoritesCollectionProvider);

        // setup handler and dependencies
        _handler = new FavoriteArticleHandler(_articleDataService);
    }

    [Test]
    public async Task FavoriteArticleHandler_adds_article_to_a_users_favorites()
    {
        // arrange
        var user = await _usersCollectionProvider.CreateUserInDatabase();
        var article = await _articleCollectionProvider.CreateArticleInDatabase();
        var request = new FavoriteArticleRequest();
        request.Slug = article.Slug;
        request.Username = user.Username;

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result.ValidationErrors == null || !result.ValidationErrors.Any(), Is.True);
        await _favoritesCollectionProvider.AssertExists(user.Username, x =>
        {
            Assert.That(x.Contains(request.Slug.GetArticleKey()), Is.True);
        });
    }
}