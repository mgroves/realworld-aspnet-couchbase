using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Users.Handlers;

public class RegistrationRequestHandler : IRequestHandler<RegistrationRequest, RegistrationResult>
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegistrationRequest> _validator;
    private readonly IUserDataService _userDataService;

    public RegistrationRequestHandler(IAuthService authService, IValidator<RegistrationRequest> validator, IUserDataService userDataService)
    {
        _authService = authService;
        _validator = validator;
        _userDataService = userDataService;
    }

    public async Task<RegistrationResult> Handle(RegistrationRequest request, CancellationToken cancellationToken)
    {
        // validation
        var result = await _validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            return new RegistrationResult
            {
                ValidationErrors = result.Errors
            };
        }

        // construct new user object
        var passwordSalt = _authService.GenerateSalt(); // User.GenerateSalt();

        var userToInsert = new User
        {
            Email = request.Model.User.Email,
            Username = request.Model.User.Username,
            Password = _authService.HashPassword(request.Model.User.Password, passwordSalt), // Models.User.HashPassword(request.Model.User.Password, passwordSalt),
            PasswordSalt = passwordSalt
        };

        // try to put new user into database
        var userResult = await _userDataService.RegisterNewUser(userToInsert);

        // if user with same key already exists, return error
        if(userResult.Status == DataResultStatus.FailedToInsert)
            return new RegistrationResult { UserAlreadyExists = true };

        // construct view model to return
        var userView = new UserViewModel
        {
            Email = request.Model.User.Email,
            Username = request.Model.User.Username,
            Image = null,
            Bio = null,
            Token = _authService.GenerateJwtToken(request.Model.User.Email, request.Model.User.Username)
        };
        return new RegistrationResult
        {
            UserAlreadyExists = false,
            UserView = userView
        };
    }
}