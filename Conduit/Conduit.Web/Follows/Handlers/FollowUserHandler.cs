using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Conduit.Web.Follows.Services;
using Conduit.Web.Models;
using MediatR;
using FluentValidation;

namespace Conduit.Web.Follows.Handlers;

public class FollowUserHandler : IRequestHandler<FollowUserRequest, FollowUserResult>
{
    private readonly IUserDataService _userDataService;
    private readonly IFollowDataService _followDataService;
    private readonly IValidator<FollowUserRequest> _validator;

    public FollowUserHandler(IUserDataService userDataService, IFollowDataService followDataService, IValidator<FollowUserRequest> validator)
    {
        _userDataService = userDataService;
        _followDataService = followDataService;
        _validator = validator;
    }

    public async Task<FollowUserResult> Handle(FollowUserRequest request, CancellationToken cancellationToken)
    {
        // validation
        var result = await _validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            return new FollowUserResult
            {
                ValidationErrors = result.Errors
            };
        }

        // make sure the user to follow actually exists
        var userToFollowResult = await _userDataService.GetUserByUsername(request.UserToFollow);
        if (userToFollowResult.Status == DataResultStatus.NotFound)
            return new FollowUserResult { UserNotFound = true };

        await _followDataService.FollowUser(request.UserToFollow, request.FollowingUsername);

        var profile = new ProfileViewModel
        {
            Username = request.UserToFollow,
            Bio = userToFollowResult.DataResult.Bio,
            Image = userToFollowResult.DataResult.Image,
            Following = true
        };

        return new FollowUserResult { Profile = profile };
    }
}

