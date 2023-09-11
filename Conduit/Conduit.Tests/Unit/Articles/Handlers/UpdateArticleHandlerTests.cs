using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.Services;
using Moq;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Articles.ViewModels;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.Extensions;
using Conduit.Web.DataAccess.Dto;
using Conduit.Tests.TestHelpers.Dto.Handlers;
using Conduit.Tests.TestHelpers.Dto.ViewModels;

namespace Conduit.Tests.Unit.Articles.Handlers;

[TestFixture]
public class UpdateArticleHandlerTests
{
    private UpdateArticleHandler _handler;
    private Mock<IArticlesDataService> _articlesDataServiceMock;
    private Mock<ISlugService> _slugServiceMock;
    private Mock<ITagsDataService> _tagsDataServiceMock;
    private Random _random;

    [SetUp]
    public async Task Setup()
    {
        _random = new Random();

        _articlesDataServiceMock = new Mock<IArticlesDataService>();
        _slugServiceMock = new Mock<ISlugService>();
        _tagsDataServiceMock = new Mock<ITagsDataService>();
        _tagsDataServiceMock.Setup(t => t.GetAllTags())
            .ReturnsAsync(new List<string> { "tag1", "tag2" });

        // assumption: article Get always returns something
        _articlesDataServiceMock.Setup(m => m.Get(It.IsAny<string>()))
            .ReturnsAsync(new DataServiceResult<Article> (ArticleHelper.CreateArticle(), DataResultStatus.Ok));
        // assumption: always allowed to update
        _articlesDataServiceMock.Setup(m => m.IsArticleAuthor(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _handler = new UpdateArticleHandler(
            _articlesDataServiceMock.Object,
            _slugServiceMock.Object,
            new UpdateArticleRequestValidator(_tagsDataServiceMock.Object));
    }

    [TestCase("", "Title is required.")]
    [TestCase("aaa", "Title must be at least 10 characters.")]
    [TestCase("00000000001111111111222222222233333333334444444444555555555566666666667777777778888888888999999999900000000001111111111", "Title must be 100 characters or less.")]
    public async Task Title_can_be_null_but_otherwise_must_be_valid(string title, string expectedErrorMessage)
    {
        // arrange
        var model = UpdateArticlePostModelHelper.Create(title: title);
        var request = UpdateArticleRequestHelper.Create(model: model);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Any(e => e.ErrorMessage == expectedErrorMessage));
    }

    // kinda hacky, but I'm using -1 here to indicate "null" so I can use TestCase
    [TestCase(0, "Description is required.")]
    [TestCase(5, "Description must be 10 characters or more.")]
    [TestCase(201, "Description must be 200 characters or less.")]
    public async Task Description_can_be_null_but_otherwise_must_be_valid(int strLength, string expectedErrorMessage)
    {
        // arrange
        var model = UpdateArticlePostModelHelper.Create(description: _random.String(strLength));
        var request = UpdateArticleRequestHelper.Create(model: model);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Any(e => e.ErrorMessage == expectedErrorMessage));
    }

    [TestCase(0, "Body is required.")]
    [TestCase(5, "Body must be 10 characters or more.")]
    [TestCase(20000000, "Body must be 15,000,000 characters or less.")]
    public async Task Body_can_be_null_must_be_valid(int strLength, string expectedErrorMessage)
    {
        // arrange
        var model = UpdateArticlePostModelHelper.Create(body: _random.String(strLength));
        var request = UpdateArticleRequestHelper.Create(model: model);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Any(e => e.ErrorMessage == expectedErrorMessage));
    }

    [Test]
    public async Task Tags_if_specified_must_be_on_the_approved_list()
    {
        // arrange
        var model = UpdateArticlePostModelHelper.Create(tags: new List<string> {$"not-approved-tag{_random.String(5)}"});
        var request = UpdateArticleRequestHelper.Create(model: model);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Any(e => e.ErrorMessage == "At least one of those tags isn't allowed."));
    }

    [Test]
    public async Task Title_if_specified_is_trimmed()
    {
        // arrange
        var model = UpdateArticlePostModelHelper.Create(title: " this has spaces that need trimmed ");
        model.Article.Tags = null;
        var request = UpdateArticleRequestHelper.Create(model: model);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        _articlesDataServiceMock.Verify(m => m.UpdateArticle(
            It.Is<Article>(a => a.Title == "this has spaces that need trimmed")));
    }

    [Test]
    public async Task Body_if_specified_is_trimmed()
    {
        // arrange
        var model = UpdateArticlePostModelHelper.Create(body: " this body has spaces that need trimmed ");
        model.Article.Tags = null;
        var request = UpdateArticleRequestHelper.Create(model: model);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        _articlesDataServiceMock.Verify(m => m.UpdateArticle(
            It.Is<Article>(a => a.Body == "this body has spaces that need trimmed")));
    }

    [Test]
    public async Task Description_if_specified_is_trimmed()
    {
        // arrange
        var model = UpdateArticlePostModelHelper.Create(description: " this description has spaces that need trimmed ");
        model.Article.Tags = null;
        var request = UpdateArticleRequestHelper.Create(model: model);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        _articlesDataServiceMock.Verify(m => m.UpdateArticle(
            It.Is<Article>(a => a.Description == "this description has spaces that need trimmed")));
    }

    [Test]
    public async Task If_title_is_updated_Slug_service_is_used_to_generate_from_title()
    {
        // arrange
        var newTitle = $"this is the new title {_random.String(5)}";
        var model = UpdateArticlePostModelHelper.Create(title: newTitle);
        model.Article.Tags = null;
        var request = UpdateArticleRequestHelper.Create(model: model);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        _slugServiceMock.Verify(m => m.NewTitleSameSlug(newTitle, It.IsAny<string>()));
    }

    [Test]
    public async Task If_specified_the_tag_list_is_passed_through()
    {
        // arrange
        var model = UpdateArticlePostModelHelper.Create();
        model.Article.Tags = new List<string> { "tag1"};
        var request = UpdateArticleRequestHelper.Create(model: model);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        _articlesDataServiceMock.Verify(m => m.UpdateArticle(
            It.Is<Article>(a => a.TagList.All(model.Article.Tags.Contains))));
    }

    [Test]
    public async Task All_fields_are_optional_and_null_means_no_change_but_at_least_one_field_should_be_updated()
    {
        // arrange
        var model = new UpdateArticlePostModelArticle();
        model.Article = new UpdateArticlePostModel
        {
            Body = null,
            Description = null,
            Tags = null,
            Title = null
        };
        var request = UpdateArticleRequestHelper.Create(model: model);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Any(e => e.ErrorMessage == "Update Failed: Please make sure to modify at least one of the following fields: Body, Title, Description, or Tags."));
    }

    [Test]
    public async Task Returns_NotFound_if_article_doesnt_exist()
    {
        // arrange
        var model = UpdateArticlePostModelHelper.Create();
        model.Article.Tags = new List<string> { "tag1" };
        var request = UpdateArticleRequestHelper.Create(model: model);
        _articlesDataServiceMock.Setup(m => m.Get(request.Slug))
            .ReturnsAsync(new DataServiceResult<Article>(null, DataResultStatus.NotFound));

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsNotFound, Is.True);
    }
    
    [Test]
    public async Task Returns_NotAuthorized_if_non_author_is_trying_to_update()
    {
        // arrange
        var model = UpdateArticlePostModelHelper.Create();
        model.Article.Tags = new List<string> { "tag1" };
        var request = UpdateArticleRequestHelper.Create(model: model);
        _articlesDataServiceMock.Setup(m => m.IsArticleAuthor(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsNotAuthorized, Is.True);
    }
}