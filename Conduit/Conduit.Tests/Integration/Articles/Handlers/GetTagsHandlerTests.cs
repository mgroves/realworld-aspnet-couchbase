using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Articles.Handlers;

[TestFixture]
public class GetTagsHandlerTests : CouchbaseIntegrationTest
{
    private GetTagsHandler _handler;
    private IConduitTagsCollectionProvider _tagsCollectionProvider;

    [SetUp]
    public async Task SetUp()
    {
        await base.Setup();

        _tagsCollectionProvider = ServiceProvider.GetRequiredService<IConduitTagsCollectionProvider>();

        // setup handler and dependencies
        _handler = new GetTagsHandler(new TagsDataService(_tagsCollectionProvider));
    }

    [Test]
    public async Task Handle_ReturnsGetTagsResult()
    {
        // Arrange
        var request = new GetTagsRequest();

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.That(result.Tags.Contains("cruising"), Is.True);
    }
}
