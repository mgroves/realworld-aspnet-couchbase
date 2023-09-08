using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.DataAccess.Providers;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Conduit.Web.Articles.Services;

namespace Conduit.Tests.Integration.Articles.Services.ArticlesDataService;

public class UpdateArticleTests : CouchbaseIntegrationTest
{
    private IConduitArticlesCollectionProvider _articleCollectionProvider;
    private IArticlesDataService _articleDataService;
    private IConduitFavoritesCollectionProvider _favoriteCollectionProvider;

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

    [Test]
    public async Task Updated_at_should_be_updated_to_current_timestamp()
    {
        // arrange
        var articleInDatabase = await _articleCollectionProvider.CreateArticleInDatabase();
        articleInDatabase.Body += "something has to be updated";

        // act
        var result = await _articleDataService.UpdateArticle(articleInDatabase);

        // assert
        Assert.That(result, Is.True);
        await _articleCollectionProvider.AssertExists(articleInDatabase.Slug, x =>
        {
            Assert.That(x.CreatedAt, Is.EqualTo(articleInDatabase.CreatedAt));
            Assert.That((new DateTimeOffset(DateTime.Now) - x.UpdatedAt).TotalSeconds, Is.LessThanOrEqualTo(30));
        });
    }
}