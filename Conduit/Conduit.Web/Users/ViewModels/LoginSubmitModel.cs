namespace Conduit.Web.Users.ViewModels;

public record LoginSubmitModel
{
    public LoginUserViewModel User { get; set; }
}

public record LoginUserViewModel
{
    public string Email { get; set; }
    public string Password { get; set; }
}
