using System.Net;
using Conduit.Web.DataAccess.Providers;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Conduit.Tests.TestHelpers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Articles.Handlers;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.Articles.ViewModels;
using Conduit.Web.Extensions;

namespace Conduit.Tests.Functional.Articles.Controllers.CommentsControllerTests;

[TestFixture]
public class AddCommentTests : FunctionalTestBase
{
    private IConduitArticlesCollectionProvider _articleCollectionProvider;
    private IConduitUsersCollectionProvider _usersCollectionProvider;
    private User _user;
    private Random _random;
    private IConduitCommentsCollectionProvider _commentsCollectionProvider;

    [SetUp]
    public async Task Setup()
    {
        await base.Setup();

        // setup database objects for arranging
        var service = WebAppFactory.Services;
        _articleCollectionProvider = service.GetRequiredService<IConduitArticlesCollectionProvider>();
        _usersCollectionProvider = service.GetRequiredService<IConduitUsersCollectionProvider>();
        _commentsCollectionProvider = service.GetRequiredService<IConduitCommentsCollectionProvider>();

        // setup an authorized header
        _user = await _usersCollectionProvider.CreateUserInDatabase();
        var jwtToken = AuthSvc.GenerateJwtToken(_user.Email, _user.Username);
        WebClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", jwtToken);

        _random = new Random();
    }

    [TestCase("")]
    [TestCase("                    ")]
    [TestCase(null)]
    public async Task Valid_comment_must_have_a_body(string invalidCommentBody)
    {
        // arrange
        var article = await _articleCollectionProvider.CreateArticleInDatabase();
        var payload = new CommentBodyModel();
        payload.Comment = new CommentBody { Body = invalidCommentBody };

        // act
        var response = await WebClient.PostAsync($"api/articles/{article.Slug}/comments", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        Assert.That(responseString, Contains.Substring("Comment body is required."));
    }

    [Test]
    public async Task Valid_comment_must_be_under_a_certain_length()
    {
        // arrange
        var article = await _articleCollectionProvider.CreateArticleInDatabase();
        var payload = new CommentBodyModel();
        payload.Comment = new CommentBody { Body = _random.String(1001) };

        // act
        var response = await WebClient.PostAsync($"api/articles/{article.Slug}/comments", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        Assert.That(responseString, Contains.Substring("Comment body is limited to maximum 1000 characters."));
    }

    [Test]
    public async Task Comment_must_be_for_an_article_that_actually_exists()
    {
        // arrange
        var slug = $"miny-wheats-1982::{_random.String(10)}";
        var payload = new CommentBodyModel();
        payload.Comment = new CommentBody { Body = $"this is a valid comment {_random.String(5)}" };

        // act
        var response = await WebClient.PostAsync($"api/articles/{slug}/comments", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(responseString, Contains.Substring($"Article {slug} not found."));
    }

    [Test]
    public async Task Comment_gets_saved_to_database()
    {
        // arrange
        var article = await _articleCollectionProvider.CreateArticleInDatabase();
        var expectedBody = $"this is a valid comment {_random.String(5)}";
        var payload = new CommentBodyModel();
        payload.Comment = new CommentBody { Body = expectedBody };

        // act
        var response = await WebClient.PostAsync($"api/articles/{article.Slug}/comments", payload.ToJsonPayload());

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await _commentsCollectionProvider.AssertExists($"{article.ArticleKey}::comments", x =>
        {
            Assert.That(x.Any(c => c.AuthorUsername == _user.Username && c.Body == expectedBody));
        });
    }
}