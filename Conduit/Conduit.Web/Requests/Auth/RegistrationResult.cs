using Conduit.Web.ViewModels;

namespace Conduit.Web.Requests.Auth;

public class RegistrationResult
{
    public bool UserAlreadyExists { get; set; }
    public UserViewModel UserView { get; set; }
}