using Conduit.Web.Models;
using Conduit.Web.Services;
using Conduit.Web.ViewModels;
using Couchbase.Extensions.DependencyInjection;
using MediatR;

namespace Conduit.Web.Requests.Auth;

public class LoginRequestHandler : IRequestHandler<LoginRequest, LoginResult>
{
    private readonly IBucketProvider _bucketProvider;
    private readonly AuthService _authService;

    public LoginRequestHandler(IBucketProvider bucketProvider, AuthService authService)
    {
        _bucketProvider = bucketProvider;
        _authService = authService;
    }

    public async Task<LoginResult> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        // get a couchbase collection
        var bucket = await _bucketProvider.GetBucketAsync("Conduit");
        var collection = await bucket.CollectionAsync("Users");

        // make sure user exists
        var userExists = await collection.ExistsAsync(request.Model.User.Email);
        if (!userExists.Exists)
            return new LoginResult { IsUnauthorized = false };

        // make sure password matches
        var userDoc = await collection.GetAsync(request.Model.User.Email);
        var userObj = userDoc.ContentAs<User>();
        if(!_authService.DoesPasswordMatch(request.Model.User.Password, userObj.Password, userObj.PasswordSalt))
            return new LoginResult { IsUnauthorized = false };

        // return a user view object WITH a JWT token
        var userView = new UserViewModel
        {
            Email = userObj.Email,
            Token = _authService.GenerateJwtToken(userObj.Username),
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