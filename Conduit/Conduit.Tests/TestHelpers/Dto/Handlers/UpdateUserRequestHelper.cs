using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.ViewModels;

namespace Conduit.Tests.TestHelpers.Dto.Handlers;

public static class UpdateUserRequestHelper
{
    /// <summary>
    /// Create a UpdateUserRequest DTO with well-formed default values.
    /// If you want value(s) to be null, use the make***Null flag(s)
    /// </summary>
    public static UpdateUserRequest Create(string? username = null,
        string? email = null,
        string? bio = null,
        string? image = null,
        string? password = null,
        bool makeImageNull = false,
        bool makeBioNull = false,
        bool makeEmailNull = false,
        bool makePasswordNull = false)
    {
        username ??= "user-" + Path.GetRandomFileName();
        email ??= "email-" + Path.GetRandomFileName() + "@example.net";
        password ??= "ValidPassword10#-" + Path.GetRandomFileName();
        bio ??= "Lorem ipsum bio " + Guid.NewGuid();
        image ??= "http://example.net/" + Path.GetRandomFileName() + ".png";
        if (makeImageNull) image = null;
        if (makeBioNull) bio = null;
        if (makeEmailNull) email = null;
        if (makePasswordNull) password = null;

        var request = new UpdateUserRequest(new UpdateUserSubmitModel
        {
            User = new UpdateUserViewModelUser
            {
                Username = username,
                Email = email,
                Bio = bio,
                Image = image,
                Password = password
            }
        });
        return request;
    }
}