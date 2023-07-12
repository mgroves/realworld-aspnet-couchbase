using Conduit.Web.Models;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Users.Handlers;

public class LoginRequestHandler : IRequestHandler<LoginRequest, LoginResult>
{
    private readonly IAuthService _authService;
    private readonly IValidator<LoginRequest> _validator;
    private readonly IUserDataService _userDataService;

    public LoginRequestHandler(IAuthService authService, IValidator<LoginRequest> validator, IUserDataService userDataService)
    {
        _authService = authService;
        _validator = validator;
        _userDataService = userDataService;
    }

    public async Task<LoginResult> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        // validation
        var result = await _validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            return new LoginResult
            {
                ValidationErrors = result.Errors
            };
        }
        
        var userResult = await _userDataService.GetUserByEmail(request.Model.User.Email);

        // make sure user exists
        if (userResult.Status == DataResultStatus.NotFound)
            return new LoginResult { IsUnauthorized = true };

        var userObj = userResult.DataResult;
        
        // make sure password is correct
        if(!_authService.DoesPasswordMatch(request.Model.User.Password, userObj.Password, userObj.PasswordSalt))
            return new LoginResult { IsUnauthorized = true };

        // return a user view object WITH a JWT token
        var userView = new UserViewModel
        {
            Email = request.Model.User.Email,   // email is the document key
            Token = _authService.GenerateJwtToken(request.Model.User.Email, userObj.Username),
            Username = userObj.Username,
            Bio = userObj.Bio,
            Image = userObj.Image
        };

        return new LoginResult
        {
            IsUnauthorized = false,
            UserView = userView
        };
    }
}