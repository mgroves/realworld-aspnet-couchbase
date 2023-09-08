using System.Net;
using Conduit.Web.DataAccess.Providers;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Tests.TestHelpers.Dto;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.Articles.ViewModels;
using Conduit.Tests.TestHelpers;

namespace Conduit.Tests.Functional.Articles.Controllers.ArticlesControllerTests;

[TestFixture]
public class CreateTests : FunctionalTestBase
{
    private IConduitUsersCollectionProvider _usersCollectionProvider;
    private User _user;

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
    }

    [Test]
    public async Task Create_article_with_valid_input()
    {
        // arrange
        var payload = CreateArticleSubmitModelHelper.Create();

        // act
        var response = await WebClient.PostAsync($"api/articles", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();
        var articleViewModel = responseString.SubDoc<ArticleViewModel>("article");

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(articleViewModel.Title, Is.EqualTo(payload.Article.Title));
        Assert.That(articleViewModel.Body, Is.EqualTo(payload.Article.Body));
        Assert.That(articleViewModel.Description, Is.EqualTo(payload.Article.Description));
        Assert.That((articleViewModel.CreatedAt - DateTimeOffset.Now).TotalSeconds, Is.LessThanOrEqualTo(30));
        Assert.That(articleViewModel.UpdatedAt, Is.Null);
    }
}