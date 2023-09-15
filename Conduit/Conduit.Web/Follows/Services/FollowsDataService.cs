using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Users.Services;
using Couchbase.KeyValue;

namespace Conduit.Web.Follows.Services;

public interface IFollowDataService
{
    Task FollowUser(string userToFollow, string followerUsername);
    Task UnfollowUser(string userToUnfollow, string followerUsername);
    Task<bool> IsCurrentUserFollowing(string currentUser, string username); 
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

    public async Task UnfollowUser(string userToUnfollow, string followerUsername)
    {
        var collection = await _followsCollectionProvider.GetCollectionAsync();

        var followKey = $"{followerUsername}::follows";

        var set = collection.Set<string>(followKey);

        await set.RemoveAsync(userToUnfollow);
    }

    public async Task<bool> IsCurrentUserFollowing(string currentUsername, string username)
    {
        // can't follow yourself
        if (currentUsername == username)
            return false;

        var collection = await _followsCollectionProvider.GetCollectionAsync();

        var followKey = $"{currentUsername}::follows";

        var set = collection.Set<string>(followKey);

        return await set.ContainsAsync(username);
    }
}