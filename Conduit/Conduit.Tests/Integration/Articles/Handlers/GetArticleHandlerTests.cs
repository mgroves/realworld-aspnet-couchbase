using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Adaptive.Services;
using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Follows.Services;
using Conduit.Web.Users.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Conduit.Tests.Integration.Articles.Handlers;

public class GetArticleHandlerTests : CouchbaseIntegrationTest
{
    private IConduitArticlesCollectionProvider _articleCollectionProvider;
    private IConduitUsersCollectionProvider _usersCollectionProvider;
    private IConduitFollowsCollectionProvider _followsCollectionProvider;
    private ArticlesDataService _articleDataService;
    private IConduitFavoritesCollectionProvider _favoriteCollectionProvider;
    private GetArticleHandler _handler;
    private AuthService _authService;
    private UserDataService _userDataService;
    private FollowsDataService _followDataService;

    public override async Task Setup()
    {
        await base.Setup();

        _usersCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();
        _articleCollectionProvider = ServiceProvider.GetRequiredService<IConduitArticlesCollectionProvider>();
        _followsCollectionProvider = ServiceProvider.GetRequiredService<IConduitFollowsCollectionProvider>();
        _favoriteCollectionProvider = ServiceProvider.GetRequiredService<IConduitFavoritesCollectionProvider>();
        var adaptiveDataService = ServiceProvider.GetRequiredService<IAdaptiveDataService>();

        // setup handler and dependencies
        var jwtSecrets = new JwtSecrets
        {
            Audience = "dummy-audience",
            Issuer = "dummy-issuer",
            SecurityKey = "dummy-securitykey-dummy-securitykey-dummy-securitykey-dummy-securitykey"
        };
        _authService = new AuthService(new OptionsWrapper<JwtSecrets>(jwtSecrets));
        _articleDataService = new ArticlesDataService(_articleCollectionProvider, _favoriteCollectionProvider);
        _followDataService = new FollowsDataService(_followsCollectionProvider);
        _userDataService = new UserDataService(_usersCollectionProvider, _authService);
        _handler = new GetArticleHandler(_articleDataService, _userDataService, _followDataService);
    }

    [Test]
    public async Task GetArticleHandler_Returns_article()
    {
        // arrange
        var currentUser = await _usersCollectionProvider.CreateUserInDatabase();
        var authorUser = await _usersCollectionProvider.CreateUserInDatabase();
        var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: authorUser.Username);
        var request = new GetArticleRequest(article.Slug, currentUser.Username);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result.ArticleView.Slug, Is.EqualTo(article.Slug));
        Assert.That(result.ArticleView.Title, Is.EqualTo(article.Title));
        Assert.That(result.ArticleView.Author.Username, Is.EqualTo(authorUser.Username));
    }
}