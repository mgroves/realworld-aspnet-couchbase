using Conduit.Web.Models;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Couchbase.Core.Exceptions.KeyValue;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Users.Handlers;

public class RegistrationRequestHandler : IRequestHandler<RegistrationRequest, RegistrationResult>
{
    private readonly IConduitUsersCollectionProvider _usersCollectionProvider;
    private readonly IAuthService _authService;
    private readonly IValidator<RegistrationRequest> _validator;

    public RegistrationRequestHandler(IConduitUsersCollectionProvider usersCollectionProvider, IAuthService authService, IValidator<RegistrationRequest> validator)
    {
        _usersCollectionProvider = usersCollectionProvider;
        _authService = authService;
        _validator = validator;
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
        
        // insert this registration into database
        var collection = await _usersCollectionProvider.GetCollectionAsync();

        var passwordSalt = _authService.GenerateSalt(); // User.GenerateSalt();
        var userToInsert = new User
        {
            Username = request.Model.User.Username,
            Password = _authService.HashPassword(request.Model.User.Password, passwordSalt), // Models.User.HashPassword(request.Model.User.Password, passwordSalt),
            PasswordSalt = passwordSalt
        };

        try
        {
            // couchbase keys must be unique
            // registration shouldn't work if the email address is already in use
            await collection.InsertAsync(request.Model.User.Email, userToInsert);
        }
        catch (DocumentExistsException ex)
        {
            return new RegistrationResult { UserAlreadyExists = true };
        }

        var userView = new UserViewModel
        {
            Email = request.Model.User.Email,
            Username = request.Model.User.Username,
            Image = null,
            Bio = null,
            Token = _authService.GenerateJwtToken(request.Model.User.Email)
        };

        return new RegistrationResult
        {
            UserAlreadyExists = false,
            UserView = userView
        };
    }
}