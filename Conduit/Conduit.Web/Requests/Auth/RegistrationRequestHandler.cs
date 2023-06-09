using Conduit.Web.Models;
using Conduit.Web.Services;
using Conduit.Web.ViewModels;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.Extensions.DependencyInjection;
using MediatR;

namespace Conduit.Web.Requests.Auth;

public class RegistrationRequestHandler : IRequestHandler<RegistrationRequest, RegistrationResult>
{
    private readonly IBucketProvider _bucketProvider;
    private readonly AuthService _authService;

    public RegistrationRequestHandler(IBucketProvider bucketProvider, AuthService authService)
    {
        _bucketProvider = bucketProvider;
        _authService = authService;
    }

    public async Task<RegistrationResult> Handle(RegistrationRequest request, CancellationToken cancellationToken)
    {
        // insert this registration into database
        var bucket = await _bucketProvider.GetBucketAsync("Conduit");
        var collection = await bucket.CollectionAsync("Users");

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
            Token = _authService.GenerateJwtToken(request.Model.User.Username)
        };

        return new RegistrationResult
        {
            UserAlreadyExists = false,
            UserView = userView
        };
    }
}