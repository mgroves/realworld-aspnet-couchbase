using Conduit.Web.Extensions;
using Conduit.Web.Users.ViewModels;

namespace Conduit.Tests.TestHelpers.Dto.ViewModels;

public static class RegistrationSubmitModelHelper
{
    public static RegistrationSubmitModel Create(
        string? email = null,
        string? password = null,
        string? username = null,
        bool makeUsernameNull = false,
        bool makeEmailNull = false,
        bool makePasswordNull = false)
    {
        var random = new Random();

        email ??= "email-" + random.String(10) + "@example.net";
        password ??= "ValidPassword1!-" + random.String(10);
        username ??= "username-" + random.String(10);
        if (makeUsernameNull) username = null;
        if (makeEmailNull) email = null;
        if (makePasswordNull) password = null;

        var model = new RegistrationSubmitModel();
        model.User = new RegistrationUserSubmitModel();
        model.User.Email = email;
        model.User.Password = password;
        model.User.Username = username;
        return model;
    }
}