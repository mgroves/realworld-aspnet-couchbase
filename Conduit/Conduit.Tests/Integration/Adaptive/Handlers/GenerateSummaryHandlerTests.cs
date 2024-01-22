using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Adaptive.Handlers;
using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Extensions;
using Conduit.Web.Users.Services;
using Microsoft.Extensions.DependencyInjection;
using Slugify;

namespace Conduit.Tests.Integration.Adaptive.Handlers;

public class GenerateSummaryHandlerTests : CouchbaseIntegrationTest
{
    private GenerateSummaryHandler _handler;
    private ISlugService _slugService;
    private IGenerativeAiService _genAiService;
    private IConduitArticlesCollectionProvider _articleCollectionProvider;
    private IConduitFavoritesCollectionProvider _favoriteCollectionProvider;
    private ArticlesDataService _articlesDataService;

    public override async Task Setup()
    {
        await base.Setup();

        _articleCollectionProvider = ServiceProvider.GetRequiredService<IConduitArticlesCollectionProvider>();
        _favoriteCollectionProvider = ServiceProvider.GetRequiredService<IConduitFavoritesCollectionProvider>();
        _genAiService = ServiceProvider.GetRequiredService<IGenerativeAiService>();

        _slugService = new SlugService(new SlugHelper());
        _articlesDataService = new ArticlesDataService(_articleCollectionProvider, _favoriteCollectionProvider);

        _handler = new GenerateSummaryHandler(_genAiService, _articlesDataService, _slugService);
    }

    [Test]
    public async Task Creates_a_summary_article()
    {
        // arrange
        var request = new GenerateSummaryRequest();
        request.RawData = "(pretend like there's a bunch of interesting article data here, but you can make up test data)";
        request.Tag = "testing";

        // act
        var response = await _handler.Handle(request, CancellationToken.None);

        // assert
        await _articleCollectionProvider.AssertExists(response.Slug);
    }

    [Test]
    public async Task Article_is_marked_as_AI_generated_with_botname_as_author()
    {
        // arrange
        var request = new GenerateSummaryRequest();
        request.RawData = "(pretend like there's a bunch of interesting article data here, but you can make up test data)";
        request.Tag = "testing";

        // act
        var response = await _handler.Handle(request, CancellationToken.None);

        // assert
        await _articleCollectionProvider.AssertExists(response.Slug, article =>
        {
            Assert.That(article.AuthorUsername, Is.EqualTo("summary-bot"));
            Assert.That(article.TagList, Contains.Item("ai-generated"));
        });
    }
}