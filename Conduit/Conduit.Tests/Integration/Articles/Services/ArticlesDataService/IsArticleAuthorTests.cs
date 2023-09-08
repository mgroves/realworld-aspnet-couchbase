using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Extensions;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Articles.Services.ArticlesDataService;

[TestFixture]
public class IsArticleAuthorTests : CouchbaseIntegrationTest
{
    private IConduitArticlesCollectionProvider _articleCollectionProvider;
    private IArticlesDataService _articleDataService;
    private IConduitFavoritesCollectionProvider _favoriteCollectionProvider;
    private IConduitUsersCollectionProvider _userCollectionProvider;
    private Random _random;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();

        ServiceCollection.AddCouchbaseBucket<IConduitBucketProvider>("ConduitIntegrationTests", b =>
        {
            b
                .AddScope("_default")
                .AddCollection<IConduitArticlesCollectionProvider>("Articles");
            b
                .AddScope("_default")
                .AddCollection<IConduitFavoritesCollectionProvider>("Favorites");
            b
                .AddScope("_default")
                .AddCollection<IConduitUsersCollectionProvider>("Users");
        });

        _articleCollectionProvider = ServiceProvider.GetRequiredService<IConduitArticlesCollectionProvider>();
        _favoriteCollectionProvider = ServiceProvider.GetRequiredService<IConduitFavoritesCollectionProvider>();
        _userCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();

        _articleDataService = new Web.Articles.Services.ArticlesDataService(
            _articleCollectionProvider,
            _favoriteCollectionProvider);

        _random = new Random();
    }

    [Test]
    public async Task Returns_true_if_author_matches_given_username()
    {
        // arrange
        var user = await _userCollectionProvider.CreateUserInDatabase();
        var article = await _articleCollectionProvider.CreateArticleInDatabase(authorUsername: user.Username);

        // act
        var result = await _articleDataService.IsArticleAuthor(article.Slug, user.Username);

        // assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task Returns_false_if_author_matches_given_username()
    {
        // arrange
        var username = _random.String(12);
        var article = await _articleCollectionProvider.CreateArticleInDatabase();

        // act
        var result = await _articleDataService.IsArticleAuthor(article.Slug, username);

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task Returns_false_if_article_doesnt_exist()
    {
        // arrange
        var username = _random.String(12);
        var articleSlug = _random.String(12) + SlugService.SLUG_DELIMETER + _random.String(10);

        // act
        var result = await _articleDataService.IsArticleAuthor(articleSlug, username);

        // assert
        Assert.That(result, Is.False);
    }
}