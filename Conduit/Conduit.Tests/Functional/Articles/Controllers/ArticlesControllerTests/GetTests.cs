using Conduit.Tests.TestHelpers.Dto;
using Conduit.Web.Articles.ViewModels;
using Conduit.Web.DataAccess.Providers;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net;
using Conduit.Tests.TestHelpers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.DataAccess.Models;

namespace Conduit.Tests.Functional.Articles.Controllers.ArticlesControllerTests;

[TestFixture]
public class GetTests : FunctionalTestBase
{
    private IConduitUsersCollectionProvider _usersCollectionProvider;
    private User _user;
    private IConduitArticlesCollectionProvider _articleCollectionProvider;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();

        // setup database objects for arranging
        var service = WebAppFactory.Services;
        _usersCollectionProvider = service.GetRequiredService<IConduitUsersCollectionProvider>();
        _articleCollectionProvider = service.GetRequiredService<IConduitArticlesCollectionProvider>();

        // setup an authorized header
        _user = await _usersCollectionProvider.CreateUserInDatabase();
        var jwtToken = AuthSvc.GenerateJwtToken(_user.Email, _user.Username);
        WebClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", jwtToken);
    }

    [Test]
    public async Task Get_an_article_that_was_just_created()
    {
        // arrange
        var author = await _usersCollectionProvider.CreateUserInDatabase();
        var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: author.Username);

        // act
        var response = await WebClient.GetAsync($"api/article/{article.Slug}");
        var responseString = await response.Content.ReadAsStringAsync();
        var articleViewModel = responseString.SubDoc<ArticleViewModel>("article");

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(articleViewModel.Title, Is.EqualTo(article.Title));
        Assert.That(articleViewModel.Body, Is.EqualTo(article.Body));
        Assert.That(articleViewModel.Description, Is.EqualTo(article.Description));
        Assert.That((articleViewModel.CreatedAt - DateTimeOffset.Now).TotalSeconds, Is.LessThanOrEqualTo(30));
        Assert.That(articleViewModel.UpdatedAt, Is.Null);
    }
}