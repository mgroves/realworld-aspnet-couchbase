using Conduit.Web.Models;
using Couchbase.KeyValue;

namespace Conduit.Tests.TestHelpers.Data;

public static class FollowHelper
{
    public static async Task AssertExists(this IConduitFollowsCollectionProvider @this, string usernameFollower, Action<List<string>>? assertions)
    {
        var followCollection = await @this.GetCollectionAsync();
        var followDocId = $"{usernameFollower}::follows";
        var followResult = await followCollection.TryGetAsync(followDocId);

        if(!followResult.Exists)
            Assert.Fail($"Document {usernameFollower} not found.");

        if (assertions != null)
        {
            var usernamesBeingFollowed = followCollection.Set<string>(followDocId).ToList();
            assertions(usernamesBeingFollowed);
        }

        Assert.Pass();
    }

    public static async Task CreateFollow(this IConduitFollowsCollectionProvider @this,
        string usernameFollowing,
        string usernameFollower)
    {
        var followCollection = await @this.GetCollectionAsync();
        var followDocId = $"{usernameFollower}::follows";

        var set = followCollection.Set<string>(followDocId);

        await set.AddAsync(usernameFollowing);
    }
}