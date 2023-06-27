using Conduit.Web.Users.ViewModels;

namespace Conduit.Web.Users.Handlers;

public class GetCurrentUserResult
{
    public UserViewModel UserView { get; set; }

    public bool IsInvalidToken { get; set; }
}