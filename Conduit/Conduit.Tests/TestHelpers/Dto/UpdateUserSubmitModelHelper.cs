using Conduit.Web.Extensions;
using Conduit.Web.Users.ViewModels;

namespace Conduit.Tests.TestHelpers.Dto;

public static class UpdateUserSubmitModelHelper
{
    public static UpdateUserSubmitModel Create(
        string? username = null,
        string? bio = null,
        string? email = null,
        string? image = null,
        string? password = null)
    {
        var random = new Random();

        username ??= "username-" + random.String(10);
        bio ??= "lorem ipsum bio " + random.String(256);
        email ??= "email-" + random.String(10) + "@example.net";
        image ??= "https://example.net/" + random.String(10) + ".jpg";
        password ??= "ValidPassword1!" + random.String(10);

        var model = new UpdateUserSubmitModel
        {
            User = new UpdateUserViewModelUser
            {
                Username = username,
                Bio = bio,
                Email = email,
                Image = image,
                Password = password
            }
        };

        return model;
    }
}