using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Articles.Services.ArticlesDataService;

[TestFixture]
public class FavoriteTests : CouchbaseIntegrationTest
{
    private IConduitArticlesCollectionProvider _articleCollectionProvider;
    private IConduitFavoritesCollectionProvider _favoriteCollectionProvider;
    private IArticlesDataService _articleDataService;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();

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

    [Test]
    [Ignore("TXNN-134 see comments")]
    public async Task Favoriting_an_already_favorited_article_does_not_change_the_favorites()
    {
        // might be a BUG https://issues.couchbase.com/browse/TXNN-134
        // or could be user error - ignoring this test until I can resolve that issue

        // arrange
        var article = await _articleCollectionProvider.CreateArticleInDatabase();
        var user = UserHelper.CreateUser();
        await _articleDataService.Favorite(article.Slug, user.Username);

        // act
        await _articleDataService.Favorite(article.Slug, user.Username);

        // assert
        await _favoriteCollectionProvider.AssertExists(user.Username, x =>
        {
            Assert.That(x.Count, Is.EqualTo(1));
        });
    }
}