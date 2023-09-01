using Conduit.Tests.TestHelpers.Data;
using Conduit.Tests.TestHelpers.Dto;
using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Providers;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Microsoft.Extensions.DependencyInjection;
using Slugify;

namespace Conduit.Tests.Integration.Articles.Handlers;

[TestFixture]
public class CreateArticleHandlerTests : CouchbaseIntegrationTest
{
    private CreateArticleHandler _handler;
    private IConduitTagsCollectionProvider _tagsCollectionProvider;
    private TagsDataService _tagsDataService;
    private IConduitArticlesCollectionProvider _articleCollectionProvider;
    private ArticlesDataService _articleDataService;
    private IConduitFavoritesCollectionProvider _favoriteCollectionProvider;

    public override async Task Setup()
    {
        await base.Setup();

        ServiceCollection.AddCouchbaseBucket<IConduitBucketProvider>("ConduitIntegrationTests", b =>
        {
            b
                .AddScope("_default")
                .AddCollection<IConduitTagsCollectionProvider>("Tags");
            b
                .AddScope("_default")
                .AddCollection<IConduitArticlesCollectionProvider>("Articles");
            b
                .AddScope("_default")
                .AddCollection<IConduitFavoritesCollectionProvider>("Favorites");
        });

        _tagsCollectionProvider = ServiceProvider.GetRequiredService<IConduitTagsCollectionProvider>();
        _articleCollectionProvider = ServiceProvider.GetRequiredService<IConduitArticlesCollectionProvider>();
        _favoriteCollectionProvider = ServiceProvider.GetRequiredService<IConduitFavoritesCollectionProvider>();

        // setup handler and dependencies
        _tagsDataService = new TagsDataService(_tagsCollectionProvider);
        var validator = new CreateArticleRequestValidator(_tagsDataService);
        _articleDataService = new ArticlesDataService(_articleCollectionProvider, _favoriteCollectionProvider);
        var slugService = new SlugService(new SlugHelper());
        _handler = new CreateArticleHandler(validator, _articleDataService, slugService);
    }

    [Test]
    public async Task Article_is_created_given_valid_input()
    {
        // arrange
        var now = new DateTimeOffset(DateTime.Now);
        var request = CreateArticleRequestHelper.Create();

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        await _articleCollectionProvider.AssertExists(result.Article.Slug, a =>
        {
            Assert.That(a.Title, Is.EqualTo(request.ArticleSubmission.Article.Title));
            Assert.That(a.Body, Is.EqualTo(request.ArticleSubmission.Article.Body));
            Assert.That(a.Description, Is.EqualTo(request.ArticleSubmission.Article.Description));
            Assert.That(a.AuthorUsername, Is.EqualTo(request.AuthorUsername));
            Assert.That((a.CreatedAt - now).TotalSeconds, Is.GreaterThan(0));
            Assert.That(a.Favorited, Is.False);
            Assert.That(a.FavoritesCount, Is.EqualTo(0));
        });
    }
}