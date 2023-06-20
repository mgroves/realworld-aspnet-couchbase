using Conduit.Web.Auth.ViewModels;

namespace Conduit.Web.Auth.Handlers;

public class GetUserResult
{
    public UserViewModel UserView { get; set; }

    public bool IsInvalidToken { get; set; }
}