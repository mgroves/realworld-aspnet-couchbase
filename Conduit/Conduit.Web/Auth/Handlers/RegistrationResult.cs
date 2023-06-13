using Conduit.Web.Auth.ViewModels;

namespace Conduit.Web.Auth.Handlers;

public class RegistrationResult
{
    public bool UserAlreadyExists { get; set; }
    public UserViewModel UserView { get; set; }
}