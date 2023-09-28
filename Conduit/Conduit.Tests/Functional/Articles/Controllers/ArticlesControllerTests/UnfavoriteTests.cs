using System.Net;
using Conduit.Web.DataAccess.Providers;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.DataAccess.Models;

namespace Conduit.Tests.Functional.Articles.Controllers.ArticlesControllerTests;

[TestFixture]
public class UnfavoriteTests : FunctionalTestBase
{
    private IConduitUsersCollectionProvider _usersCollectionProvider;
    private User _user;
    private IConduitArticlesCollectionProvider _articleCollectionProvider;
    private IConduitFavoritesCollectionProvider _favoriteCollectionProvider;
    private Random _random;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();

        // setup database objects for arranging
        var service = WebAppFactory.Services;
        _usersCollectionProvider = service.GetRequiredService<IConduitUsersCollectionProvider>();

        // setup an authorized header
        _user = await _usersCollectionProvider.CreateUserInDatabase();
        var jwtToken = AuthSvc.GenerateJwtToken(_user.Email, _user.Username);
        WebClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", jwtToken);

        // setup services
        _articleCollectionProvider = service.GetRequiredService<IConduitArticlesCollectionProvider>();
        _favoriteCollectionProvider = service.GetRequiredService<IConduitFavoritesCollectionProvider>();
        _random = new Random();
    }

    [Test]
    public async Task Valid_unfavoriting_of_article()
    {
        // arrange
        var author = await _usersCollectionProvider.CreateUserInDatabase();
        var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: author.Username);
        await _favoriteCollectionProvider.AddFavoriteInDatabase(_user.Username, article.Slug);

        // act
        var response = await WebClient.DeleteAsync($"api/articles/{article.Slug}/unfavorite");

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}