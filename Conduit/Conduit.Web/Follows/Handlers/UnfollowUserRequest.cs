using MediatR;

namespace Conduit.Web.Follows.Handlers;

public class UnfollowUserRequest : IRequest<UnfollowUserResult>
{
    public UnfollowUserRequest(string userToUnfollow, string followingUsername)
    {
        UserToUnfollow = userToUnfollow;
        FollowingUsername = followingUsername;
    }

    public string UserToUnfollow { get; }
    public string FollowingUsername { get; }
}