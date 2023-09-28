using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.Services;
using Conduit.Web.Articles.ViewModels;
using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Dto.Articles;
using Moq;

namespace Conduit.Tests.Unit.Articles.Handlers;

[TestFixture]
public class GetArticlesHandlerTests
{
    private Mock<IArticlesDataService> _mockArticleDataService;
    private GetArticlesHandler _handler;

    [SetUp]
    public async Task Setup()
    {
        _mockArticleDataService = new Mock<IArticlesDataService>();
        _mockArticleDataService.Setup(m => m.GetArticles(It.IsAny<GetArticlesSpec>()))
            .ReturnsAsync(new DataServiceResult<ArticlesViewModel>(new ArticlesViewModel(), DataResultStatus.Ok));

        var validator = new GetArticlesRequestValidator();
        _handler = new GetArticlesHandler(validator, _mockArticleDataService.Object);
    }

    [TestCase(-128, "Limit must be greater than 0.")]
    [TestCase(-1, "Limit must be greater than 0.")]
    [TestCase(0, "Limit must be greater than 0.")]
    [TestCase(51, "Limit must be less than or equal to 50.")]
    [TestCase(int.MaxValue, "Limit must be less than or equal to 50.")]
    public async Task Limit_value_must_be_valid(int invalidLimit, string expectedMessage)
    {
        // arrange
        var filter = new ArticleFilterOptionsModel();
        filter.Limit = invalidLimit;
        var request = new GetArticlesRequest("doesnt-matter", filter);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors[0].ErrorMessage, Is.EqualTo(expectedMessage));
    }

    [TestCase(-128, "Offset must be greater than or equal to 0.")]
    [TestCase(-1, "Offset must be greater than or equal to 0.")]
    public async Task Offset_value_must_be_valid(int invalidOffset, string expectedMessage)
    {
        // arrange
        var filter = new ArticleFilterOptionsModel();
        filter.Offset = invalidOffset;
        var request = new GetArticlesRequest("doesnt-matter", filter);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors[0].ErrorMessage, Is.EqualTo(expectedMessage));
    }

    [Test]
    public async Task Handler_calls_data_service_with_cleaned_up_filter()
    {
        // arrange
        var loggedInUser = " current-user-needs-trimmed ";
        var filter = new ArticleFilterOptionsModel();
        filter.Author = " author-this-needs-trimmed ";
        filter.Favorited = " favortedby-needs-trimmed ";
        filter.Tag = " tag-needs-trimmed ";
        var request = new GetArticlesRequest(loggedInUser, filter);

        // act
        await _handler.Handle(request, CancellationToken.None);

        // assert
        _mockArticleDataService.Verify(x => x.GetArticles(It.Is<GetArticlesSpec>(
            f =>
                f.Username == "current-user-needs-trimmed"
                       && f.AuthorUsername == "author-this-needs-trimmed"
                       && f.FavoritedByUsername == "favortedby-needs-trimmed"
                       && f.Tag == "tag-needs-trimmed"
            )));
    }

    [Test]
    public async Task Handler_returns_failure_if_getting_articles_data_is_not_OK()
    {
        // arrange
        var filter = new ArticleFilterOptionsModel();
        var request = new GetArticlesRequest("doesnt-matter", filter);
        _mockArticleDataService.Setup(m => m.GetArticles(It.IsAny<GetArticlesSpec>()))
            .ReturnsAsync(new DataServiceResult<ArticlesViewModel>(null, DataResultStatus.Error));

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result.IsFailure, Is.True);
    }
}