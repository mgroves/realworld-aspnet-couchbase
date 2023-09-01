using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Extensions;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Articles.Services.ArticlesDataService;

[TestFixture]
public class FavoriteTests : CouchbaseIntegrationTest
{
    private IConduitArticlesCollectionProvider _articleCollectionProvider;
    private IConduitFavoritesCollectionProvider _favoriteCollectionProvider;
    private Web.Articles.Services.ArticlesDataService _articleDataService;

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
        });

        _articleCollectionProvider = ServiceProvider.GetRequiredService<IConduitArticlesCollectionProvider>();
        _favoriteCollectionProvider = ServiceProvider.GetRequiredService<IConduitFavoritesCollectionProvider>();

        _articleDataService = new Web.Articles.Services.ArticlesDataService(
            _articleCollectionProvider,
            _favoriteCollectionProvider);
    }

    [TestCase(0,1)]
    [TestCase(73,74)]
    public async Task Favoriting_works_and_increases_count(int initialCount, int expectedCount)
    {
        // arrange
        var article = await _articleCollectionProvider.CreateArticleInDatabase(favoritesCount: initialCount);
        var user = UserHelper.CreateUser();

        // act
        await _articleDataService.Favorite(article.Slug, user.Username);

        // assert
        await _favoriteCollectionProvider.AssertExists(user.Username, x =>
        {
            Assert.That(x.Contains(article.Slug.GetArticleKey()), Is.True);
        });
        await _articleCollectionProvider.AssertExists(article.Slug, x =>
        {
            Assert.That(x.FavoritesCount, Is.EqualTo(expectedCount));
        });
    }
}