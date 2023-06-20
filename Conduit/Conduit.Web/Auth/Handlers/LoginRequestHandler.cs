using Conduit.Web.Auth.Services;
using Conduit.Web.Auth.ViewModels;
using Conduit.Web.Models;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Auth.Handlers;

public class LoginRequestHandler : IRequestHandler<LoginRequest, LoginResult>
{
    private readonly IConduitUsersCollectionProvider _usersCollectionProvider;
    private readonly IAuthService _authService;
    private readonly IValidator<LoginRequest> _validator;

    public LoginRequestHandler(IConduitUsersCollectionProvider usersCollectionProvider, IAuthService authService, IValidator<LoginRequest> validator)
    {
        _usersCollectionProvider = usersCollectionProvider;
        _authService = authService;
        _validator = validator;
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
        
        // get a couchbase collection
        var collection = await _usersCollectionProvider.GetCollectionAsync();

        // make sure user exists
        var userDocumentKey = request.Model.User.Email;
        var userExists = await collection.ExistsAsync(userDocumentKey);
        if (!userExists.Exists)
            return new LoginResult { IsUnauthorized = true };

        // make sure password matches
        var userDoc = await collection.GetAsync(userDocumentKey);
        var userObj = userDoc.ContentAs<User>();
        if(!_authService.DoesPasswordMatch(request.Model.User.Password, userObj.Password, userObj.PasswordSalt))
            return new LoginResult { IsUnauthorized = true };

        // return a user view object WITH a JWT token
        var userView = new UserViewModel
        {
            Email = userDocumentKey,   // email is the document key
            Token = _authService.GenerateJwtToken(userDocumentKey),
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