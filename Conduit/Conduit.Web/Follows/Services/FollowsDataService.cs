using Conduit.Web.Models;
using Couchbase.KeyValue;

namespace Conduit.Web.Follows.Services;

public interface IFollowDataService
{
    Task FollowUser(string userToFollow, string followerUsername);
}

public class FollowsDataService : IFollowDataService
{
    private readonly IConduitFollowsCollectionProvider _followsCollectionProvider;

    public FollowsDataService(IConduitFollowsCollectionProvider followsCollectionProvider)
    {
        _followsCollectionProvider = followsCollectionProvider;
    }

    public async Task FollowUser(string userToFollow, string followerUsername)
    {
        var collection = await _followsCollectionProvider.GetCollectionAsync();

        var followKey = $"{followerUsername}::follows";

        var set = collection.Set<string>(followKey);

        await set.AddAsync(userToFollow);
    }
}