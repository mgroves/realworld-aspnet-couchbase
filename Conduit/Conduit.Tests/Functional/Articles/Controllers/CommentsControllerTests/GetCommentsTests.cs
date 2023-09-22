using System.Net;
using System.Net.Http.Headers;
using Conduit.Tests.TestHelpers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Functional.Articles.Controllers.CommentsControllerTests;

[TestFixture]
public class GetCommentsTests : FunctionalTestBase
{
    private IConduitArticlesCollectionProvider _articleCollectionProvider;
    private IConduitUsersCollectionProvider _usersCollectionProvider;
    private User _user;
    private Random _random;
    private IConduitCommentsCollectionProvider _commentsCollectionProvider;
    private IConduitFollowsCollectionProvider _followCollectionProvider;

    [SetUp]
    public async Task Setup()
    {
        await base.Setup();

        // setup database objects for arranging
        var service = WebAppFactory.Services;
        _articleCollectionProvider = service.GetRequiredService<IConduitArticlesCollectionProvider>();
        _usersCollectionProvider = service.GetRequiredService<IConduitUsersCollectionProvider>();
        _commentsCollectionProvider = service.GetRequiredService<IConduitCommentsCollectionProvider>();
        _followCollectionProvider = service.GetRequiredService<IConduitFollowsCollectionProvider>();

        // setup an authorized header
        _user = await _usersCollectionProvider.CreateUserInDatabase();
        var jwtToken = AuthSvc.GenerateJwtToken(_user.Email, _user.Username);
        WebClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", jwtToken);

        _random = new Random();
    }

    [Test]
    public async Task Comments_for_an_article_are_returned()
    {
        // arrange - an article
        var author = await _usersCollectionProvider.CreateUserInDatabase();
        var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: author.Username);
        // arrange - two comments from two different users
        var commenter1 = await _usersCollectionProvider.CreateUserInDatabase();
        var commenter2 = await _usersCollectionProvider.CreateUserInDatabase();
        var commentBody1 = _random.String(64);
        var commentBody2 = _random.String(64);
        await _commentsCollectionProvider.CreateCommentInDatabase(article.Slug, commenter1.Username, commentBody1);
        await _commentsCollectionProvider.CreateCommentInDatabase(article.Slug, commenter2.Username, commentBody2);

        // act
        var response = await WebClient.GetAsync($"/api/articles/{article.Slug}/comments");
        var responseString = await response.Content.ReadAsStringAsync();
        var articleViewModel = responseString.SubDoc<List<CommentListDataView>>("comments");

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(articleViewModel.Count, Is.EqualTo(2));
        await _commentsCollectionProvider.AssertExists(article.Slug, x =>
        {
            Assert.That(x.Any(c => c.Body == commentBody1));
            Assert.That(x.Any(c => c.Body == commentBody2));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Comments_include_correct_following_information(bool doesFollowAuthor)
    {
        // arrange - an article
        var author = await _usersCollectionProvider.CreateUserInDatabase();
        var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: author.Username);
        // arrange - comment
        var commenter = await _usersCollectionProvider.CreateUserInDatabase();
        await _commentsCollectionProvider.CreateCommentInDatabase(article.Slug, commenter.Username, _random.String(64));
        // arrange - follow (or not) the comment author
        if (doesFollowAuthor)
            await _followCollectionProvider.CreateFollow(commenter.Username, _user.Username);

        // act
        var response = await WebClient.GetAsync($"/api/articles/{article.Slug}/comments");
        var responseString = await response.Content.ReadAsStringAsync();
        var articleViewModel = responseString.SubDoc<List<CommentListDataView>>("comments");

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(articleViewModel.Count, Is.EqualTo(1));
        Assert.That(articleViewModel[0].Author.Following, Is.EqualTo(doesFollowAuthor));
    }
}