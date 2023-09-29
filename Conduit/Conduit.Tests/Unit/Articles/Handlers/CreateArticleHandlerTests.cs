using Conduit.Tests.TestHelpers.Dto.Handlers;
using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.Services;
using Conduit.Web.Extensions;
using Moq;

namespace Conduit.Tests.Unit.Articles.Handlers;

[TestFixture]
public class CreateArticleHandlerTests
{
    private CreateArticleHandler _handler;
    private Mock<ITagsDataService> _tagsDataServiceMock;
    private List<string> _allTags;
    private Random _random;
    private Mock<IArticlesDataService> _articleDataServiceMock;
    private Mock<ISlugService> _slugServiceMock;

    [SetUp]
    public async Task Setup()
    {
        _tagsDataServiceMock = new Mock<ITagsDataService>();
        _articleDataServiceMock = new Mock<IArticlesDataService>();
        _slugServiceMock = new Mock<ISlugService>();
        _allTags = new List<string> { "Couchbase", "cruising" };
        _random = new Random();

        _handler = new CreateArticleHandler(
            new CreateArticleRequestValidator(_tagsDataServiceMock.Object),
            _articleDataServiceMock.Object,
            _slugServiceMock.Object);
        _tagsDataServiceMock.Setup(m => m.GetAllTags()).ReturnsAsync(_allTags);
    }

    [TestCase("", "Title is required.")]
    [TestCase(null, "Title is required.")]
    [TestCase("aaa", "Title must be at least 10 characters.")]
    [TestCase("00000000001111111111222222222233333333334444444444555555555566666666667777777778888888888999999999900000000001111111111", "Title must be 100 characters or less.")]
    public async Task Title_must_be_valid(string title, string expectedErrorMessage)
    {
        // arrange
        var request = CreateArticleRequestHelper.Create();
        request.ArticleSubmission.Article.Title = title;

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Any(e => e.ErrorMessage == expectedErrorMessage));
    }

    // kinda hacky, but I'm using -1 here to indicate "null" so I can use TestCase
    [TestCase(-1, "Description is required.")]
    [TestCase(0, "Description is required.")]
    [TestCase(5, "Description must be 10 characters or more.")]
    [TestCase(201, "Description must be 200 characters or less.")]
    public async Task Description_must_be_valid(int strLength, string expectedErrorMessage)
    {
        // arrange
        var request = CreateArticleRequestHelper.Create();
        request.ArticleSubmission.Article.Description = 
            strLength == -1
                ? null 
                : _random.String(strLength);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Any(e => e.ErrorMessage == expectedErrorMessage));
    }

    // kinda hacky, but I'm using -1 here to indicate "null" so I can use TestCase
    [TestCase(-1, "Body is required.")]
    [TestCase(0, "Body is required.")]
    [TestCase(5, "Body must be 10 characters or more.")]
    [TestCase(20000000, "Body must be 15,000,000 characters or less.")]
    public async Task Body_must_be_valid(int strLength, string expectedErrorMessage)
    {
        // arrange
        var request = CreateArticleRequestHelper.Create();
        request.ArticleSubmission.Article.Body =
            strLength == -1
                ? null
                : _random.String(strLength);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Any(e => e.ErrorMessage == expectedErrorMessage));
    }

    [Test]
    public async Task Tags_must_be_on_the_approved_list()
    {
        // arrange
        var request = CreateArticleRequestHelper.Create();
        request.ArticleSubmission.Article.TagList = new List<string>
        {
            "not-approved-tag-" + Path.GetRandomFileName()
        };

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Any(e => e.ErrorMessage == "At least one of those tags isn't allowed."));
    }

    [Test]
    public async Task Title_is_trimmed()
    {
        // arrange
        var request = CreateArticleRequestHelper.Create();
        request.ArticleSubmission.Article.Title = " this has spaces that need trimmed ";

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result,Is.Not.Null);
        Assert.That(result.Article.Title, Is.EqualTo("this has spaces that need trimmed"));
    }

    [Test]
    public async Task Body_is_trimmed()
    {
        // arrange
        var request = CreateArticleRequestHelper.Create();
        request.ArticleSubmission.Article.Body = " this body has spaces that need trimmed ";

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Article.Body, Is.EqualTo("this body has spaces that need trimmed"));
    }

    [Test]
    public async Task Description_is_trimmed()
    {
        // arrange
        var request = CreateArticleRequestHelper.Create();
        request.ArticleSubmission.Article.Description = " this desc has spaces that need trimmed ";

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Article.Description, Is.EqualTo("this desc has spaces that need trimmed"));
    }

    [Test]
    public async Task Slug_service_is_used_to_generate_from_title()
    {
        // arrange
        var request = CreateArticleRequestHelper.Create();
        request.ArticleSubmission.Article.Title = "slugify this title";
        _slugServiceMock.Setup(m => m.GenerateSlug(request.ArticleSubmission.Article.Title))
            .Returns("slugified-title-a8d9a8ef");

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Article.Slug, Is.EqualTo("slugified-title-a8d9a8ef"));
    }

    [Test]
    public async Task Tag_list_is_passed_through()
    {
        // arrange
        var request = CreateArticleRequestHelper.Create();
        request.ArticleSubmission.Article.TagList = _allTags.Take(1).ToList();

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Article.TagList.Contains(request.ArticleSubmission.Article.TagList.Single()), Is.True);
    }

    [Test]
    public async Task CreatedAt_date_is_set()
    {
        // arrange
        var request = CreateArticleRequestHelper.Create();

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert - as long as the CreatedAt is within 30 seconds, I'd say that's close enough for a test
        Assert.That(result, Is.Not.Null);
        Assert.That((new DateTimeOffset(DateTime.Now) - result.Article.CreatedAt).TotalSeconds, Is.LessThanOrEqualTo(30));
    }


    [Test]
    public async Task Favorited_is_initially_false_FavoritesCount_is_zero()
    {
        // arrange
        var request = CreateArticleRequestHelper.Create();
        request.ArticleSubmission.Article.TagList = _allTags.Take(1).ToList();

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Article.Favorited, Is.False);
        Assert.That(result.Article.FavoritesCount, Is.EqualTo(0));
    }
}