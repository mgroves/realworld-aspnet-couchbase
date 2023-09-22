using Conduit.Web.Articles.Handlers;
using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Extensions;
using Couchbase.KeyValue;

namespace Conduit.Web.Articles.Services;

public interface ICommentsDataService
{
    Task<DataServiceResult<Comment>> Add(Comment newComment, string slug);
}

public class CommentsDataService : ICommentsDataService
{
    private readonly IConduitCommentsCollectionProvider _commentsCollectionProvider;

    public string GetCommentsKey(string articleKey) => $"{articleKey}::comments";

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

    private async Task<ulong> GetNextCommentId(string slug)
    {
        var collection = await _commentsCollectionProvider.GetCollectionAsync();

        var result = await collection.Binary.IncrementAsync($"{slug.GetArticleKey()}::counter");
        return result.Content;
    }
}