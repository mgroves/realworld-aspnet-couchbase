using Conduit.Tests.TestHelpers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.Services;
using Conduit.Web.Articles.ViewModels;
using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Providers;
using Couchbase.Query;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Articles.Services.ArticlesDataService;

[TestFixture]
public class GetArticlesTests : CouchbaseIntegrationTest
{
    private IConduitArticlesCollectionProvider _articleCollectionProvider;
    private IArticlesDataService _articleDataService;
    private IConduitFavoritesCollectionProvider _favoriteCollectionProvider;
    private IConduitUsersCollectionProvider _userCollectionProvider;
    private IConduitFollowsCollectionProvider _followCollectionProvider;
    private List<string> _keysToCleanup;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();

        _userCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();
        _articleCollectionProvider = ServiceProvider.GetRequiredService<IConduitArticlesCollectionProvider>();
        _favoriteCollectionProvider = ServiceProvider.GetRequiredService<IConduitFavoritesCollectionProvider>();
        _followCollectionProvider = ServiceProvider.GetRequiredService<IConduitFollowsCollectionProvider>();

        var articlesDataServiceWrapper = new ArticlesDataServiceWrapper(
            _articleCollectionProvider,
            _favoriteCollectionProvider);
        // set to RequestPlus, since being able to immediately verify is important for testing
        articlesDataServiceWrapper.ScanConsistencyValue = QueryScanConsistency.RequestPlus;

        _articleDataService = articlesDataServiceWrapper;

