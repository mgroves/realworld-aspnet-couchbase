using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Extensions;
using Couchbase.KeyValue;

namespace Conduit.Tests.TestHelpers.Data;

public static class CommentHelper
{
    public static async Task AssertExists(this IConduitCommentsCollectionProvider @this, string slug, Action<IList<Comment>>? assertions = null)
    {
        var collection = await @this.GetCollectionAsync();

        var key = CommentsDataService.GetCommentsKey(slug.GetArticleKey());
        var doesExist = await collection.ExistsAsync(key);
        Assert.That(doesExist.Exists, Is.True);

        var listOfComments = collection.List<Comment>(key);
        if (assertions != null)
            assertions(listOfComments);
    }

    public static async Task<Comment> CreateCommentInDatabase(this IConduitCommentsCollectionProvider @this,
        string slug,
        string username,
        string? body = null,
        ulong? id = null)
    {
        var random = new Random();

        body ??= random.String(64);
        id ??= (ulong)random.Next(100000, 200000);

        var collection = await @this.GetCollectionAsync();

        var key = CommentsDataService.GetCommentsKey(slug.GetArticleKey());

        var comments = collection.List<Comment>(key);
        var comment = new Comment
        {
            AuthorUsername = username,
            Body = body,
            CreatedAt = DateTimeOffset.Now,
            Id = id.Value
        };
        await comments.AddAsync(comment);
        return comment;
    }
}