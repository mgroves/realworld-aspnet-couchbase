using Conduit.Web.Models;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Couchbase.KeyValue;
using FluentValidation;
using MediatR;

namespace Conduit.Web.Users.Handlers;

public class UpdateUserHandler : IRequestHandler<UpdateUserRequest, UpdateUserResult>
{
    private readonly IValidator<UpdateUserRequest> _validator;
    private readonly IConduitUsersCollectionProvider _usersCollectionProvider;
    private readonly IAuthService _authService;

    public UpdateUserHandler(IValidator<UpdateUserRequest> validator, IConduitUsersCollectionProvider usersCollectionProvider, IAuthService authService)
    {
        _validator = validator;
        _usersCollectionProvider = usersCollectionProvider;
        _authService = authService;
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

        // update ONLY the fields that are not empty
        // leave the other fields alone
        var collection = await _usersCollectionProvider.GetCollectionAsync();
        await collection.MutateInAsync(request.Model.User.Email, specs =>
        {
            // TODO: create UpsertIfNotEmpty extension method to make this less verbose?
            // TODO: and possibly avoid hardcoding field names?
            if (!string.IsNullOrEmpty(request.Model.User.Username))
                specs.Upsert("username", request.Model.User.Username);
            if (!string.IsNullOrEmpty(request.Model.User.Bio))
                specs.Upsert("bio", request.Model.User.Bio);
            if (!string.IsNullOrEmpty(request.Model.User.Image))
                specs.Upsert("image", request.Model.User.Image);
            if (!string.IsNullOrEmpty(request.Model.User.Password))
            {
                var salt = _authService.GenerateSalt();
                specs.Upsert("password", _authService.HashPassword(request.Model.User.Password, salt));
                specs.Upsert("passwordSalt", salt);
            }
        });

        // read the whole user back out
        var userDoc = await collection.GetAsync(request.Model.User.Email);
        var userDocObj = userDoc.ContentAs<User>();

        return new UpdateUserResult
        {
            UserView = new UserViewModel
            {
                Bio = userDocObj.Bio,
                Email = request.Model.User.Email,
                Image = userDocObj.Image,
                Token = _authService.GenerateJwtToken(request.Model.User.Email),
                Username = userDocObj.Username
            }
        };
    }
}