        // cleanup articles between tests
        _keysToCleanup = new List<string>();
    }

    [TearDown]
    public async Task Cleanup()
    {
        var coll = await _articleCollectionProvider.GetCollectionAsync();
        foreach (var key in _keysToCleanup)
            await coll.RemoveAsync(key);
    }

    [Test]
    public async Task Results_with_no_filters_no_authenticated_user()
    {
        // arrange - create articles so there's at least 20 in the db
        var author = await _userCollectionProvider.CreateUserInDatabase();
        for (var i = 0; i < 20; i++)
        {
            var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: author.Username);
            _keysToCleanup.Add(article.ArticleKey);
        }
        var request = new GetArticlesRequest(null, new ArticleFilterOptionsModel());

        // act
        var result = await _articleDataService.GetArticles(request);

        // assert
        Assert.That(result.Status, Is.EqualTo(DataResultStatus.Ok));
        Assert.That(result.DataResult.Count, Is.EqualTo(20));
    }

    [Test]
    public async Task Results_with_authenticated_user_favorited()
    {
        // arrange - create articles that are all favorited by a user
        var user = await _userCollectionProvider.CreateUserInDatabase();
        var author = await _userCollectionProvider.CreateUserInDatabase();
        for (var i = 0; i < 20; i++)
        {
            var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: author.Username);
            await _favoriteCollectionProvider.AddFavoriteInDatabase(user.Username, article.Slug);
            _keysToCleanup.Add(article.ArticleKey);
        }
        var request = new GetArticlesRequest(user.Username, new ArticleFilterOptionsModel());

        // act
        var results = await _articleDataService.GetArticles(request);

        // assert
        Assert.That(results.Status, Is.EqualTo(DataResultStatus.Ok));
        foreach (var result in results.DataResult)
        {
            Assert.That(result.Favorited, Is.True);
        }
    }

    [Test]
    public async Task Results_with_authenticated_user_following_authors()
    {
        // arrange - create authors that are all followed by a user
        var user = await _userCollectionProvider.CreateUserInDatabase();
        for (var i = 0; i < 20; i++)
        {
            var author = await _userCollectionProvider.CreateUserInDatabase();
            var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: author.Username);
            await _followCollectionProvider.CreateFollow(author.Username, user.Username);
            _keysToCleanup.Add(article.ArticleKey);
        }
        var request = new GetArticlesRequest(user.Username, new ArticleFilterOptionsModel());

        // act
        var results = await _articleDataService.GetArticles(request);

        // assert
        Assert.That(results.Status, Is.EqualTo(DataResultStatus.Ok));
        foreach (var result in results.DataResult)
        {
            Assert.That(result.Author.Following, Is.True);
        }
    }

    [Test]
    public async Task Results_with_tag_specified()
    {
        // arrange 20 articles that are tagged, 20 that aren't (striped)
        var cruisingTag = new List<string> { "cruising" };
        var baseballTag = new List<string> { "baseball" };
        var author = await _userCollectionProvider.CreateUserInDatabase();
        for (var i = 0; i < 20; i++)
        {
            var a1 = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: author.Username, tagList: cruisingTag);
            var a2 = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: author.Username, tagList: baseballTag);
            _keysToCleanup.Add(a1.ArticleKey);
            _keysToCleanup.Add(a2.ArticleKey);
        }
        var filter = new ArticleFilterOptionsModel();
        filter.Tag = "baseball";
        var request = new GetArticlesRequest(null, filter);

        // act
        var results = await _articleDataService.GetArticles(request);

        // assert
        Assert.That(results.Status, Is.EqualTo(DataResultStatus.Ok));
        Assert.That(results.DataResult.All(x => x.TagList.Contains("baseball")), Is.True);
        Assert.That(results.DataResult.All(x => !x.TagList.Contains("cruising")), Is.True);
    }

    [Test]
    public async Task Results_with_author_specified()
    {
        // arrange 20 articles that are by a certain author, 20 that aren't (striped)
        var authorTarget = await _userCollectionProvider.CreateUserInDatabase();
        var authorOther = await _userCollectionProvider.CreateUserInDatabase();
        for (var i = 0; i < 20; i++)
        {
            var a1 =await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: authorTarget.Username);
            var a2 = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: authorOther.Username);
            _keysToCleanup.Add(a1.ArticleKey);
            _keysToCleanup.Add(a2.ArticleKey);
        }
        var filter = new ArticleFilterOptionsModel();
        filter.Author = authorTarget.Username;
        var request = new GetArticlesRequest(null, filter);

        // act
        var results = await _articleDataService.GetArticles(request);

        // assert
        Assert.That(results.Status, Is.EqualTo(DataResultStatus.Ok));
        Assert.That(results.DataResult.All(x => x.Author.Username == authorTarget.Username), Is.True);
        Assert.That(results.DataResult.All(x => x.Author.Username != authorOther.Username), Is.True);
    }

    [Test]
    public async Task Results_with_favoritedBy_specified()
    {
        // arrange 20 articles that are favorited by a certain user, 20 that aren't (striped)
        var userFavorited = await _userCollectionProvider.CreateUserInDatabase();
        for (var i = 0; i < 20; i++)
        {
            var a1 = await _articleCollectionProvider.CreateArticleInDatabase();
            var article = await _articleCollectionProvider.CreateArticleInDatabase();
            await _favoriteCollectionProvider.AddFavoriteInDatabase(userFavorited.Username, article.Slug);
            _keysToCleanup.Add(a1.ArticleKey);
            _keysToCleanup.Add(article.ArticleKey);
        }
        var filter = new ArticleFilterOptionsModel();
        filter.Favorited = userFavorited.Username;
        var request = new GetArticlesRequest(null, filter);

        // act
        var results = await _articleDataService.GetArticles(request);

        // assert
        Assert.That(results.Status, Is.EqualTo(DataResultStatus.Ok));
        Assert.That(results.DataResult.All(x => x.Favorited), Is.True);
    }

    [TestCase(5)]
    [TestCase(55)]
    [TestCase(1)]
    public async Task Results_with_limit_specified(int limit)
    {
        // arrange limit (plus one) articles
        for (var i = 0; i < (limit+1); i++)
        {
            var author = await _userCollectionProvider.CreateUserInDatabase();
            var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: author.Username);
            _keysToCleanup.Add(article.ArticleKey);
        }

        var filter = new ArticleFilterOptionsModel();
        filter.Limit = limit;
        var request = new GetArticlesRequest(null, filter);

        // act
        var results = await _articleDataService.GetArticles(request);

        // assert
        Assert.That(results.Status, Is.EqualTo(DataResultStatus.Ok));
        Assert.That(results.DataResult.Count, Is.EqualTo(limit));
    }

    [Test]
    public async Task Results_with_offset_specified()
    {
        // arrange 5 articles (these will be expected in the results, since they are the oldest)
        var author = await _userCollectionProvider.CreateUserInDatabase();
        var expectedSlugs = new List<string>();
        for (var i = 0; i < 5; i++)
        {
            var article = await _articleCollectionProvider.CreateArticleInDatabase(
                authorUsername: author.Username);
            expectedSlugs.Add(article.Slug);
            _keysToCleanup.Add(article.ArticleKey);
        }
        // arrange 10 articles (these will be skipped in the results, since they are the newest)
        for (var i = 0; i < 10; i++)
        {
            var a = await _articleCollectionProvider.CreateArticleInDatabase(
                authorUsername: author.Username);
            _keysToCleanup.Add(a.ArticleKey);
        }

        var filter = new ArticleFilterOptionsModel();
        filter.Offset = 10;
        filter.Limit = 5;
        var request = new GetArticlesRequest(null, filter);

        // act
        var results = await _articleDataService.GetArticles(request);

        // assert
        Assert.That(results.Status, Is.EqualTo(DataResultStatus.Ok));
        Assert.That(results.DataResult.Count, Is.EqualTo(5));
        foreach (var result in results.DataResult)
            Assert.That(expectedSlugs.Any(e => e == result.Slug), Is.True);
    }
}