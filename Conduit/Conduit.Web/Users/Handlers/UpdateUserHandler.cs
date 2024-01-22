using Conduit.Web.Adaptive.Services;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Couchbase.Core.Exceptions.KeyValue;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Users.Handlers;

public class UpdateUserHandler : IRequestHandler<UpdateUserRequest, UpdateUserResult>
{
    private readonly IValidator<UpdateUserRequest> _validator;
    private readonly IAuthService _authService;
    private readonly IUserDataService _userDataService;
    private readonly IAdaptiveDataService _adaptiveDataService;

    public UpdateUserHandler(IValidator<UpdateUserRequest> validator, IAuthService authService, IUserDataService userDataService, IAdaptiveDataService adaptiveDataService)
    {
        _validator = validator;
        _authService = authService;
        _userDataService = userDataService;
        _adaptiveDataService = adaptiveDataService;
    }

    public async Task<UpdateUserResult> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        // validation
        var result = await _validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            return new UpdateUserResult
            {
                ValidationErrors = result.Errors
            };
        }

        // update the user fields
        try
        {
            await _userDataService.UpdateUserFields(request.Model.User);
        }
        catch (DocumentNotFoundException ex)
        {
            return new UpdateUserResult { IsNotFound = true };
        }

        // update adaptive tags (if necessary)
        if (!string.IsNullOrEmpty(request.Model.User.Bio))
        {
            await _adaptiveDataService.UpdateUserProfileAdaptiveTags(request.Model.User.Username, request.Model.User.Bio);
        }

        // get the user back out (post-update)
        var userResult = await _userDataService.GetUserByUsername(request.Model.User.Username);
        var user = userResult.DataResult;

        // return view
        return new UpdateUserResult
        {
            UserView = new UserViewModel
            {
                Bio = user.Bio,
                Email = request.Model.User.Email,
                Image = user.Image,
                Token = _authService.GenerateJwtToken(user.Email, request.Model.User.Username),
                Username = user.Username
            }
        };
    }
}