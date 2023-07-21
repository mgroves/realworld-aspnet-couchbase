using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.ViewModels;

namespace Conduit.Tests.TestHelpers.Dto;

public static class RegistrationRequestHelper
{
    /// <summary>
    /// Create a RegistrationRequest DTO with well-formed
    /// default values
    /// </summary>
    public static RegistrationRequest Create(
        string? username = null,
        string? password = null,
        string? email = null)
    {
        username ??= "user-" + Path.GetRandomFileName();
        password ??= "ValidPassword!1#-" + Path.GetRandomFileName();
        email ??= "email-" + Path.GetRandomFileName() + "@example.net";

        var submittedInfo = new RegistrationUserSubmitModel
        {
            Username = username,
            Password = password,
            Email = email
        };
        var request = new RegistrationRequest(new RegistrationSubmitModel
        {
            User = submittedInfo
        });
        return request;
    }
}