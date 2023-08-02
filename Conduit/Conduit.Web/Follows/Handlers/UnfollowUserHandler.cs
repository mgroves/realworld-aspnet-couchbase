using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Conduit.Web.Follows.Services;
using Conduit.Web.Models;
using MediatR;
using FluentValidation;

namespace Conduit.Web.Follows.Handlers;

public class UnfollowUserHandler : IRequestHandler<UnfollowUserRequest, UnfollowUserResult>
{
    private readonly IUserDataService _userDataService;
    private readonly IFollowDataService _followDataService;
    private readonly IValidator<UnfollowUserRequest> _validator;

    public UnfollowUserHandler(IUserDataService userDataService, IFollowDataService followDataService, IValidator<UnfollowUserRequest> validator)
    {
        _userDataService = userDataService;
        _followDataService = followDataService;
        _validator = validator;
    }

    public async Task<UnfollowUserResult> Handle(UnfollowUserRequest request, CancellationToken cancellationToken)
    {
        // validation
        var result = await _validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            return new UnfollowUserResult
            {
                ValidationErrors = result.Errors
            };
        }

        // make sure the user to follow actually exists
        var userToFollowResult = await _userDataService.GetUserByUsername(request.UserToUnfollow);
        if (userToFollowResult.Status == DataResultStatus.NotFound)
            return new UnfollowUserResult { UserNotFound = true };

        await _followDataService.UnfollowUser(request.UserToUnfollow, request.FollowingUsername);

        var profile = new ProfileViewModel
        {
            Username = request.UserToUnfollow,
            Bio = userToFollowResult.DataResult.Bio,
            Image = userToFollowResult.DataResult.Image,
            Following = false
        };

        return new UnfollowUserResult { Profile = profile };
    }
}

