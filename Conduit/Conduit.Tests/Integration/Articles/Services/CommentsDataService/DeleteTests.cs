using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.DataAccess;
using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Conduit.Tests.Integration.Articles.Services.CommentsDataService;

[TestFixture]
public class DeleteTests : CouchbaseIntegrationTest
{
    private IConduitCommentsCollectionProvider _commentsCollectionProvider;
    private IConduitArticlesCollectionProvider _articlesCollectionProvider;
    private Web.Articles.Services.CommentsDataService _commentsDataService;
    private IConduitUsersCollectionProvider _userCollectionProvider;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();

        _commentsCollectionProvider = ServiceProvider.GetRequiredService<IConduitCommentsCollectionProvider>();
        _articlesCollectionProvider = ServiceProvider.GetRequiredService<IConduitArticlesCollectionProvider>();
        _userCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();

        var couchbaseOptions = new OptionsWrapper<CouchbaseOptions>(new CouchbaseOptions());
        _commentsDataService = new Web.Articles.Services.CommentsDataService(_commentsCollectionProvider, couchbaseOptions);
    }

    [Test]
    public async Task Can_delete_a_comment_by_id()
    {
        // arrange an article
        var author = await _userCollectionProvider.CreateUserInDatabase();
        var article = await _articlesCollectionProvider.CreateArticleInDatabase(authorUsername: author.Username);
        var commenter = await _userCollectionProvider.CreateUserInDatabase();
        // arrange some comments on the article
        var comment1 = await _commentsCollectionProvider.CreateCommentInDatabase(article.Slug, commenter.Username);
        var comment2 = await _commentsCollectionProvider.CreateCommentInDatabase(article.Slug, commenter.Username);

        // act
        var result = await _commentsDataService.Delete(comment1.Id, article.Slug, commenter.Username);

        // assert
        Assert.That(result, Is.EqualTo(DataResultStatus.Ok));
        await _commentsCollectionProvider.AssertExists(article.Slug, x =>
        {
            Assert.That(x.Count, Is.EqualTo(1));
            Assert.That(x[0].Id, Is.EqualTo(comment2.Id));
        });
    }

    [Test]
    public async Task Id_must_exist()
    {
        // arrange an article
        var author = await _userCollectionProvider.CreateUserInDatabase();
        var article = await _articlesCollectionProvider.CreateArticleInDatabase(authorUsername: author.Username);
        var comment1 = await _commentsCollectionProvider.CreateCommentInDatabase(article.Slug, author.Username);
        var idDoesntExist = comment1.Id + 1;

        // act
        var result = await _commentsDataService.Delete(idDoesntExist, article.Slug, author.Username);

        // assert
        Assert.That(result, Is.EqualTo(DataResultStatus.NotFound));
    }

    [Test]
    public async Task Cant_delete_comments_that_you_didnt_author()
    {
        // arrange an article
        var author = await _userCollectionProvider.CreateUserInDatabase();
        var article = await _articlesCollectionProvider.CreateArticleInDatabase(authorUsername: author.Username);
        var comment1 = await _commentsCollectionProvider.CreateCommentInDatabase(article.Slug, author.Username);
        var someOtherUser = await _userCollectionProvider.CreateUserInDatabase();

        // act
        var result = await _commentsDataService.Delete(comment1.Id, article.Slug, someOtherUser.Username);

        // assert
        Assert.That(result, Is.EqualTo(DataResultStatus.Unauthorized));
    }
}