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
        var emailClaim = _authService.GetEmailClaim(request.BearerToken);
        if (emailClaim.IsNotFound)
            return new GetCurrentUserResult { IsInvalidToken = true };

        var email = emailClaim.Value;

        var userResult = await _userDataService.GetUserByEmail(email);
        if (userResult.Status != DataResultStatus.Ok)
            return new GetCurrentUserResult { UserNotFound = true };

        var user = userResult.DataResult;

        return new GetCurrentUserResult
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