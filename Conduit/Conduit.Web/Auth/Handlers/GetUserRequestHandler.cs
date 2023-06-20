using MediatR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Conduit.Web.Auth.Services;
using Conduit.Web.Auth.ViewModels;
using Conduit.Web.Models;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;

namespace Conduit.Web.Auth.Handlers;

public class GetUserRequestHandler : IRequestHandler<GetUserRequest, GetUserResult>
{
    private readonly IConduitUsersCollectionProvider _usersCollectionProvider;
    private readonly IAuthService _authService;

    public GetUserRequestHandler(IConduitUsersCollectionProvider usersCollectionProvider, IAuthService authService)
    {
        _usersCollectionProvider = usersCollectionProvider;
        _authService = authService;
    }

    public async Task<GetUserResult> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        string email;
        IGetResult userDoc;
        try
        {
            // JWT checking throws ArgumentException if token isn't valid
            var handler = new JwtSecurityTokenHandler();
            var claims = handler.ReadJwtToken(request.BearerToken).Claims;
            email = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value;

            // Couchbase throws DocumentExistsException if document doesn't exist
            var collection = await _usersCollectionProvider.GetCollectionAsync();

            userDoc = await collection.GetAsync(email);
        }
        catch (Exception ex) when (ex is ArgumentException or DocumentExistsException)
        {
            return new GetUserResult { IsInvalidToken = true };
        }

        var user = userDoc.ContentAs<User>();

        return new GetUserResult
        {
            UserView = new UserViewModel
            {
                Email = email,
                Token = _authService.GenerateJwtToken(email),
                Username = user.Username,
                Bio = user.Bio,
                Image = user.Image
            }
        };
    }
}