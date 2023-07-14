using MediatR;

namespace Conduit.Web.Follows.Handlers;

public class FollowUserRequest : IRequest<FollowUserResult>
{
    public FollowUserRequest(string userToFollow, string followingUsername)
    {
        UserToFollow = userToFollow;
        FollowingUsername = followingUsername;
    }

    public string UserToFollow { get; }
    public string FollowingUsername { get; }
}