using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.Services;
using Moq;

namespace Conduit.Tests.Unit.Articles.Handlers;

[TestFixture]
public class FavoriteArticleHandlerTests
{
    private Mock<IArticlesDataService> _articleDataServiceMock;
    private FavoriteArticleHandler _handler;

    [SetUp]
    public async Task Setup()
    {
        _articleDataServiceMock = new Mock<IArticlesDataService>();

        _handler = new FavoriteArticleHandler(
            _articleDataServiceMock.Object);
    }

    [Test]
    public async Task If_article_exists_it_can_be_favorited()
    {
        // arrange
        var slug = "valid-slug::a8as9d89ads";
        var username = "valid-username";
        _articleDataServiceMock.Setup(m => m.Exists(slug))
            .ReturnsAsync(true);
        var request = new FavoriteArticleRequest
        {
            Slug = slug,
            Username = username
        };

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result.ValidationErrors == null || !result.ValidationErrors.Any(), Is.True);
    }

    [Test]
    public async Task If_article_doest_exists_then_invalid_request()
    {
        // arrange
        var slug = "valid-slug::f9ad09fads";
        var username = "valid-username-too";
        _articleDataServiceMock.Setup(m => m.Exists(slug))
            .ReturnsAsync(false);
        var request = new FavoriteArticleRequest
        {
            Slug = slug,
            Username = username
        };

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors, Is.Not.Empty);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors[0].ErrorMessage, Is.EqualTo("Article not found."));
    }
}