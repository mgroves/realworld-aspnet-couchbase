using Conduit.Web.Models;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using MediatR;

namespace Conduit.Web.Users.Handlers;

public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserRequest, GetCurrentUserResult>
{
    private readonly IAuthService _authService;
    private readonly IUserDataService _userDataService;

    public GetCurrentUserHandler(IAuthService authService, IUserDataService userDataService)
    {
        _authService = authService;
        _userDataService = userDataService;
    }

    public async Task<GetCurrentUserResult> Handle(GetCurrentUserRequest request, CancellationToken cancellationToken)
    {
        var usernameClaim = _authService.GetUsernameClaim(request.BearerToken);
        if (usernameClaim.IsNotFound)
            return new GetCurrentUserResult { IsInvalidToken = true };

        var username = usernameClaim.Value;

        var userResult = await _userDataService.GetUserByUsername(username);
        if (userResult.Status != DataResultStatus.Ok)
            return new GetCurrentUserResult { UserNotFound = true };

        var user = userResult.DataResult;

        return new GetCurrentUserResult
        {
            UserView = new UserViewModel
            {
                Email = user.Email,
                Token = _authService.GenerateJwtToken(user.Email, username),
                Username = username,
                Bio = user.Bio,
                Image = user.Image
            }
        };
    }
}