using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.Follows.Services;
using Conduit.Web.Users.Services;
using Moq;

namespace Conduit.Tests.Unit.Articles.Handlers;

[TestFixture]
public class GetArticleHandlerTests
{
    private Mock<IArticlesDataService> _articleDataServiceMock;
    private GetArticleHandler _handler;
    private Mock<IUserDataService> _usersDataServiceMock;
    private Mock<IFollowDataService> _followsDataServiceMock;

    [SetUp]
    public async Task Setup()
    {
        _articleDataServiceMock = new Mock<IArticlesDataService>();
        _usersDataServiceMock = new Mock<IUserDataService>();
        _followsDataServiceMock = new Mock<IFollowDataService>();

        // mock defaults
        _followsDataServiceMock.Setup(m => m.IsCurrentUserFollowing(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _articleDataServiceMock.Setup(m => m.IsFavorited(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _handler = new GetArticleHandler(
            _articleDataServiceMock.Object,
            _usersDataServiceMock.Object,
            _followsDataServiceMock.Object);
    }

    [Test]
    public async Task When_article_exists_return_a_view_of_it_and_author()
    {
        // arrange
        var author = UserHelper.CreateUser();
        var article = ArticleHelper.CreateArticle(authorUsername: author.Username);
        var request = new GetArticleRequest(article.Slug, "valid-username");
        _articleDataServiceMock.Setup(m => m.Exists(article.Slug))
            .ReturnsAsync(true);
        _articleDataServiceMock.Setup(m => m.Get(article.Slug))
            .ReturnsAsync(new DataServiceResult<Article>(article, DataResultStatus.Ok));
        _usersDataServiceMock.Setup(m => m.GetProfileByUsername(author.Username))
            .ReturnsAsync(new DataServiceResult<User>(author, DataResultStatus.Ok));
    
        // act
        var result = await _handler.Handle(request, CancellationToken.None);
    
        // assert
        Assert.That(result.ArticleView, Is.Not.Null);
        Assert.That(result.ValidationErrors == null || !result.ValidationErrors.Any(), Is.True);
    }
    
    [Test]
    public async Task Invalid_when_article_doesnt_exist()
    {
        // arrange
        var request = new GetArticleRequest("doesnt-matter", "valid-username");
        _articleDataServiceMock.Setup(m => m.Exists(It.IsAny<string>()))
            .ReturnsAsync(false);
    
        // act
        var result = await _handler.Handle(request, CancellationToken.None);
    
        // assert
        Assert.That(result.ValidationErrors != null && result.ValidationErrors.Any(), Is.True);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors[0].ErrorMessage, Is.EqualTo("Article not found."));
    }
    
    [Test]
    public async Task When_user_not_authenticated_following_and_favorited_default_to_false()
    {
        // arrange
        var author = UserHelper.CreateUser();
        var article = ArticleHelper.CreateArticle(authorUsername: author.Username);
        var request = new GetArticleRequest(article.Slug, null);
        _articleDataServiceMock.Setup(m => m.Exists(article.Slug))
            .ReturnsAsync(true);
        _articleDataServiceMock.Setup(m => m.Get(article.Slug))
            .ReturnsAsync(new DataServiceResult<Article>(article, DataResultStatus.Ok));
        _usersDataServiceMock.Setup(m => m.GetProfileByUsername(author.Username))
            .ReturnsAsync(new DataServiceResult<User>(author, DataResultStatus.Ok));
    
        // act
        var result = await _handler.Handle(request, CancellationToken.None);
    
        // assert
        Assert.That(result.ArticleView, Is.Not.Null);
        Assert.That(result.ValidationErrors == null || !result.ValidationErrors.Any(), Is.True);
        Assert.That(result.ArticleView.Favorited, Is.False);
        Assert.That(result.ArticleView.Author.Following, Is.False);
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task When_returning_article_to_authenticated_user_following_and_favorited_are_set(bool isFollowing, bool isFavorited)
    {
        // arrange
        var author = UserHelper.CreateUser();
        var article = ArticleHelper.CreateArticle(authorUsername: author.Username);
        var request = new GetArticleRequest(article.Slug, "valid-username");
        _articleDataServiceMock.Setup(m => m.Exists(article.Slug))
            .ReturnsAsync(true);
        _articleDataServiceMock.Setup(m => m.Get(article.Slug))
            .ReturnsAsync(new DataServiceResult<Article>(article, DataResultStatus.Ok));
        _usersDataServiceMock.Setup(m => m.GetProfileByUsername(author.Username))
            .ReturnsAsync(new DataServiceResult<User>(author, DataResultStatus.Ok));
        _followsDataServiceMock.Setup(m => m.IsCurrentUserFollowing(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(isFollowing);
        _articleDataServiceMock.Setup(m => m.IsFavorited(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(isFavorited);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result.ArticleView, Is.Not.Null);
        Assert.That(result.ValidationErrors == null || !result.ValidationErrors.Any(), Is.True);
        Assert.That(result.ArticleView.Author.Following, Is.EqualTo(isFollowing));
        Assert.That(result.ArticleView.Favorited, Is.EqualTo(isFavorited));
    }
}