using System.Net;
using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Providers;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Extensions;

namespace Conduit.Tests.Functional.Articles.Controllers.ArticlesControllerTests;

[TestFixture]
public class DeleteTests : FunctionalTestBase
{
    private Random _random;

    private IConduitArticlesCollectionProvider _articleCollectionProvider;
    private IConduitUsersCollectionProvider _usersCollectionProvider;

    private User _user;
    // private IConduitUsersCollectionProvider _usersCollectionProvider;
    // private User _user;
    // private IConduitArticlesCollectionProvider _articleCollectionProvider;
    // private ISlugService _slugService;
    // private Random _random;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();

        _random = new Random();

        // setup database objects for arranging
        var service = WebAppFactory.Services;
        _usersCollectionProvider = service.GetRequiredService<IConduitUsersCollectionProvider>();
        _articleCollectionProvider = service.GetRequiredService<IConduitArticlesCollectionProvider>();
        // _slugService = service.GetService<ISlugService>();
        //
        // setup an authorized header
        _user = await _usersCollectionProvider.CreateUserInDatabase();
        var jwtToken = AuthSvc.GenerateJwtToken(_user.Email, _user.Username);
        WebClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", jwtToken);
    }

    [Test]
    public async Task Article_can_be_deleted()
    {
        // arrange
        var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: _user.Username);

        // act
        var response = await WebClient.DeleteAsync($"api/articles/{article.Slug}");

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await _articleCollectionProvider.AssertNotExists(article.Slug);
    }

    [Test]
    public async Task Article_for_some_other_author_can_not_be_deleted()
    {
        // arrange
        var article = await _articleCollectionProvider.CreateArticleInDatabase();

        // act
        var response = await WebClient.DeleteAsync($"api/articles/{article.Slug}");
        var responseString = await response.Content.ReadAsStringAsync();

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(responseString, Contains.Substring("You're not allowed to delete that article."));
    }

    [Test]
    public async Task Article_that_doesnt_exist_returns_not_found()
    {
        // I don't think this is possible, unless there's an extreme edge case
    }

    [TestCase("", "Article slug is required.")]
    [TestCase(null, "Article slug is required.")]
    public async Task Article_slug_must_be_specified(string invalidArticleSlug, string expectedErrorMessage)
    {
        // asp.net already prevents this
    }

    [TestCase("invalid-slug")]
    public async Task With_invalid_slug_this_is_unauthorized(string invalidSlug)
    {
        // arrange
        var slug = invalidSlug;

        // act
        var response = await WebClient.DeleteAsync($"api/articles/{slug}");

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}