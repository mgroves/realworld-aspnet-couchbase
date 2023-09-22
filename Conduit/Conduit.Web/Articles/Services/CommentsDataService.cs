using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Extensions;
using Couchbase.KeyValue;
using Couchbase;
using Couchbase.Query;

namespace Conduit.Web.Articles.Services;

public interface ICommentsDataService
{
    Task<DataServiceResult<Comment>> Add(Comment newComment, string slug);
    Task<DataServiceResult<List<CommentListDataView>>> Get(string slug, string? currentUsername = null);
}

public class CommentsDataService : ICommentsDataService
{
    private readonly IConduitCommentsCollectionProvider _commentsCollectionProvider;

    // a virtual method so it can be overridden by a testing class if necessary
    protected virtual QueryScanConsistency ScanConsistency => QueryScanConsistency.NotBounded;

    public static string GetCommentsKey(string articleKey) => $"{articleKey}::comments";

    public CommentsDataService(IConduitCommentsCollectionProvider commentsCollectionProvider)
    {
        _commentsCollectionProvider = commentsCollectionProvider;
    }

    public async Task<DataServiceResult<Comment>> Add(Comment newComment, string slug)
    {
        var collection = await _commentsCollectionProvider.GetCollectionAsync();

        var articleKey = slug.GetArticleKey();
        var key = GetCommentsKey(articleKey);
        var commentSet = collection.List<Comment>(key);

        // add a unique long ID
        newComment.Id = await GetNextCommentId(slug);

        try
        {
            await commentSet.AddAsync(newComment);
        }
        catch (Exception ex)
        {
            return new DataServiceResult<Comment>(null, DataResultStatus.Error);
        }

        return new DataServiceResult<Comment>(newComment, DataResultStatus.Ok);
    }

    public async Task<DataServiceResult<List<CommentListDataView>>> Get(string slug, string? currentUsername)
    {
        var loggedInJoin = "";
        if (currentUsername != null)
        {
            loggedInJoin = " LEFT JOIN Conduit._default.Follows follow ON ($currentUsername || \"::follows\") = META(follow).id ";
        }

        var sql = $@"SELECT VALUE
            {{
                c.body,
                c.createdAt,
                c.id,
                ""author"" : {{
                    author.bio,
                    author.image,
                    ""username"": c.authorUsername,
                    ""following"": ARRAY_CONTAINS(COALESCE(follow,[]), c.authorUsername)
                }}
            }}

        FROM Conduit._default.Comments c2
        UNNEST c2 AS c
        JOIN Conduit._default.Users author ON c.authorUsername = META(author).id

        {loggedInJoin}

        /* parameterized with comments key */
        WHERE META(c2).id = $commentsKey;";

        var collection = await _commentsCollectionProvider.GetCollectionAsync();
        var cluster = collection.Scope.Bucket.Cluster;
        try
        {

            var results = await cluster.QueryAsync<CommentListDataView>(sql, options =>
            {
                options.Parameter("commentsKey", GetCommentsKey(slug.GetArticleKey()));
                options.Parameter("currentUsername", currentUsername);
                options.ScanConsistency(ScanConsistency);
            });
            return new DataServiceResult<List<CommentListDataView>>(await results.Rows.ToListAsync(), DataResultStatus.Ok);
        }
        catch (Exception ex)
        {
            return new DataServiceResult<List<CommentListDataView>>(null, DataResultStatus.Error);
        }
    }

    private async Task<ulong> GetNextCommentId(string slug)
    {
        var collection = await _commentsCollectionProvider.GetCollectionAsync();

        var result = await collection.Binary.IncrementAsync($"{slug.GetArticleKey()}::counter");
        return result.Content;
    }
}