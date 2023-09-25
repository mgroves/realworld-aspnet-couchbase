using Conduit.Web.DataAccess;
using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Extensions;
using Couchbase.KeyValue;
using Couchbase;
using Couchbase.Query;
using Microsoft.Extensions.Options;

namespace Conduit.Web.Articles.Services;

public interface ICommentsDataService
{
    Task<DataServiceResult<Comment>> Add(Comment newComment, string slug);
    Task<DataServiceResult<List<CommentListDataView>>> Get(string slug, string? currentUsername = null);
}

public class CommentsDataService : ICommentsDataService
{
    private readonly IConduitCommentsCollectionProvider _commentsCollectionProvider;
    private readonly IOptions<CouchbaseOptions> _couchbaseOptions;

    public static string GetCommentsKey(string articleKey) => $"{articleKey}::comments";

    public CommentsDataService(IConduitCommentsCollectionProvider commentsCollectionProvider, IOptions<CouchbaseOptions> couchbaseOptions)
    {
        _commentsCollectionProvider = commentsCollectionProvider;
        _couchbaseOptions = couchbaseOptions;
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
        var collection = await _commentsCollectionProvider.GetCollectionAsync();
        var scope = collection.Scope;
        var bucket = scope.Bucket;

        var loggedInJoin = "";
        if (currentUsername != null)
        {
            loggedInJoin = $" LEFT JOIN `{bucket.Name}`.`{scope.Name}`.`Follows` follow ON ($currentUsername || \"::follows\") = META(follow).id ";
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

        FROM {bucket.Name}.`{scope.Name}`.`Comments` c2
        UNNEST c2 AS c
        JOIN `{bucket.Name}`.`{scope.Name}`.`Users` author ON c.authorUsername = META(author).id

        {loggedInJoin}

        WHERE META(c2).id = $commentsKey;";

        var cluster = collection.Scope.Bucket.Cluster;
        // try
        // {
        //
            var results = await cluster.QueryAsync<CommentListDataView>(sql, options =>
            {
                options.Parameter("commentsKey", GetCommentsKey(slug.GetArticleKey()));
                options.Parameter("currentUsername", currentUsername);
                options.ScanConsistency(_couchbaseOptions.Value.ScanConsistency);
            });
            return new DataServiceResult<List<CommentListDataView>>(await results.Rows.ToListAsync(), DataResultStatus.Ok);
        // }
        // catch (Exception ex)
        // {
        //     return new DataServiceResult<List<CommentListDataView>>(null, DataResultStatus.Error);
        // }
    }

    private async Task<ulong> GetNextCommentId(string slug)
    {
        var collection = await _commentsCollectionProvider.GetCollectionAsync();

        var result = await collection.Binary.IncrementAsync($"{slug.GetArticleKey()}::counter");
        return result.Content;
    }
}