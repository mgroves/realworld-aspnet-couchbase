using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.ViewModels;

namespace Conduit.Tests.TestHelpers.Dto.Handlers;

public static class LoginRequestHelper
{
    /// <summary>
    /// Create a LoginRequest DTO object with default well-formed values
    /// </summary>
    public static LoginRequest Create(string? email = null, string? password = null)
    {
        email ??= "email-" + Path.GetRandomFileName() + "@example.net";
        password ??= "ValidPassword1#A-" + Path.GetRandomFileName();

        var userViewModel = new LoginUserViewModel
        {
            Email = email,
            Password = password
        };
        var request = new LoginRequest(new LoginSubmitModel
        {
            User = userViewModel
        });
        return request;
    }
}