using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using MediatR;
using FluentValidation;

namespace Conduit.Web.Users.Handlers;

public class GetProfileHandler : IRequestHandler<GetProfileRequest, GetProfileResult>
{
    private readonly IUserDataService _userDataService;
    private readonly IValidator<GetProfileRequest> _validator;

    public GetProfileHandler(IUserDataService userDataService, IValidator<GetProfileRequest> validator)
    {
        _userDataService = userDataService;
        _validator = validator;
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

        // TODO: if JWT is specified, use that to determine if the logged-in user is following this profile

        var profileResult = await _userDataService.GetProfileByUsername(request.Username);
        if (profileResult.Status == DataResultStatus.NotFound)
            return new GetProfileResult { UserNotFound = true };

        var result = profileResult.DataResult;

        return new GetProfileResult
        {
            ProfileView = new ProfileViewModel
            {
                Username = result.Username,
                Bio = result.Bio,
                Image = result.Image,
                Following = false       // TODO: determine following or not (this MIGHT be done in the data layer?)
            }
        };
    }
}