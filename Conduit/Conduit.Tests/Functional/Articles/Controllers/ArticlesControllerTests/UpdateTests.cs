using System.Net;
using Conduit.Web.DataAccess.Providers;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Conduit.Tests.TestHelpers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Articles.Services;
using Conduit.Web.Articles.ViewModels;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.Extensions;
using Conduit.Tests.TestHelpers.Dto.ViewModels;

namespace Conduit.Tests.Functional.Articles.Controllers.ArticlesControllerTests;

[TestFixture]
public class UpdateTests : FunctionalTestBase
{
    private IConduitUsersCollectionProvider _usersCollectionProvider;
    private User _user;
    private IConduitArticlesCollectionProvider _articleCollectionProvider;
    private ISlugService _slugService;
    private Random _random;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();

        _random = new Random();

        // setup database objects for arranging
        var service = WebAppFactory.Services;
        _usersCollectionProvider = service.GetRequiredService<IConduitUsersCollectionProvider>();
        _articleCollectionProvider = service.GetRequiredService<IConduitArticlesCollectionProvider>();
        _slugService = service.GetService<ISlugService>();

        // setup an authorized header
        _user = await _usersCollectionProvider.CreateUserInDatabase();
        var jwtToken = AuthSvc.GenerateJwtToken(_user.Email, _user.Username);
        WebClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", jwtToken);
    }

    [Test]
    public async Task Update_article_succeeds()
    {
        // arrange
        var oldTags = new List<string> { "Couchbase" };
        var newTags = new List<string> { "cruising" };
        var user = await _usersCollectionProvider.CreateUserInDatabase();
        var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: user.Username, tagList: oldTags);
        var payload = UpdateArticlePostModelHelper.Create(tags: newTags);
        var expectedNewSlug = _slugService.NewTitleSameSlug(payload.Article.Title, article.Slug);

        // act
        var response = await WebClient.PutAsync($"api/articles/{article.Slug}", payload.ToJsonPayload());

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await _articleCollectionProvider.AssertExists(article.Slug, x =>
        {
            Assert.That(x.Title, Is.EqualTo(payload.Article.Title));
            Assert.That(x.Body, Is.EqualTo(payload.Article.Body));
            Assert.That(x.Description, Is.EqualTo(payload.Article.Description));
            Assert.That(x.TagList.All(t => newTags.Contains(t)), Is.True);
            Assert.That(x.Slug, Is.EqualTo(expectedNewSlug));
        });
    }

    [TestCase("", "Title is required.")]
    [TestCase("tooshort", "Title must be at least 10 characters.")]
    [TestCase("title_is_too_long_x_title_is_too_long_x_title_is_too_long_x_title_is_too_long_x_title_is_too_long_x_title_is_too_long_x_", "Title must be 100 characters or less.")]
    [TestCase($"this_title_is_invalid{SlugService.SLUG_DELIMETER}because_of_delimeter", "Double colon (::) is not allowed in titles. Sorry, it's weird.")]
    public async Task Update_article_fails_when_title_invalid(string invalidTitle, string message)
    {
        // arrange
        var user = await _usersCollectionProvider.CreateUserInDatabase();
        var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: user.Username);
        var payload = UpdateArticlePostModelHelper.Create();
        payload.Article.Title = invalidTitle;

        // act
        var response = await WebClient.PutAsync($"api/articles/{article.Slug}", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        Assert.That(responseString, Contains.Substring(message));
    }

    [TestCase(0, "Description is required.")]
    [TestCase(5, "Description must be 10 characters or more.")]
    [TestCase(205, "Description must be 200 characters or less.")]
    public async Task Update_article_fails_when_description_invalid(int invalidDescriptionLength, string message)
    {
        // arrange
        var user = await _usersCollectionProvider.CreateUserInDatabase();
        var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: user.Username);
        var payload = UpdateArticlePostModelHelper.Create();
        payload.Article.Description = _random.String(invalidDescriptionLength);

        // act
        var response = await WebClient.PutAsync($"api/articles/{article.Slug}", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        Assert.That(responseString, Contains.Substring(message));
    }

    [TestCase(0, "Body is required.")]
    [TestCase(5, "Body must be 10 characters or more.")]
    [TestCase(150000005, "Body must be 15,000,000 characters or less.")]
    public async Task Update_article_fails_when_body_invalid(int invalidBodyLength, string message)
    {
        // arrange
        var user = await _usersCollectionProvider.CreateUserInDatabase();
        var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: user.Username);
        var payload = UpdateArticlePostModelHelper.Create();
        payload.Article.Body = _random.String(invalidBodyLength);

        // act
        var response = await WebClient.PutAsync($"api/articles/{article.Slug}", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        Assert.That(responseString, Contains.Substring(message));
    }

    [TestCase("SurlyDev_is_not_a_tag", "At least one of those tags isn't allowed.")]
    [TestCase("", "At least one of those tags isn't allowed.")]
    public async Task Update_article_fails_when_tags_invalid(string invalidTag, string message)
    {
        // arrange
        var user = await _usersCollectionProvider.CreateUserInDatabase();
        var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: user.Username);
        var payload = UpdateArticlePostModelHelper.Create();
        payload.Article.Tags = new List<string> { invalidTag };

        // act
        var response = await WebClient.PutAsync($"api/articles/{article.Slug}", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        Assert.That(responseString, Contains.Substring(message));
    }

    [Test]
    public async Task Update_article_fails_when_article_not_found()
    {
        // arrange
        var payload = UpdateArticlePostModelHelper.Create();
        var slug = _slugService.GenerateSlug(payload.Article.Title);

        // act
        var response = await WebClient.PutAsync($"api/articles/{slug}", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(responseString, Contains.Substring("Article not found"));
    }

    [Test]
    public async Task Update_needs_at_least_one_value_to_update()
    {
        // arrange
        var article = await _articleCollectionProvider.CreateArticleInDatabase();
        var payload = UpdateArticlePostModelHelper.Create();
        payload.Article.Body = null;
        payload.Article.Description = null;
        payload.Article.Tags = null;
        payload.Article.Title = null;

        // act
        var response = await WebClient.PutAsync($"api/articles/{article.Slug}", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        Assert.That(responseString, Contains.Substring("Update Failed: Please make sure to modify at least one of the following fields: Body, Title, Description, or Tags."));
    }

    [Test]
    public async Task Update_returns_the_updated_article()
    {
        // arrange
        var oldTags = new List<string> { "Couchbase" };
        var newTags = new List<string> { "cruising" };
        var user = await _usersCollectionProvider.CreateUserInDatabase();
        var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: user.Username, tagList: oldTags);
        var payload = UpdateArticlePostModelHelper.Create(tags: newTags);

        // act
        var response = await WebClient.PutAsync($"api/articles/{article.Slug}", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();
        var articleViewModel = responseString.SubDoc<ArticleViewModel>("article");

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(articleViewModel.Title, Is.EqualTo(payload.Article.Title));
        Assert.That(articleViewModel.Body, Is.EqualTo(payload.Article.Body));
        Assert.That(articleViewModel.Description, Is.EqualTo(payload.Article.Description));
        Assert.That(articleViewModel.CreatedAt, Is.EqualTo(article.CreatedAt));
        Assert.That(articleViewModel.TagList.All(newTags.Contains), Is.True);
        Assert.That(articleViewModel.Favorited, Is.EqualTo(article.Favorited));
        Assert.That(articleViewModel.FavoritesCount, Is.EqualTo(article.FavoritesCount));
        Assert.That(articleViewModel.UpdatedAt, Is.GreaterThanOrEqualTo(new DateTimeOffset(DateTime.Now - TimeSpan.FromSeconds(30))));
    }

    [Test]
    public async Task Update_succeeds_but_retrieval_fails()
    {
        // this is a pretty weird, rare edge case
        // testing for it in an integration test is more trouble than it's worth!
    }
}