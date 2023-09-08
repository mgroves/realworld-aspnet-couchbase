using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Dto;
using Moq;

namespace Conduit.Tests.Unit.Articles.Handlers;

[TestFixture]
public class ArticleDeleteHandlerTests
{
    private Mock<IArticlesDataService> _articleDataServiceMock;
    private ArticleDeleteHandler _handler;

    [SetUp]
    public async Task Setup()
    {
        _articleDataServiceMock = new Mock<IArticlesDataService>();
        //_random = new Random();

        _handler = new ArticleDeleteHandler(
            _articleDataServiceMock.Object,
            new ArticleDeleteRequestValidator());

        // by default, give user is always author
        _articleDataServiceMock.Setup(m => m.IsArticleAuthor(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
    }

    [Test]
    public async Task Deleting_article_via_slug()
    {
        // arrange
        var slug = "a-slug::abc123abc";
        var request = new ArticleDeleteRequest(slug, "a-username");
        _articleDataServiceMock.Setup(m => m.DeleteArticle(slug))
            .ReturnsAsync(new DataServiceResult<string>(slug, DataResultStatus.Ok));

        // act
        var response = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(response.ArticleNotFound, Is.False);
        Assert.That(response.IsUnauthorized, Is.False);
        Assert.That(response.ValidationErrors == null || !response.ValidationErrors.Any(), Is.True);
        _articleDataServiceMock.Verify(m => m.DeleteArticle(slug), Times.Once);
    }

    [Test]
    public async Task Not_allowed_to_delete_someone_elses_article()
    {
        // arrange
        var slug = "a-slug::abc123abc";
        var request = new ArticleDeleteRequest(slug, "a-username");
        _articleDataServiceMock.Setup(m => m.IsArticleAuthor(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // act
        var response = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(response.IsUnauthorized, Is.True);
    }

    [Test]
    public async Task Cant_delete_an_article_that_doesnt_exist()
    {
        // arrange
        var slug = "a-slug::abc123abc";
        var request = new ArticleDeleteRequest(slug, "a-username");
        _articleDataServiceMock.Setup(m => m.DeleteArticle(request.Slug))
            .ReturnsAsync(new DataServiceResult<string>(slug, DataResultStatus.NotFound));

        // act
        var response = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(response.ArticleNotFound, Is.True);
    }
}