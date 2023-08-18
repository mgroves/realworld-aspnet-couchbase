using Conduit.Web.Follows.Services;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using MediatR;
using FluentValidation;
using Conduit.Web.DataAccess.Dto;

namespace Conduit.Web.Users.Handlers;

public class GetProfileHandler : IRequestHandler<GetProfileRequest, GetProfileResult>
{
    private readonly IUserDataService _userDataService;
    private readonly IValidator<GetProfileRequest> _validator;
    private readonly IFollowDataService _followDataService;

    public GetProfileHandler(IUserDataService userDataService, IValidator<GetProfileRequest> validator, IFollowDataService followDataService)
    {
        _userDataService = userDataService;
        _validator = validator;
        _followDataService = followDataService;
    }

    public async Task<GetProfileResult> Handle(GetProfileRequest request, CancellationToken cancellationToken)
    {
        // validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new GetProfileResult
            {
                ValidationErrors = validationResult.Errors
            };
        }

        var profileResult = await _userDataService.GetProfileByUsername(request.Username);
        if (profileResult.Status == DataResultStatus.NotFound)
            return new GetProfileResult { UserNotFound = true };

        var result = profileResult.DataResult;

        // if JWT is specified, use that to determine if the logged-in user is following this profile
        bool isCurrentUserFollowing = false;
        if (!string.IsNullOrEmpty(request.OptionalBearerToken))
        {
            isCurrentUserFollowing = await _followDataService.IsCurrentUserFollowing(request.OptionalBearerToken, request.Username);
        }

        return new GetProfileResult
        {
            ProfileView = new ProfileViewModel
            {
                Username = request.Username,
                Bio = result.Bio,
                Image = result.Image,
                Following = isCurrentUserFollowing
            }
        };
    }
}