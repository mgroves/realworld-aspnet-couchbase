namespace Conduit.Web.ViewModels;

public class LoginViewModel
{
    public LoginUserViewModel User { get; set; }
}

public class LoginUserViewModel
{
    public string Email { get; set; }
    public string Password { get; set; }
}
