using Conduit.Web.Users.ViewModels;

namespace Conduit.Tests.TestHelpers.Dto;

public static class LoginSubmitModelHelper
{
    /// <summary>
    /// Create a LoginSubmitModel DTO
    /// with well-formed values if arguments are not specified
    /// To set null values, use the "make*Null" parameters
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="makeEmailNull"></param>
    /// <param name="makePasswordNull"></param>
    /// <returns></returns>
    public static LoginSubmitModel Create(
        string? email = null,
        string? password = null,
        bool makeEmailNull = false,
        bool makePasswordNull = false)
    {
        email ??= "email-" + Path.GetRandomFileName() + "@example.net";
        password ??= "Valid1!Password-" + Path.GetRandomFileName();

        if (makeEmailNull) email = null;
        if (makePasswordNull) password = null;

        return new LoginSubmitModel
        {
            User = new LoginUserViewModel
            {
                Email = email,
                Password = password
            }
        };
    }
}