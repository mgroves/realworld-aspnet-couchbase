using Conduit.Web.Users.ViewModels;

namespace Conduit.Web.Users.Handlers;

public class GetUserResult
{
    public UserViewModel UserView { get; set; }

    public bool IsInvalidToken { get; set; }
}