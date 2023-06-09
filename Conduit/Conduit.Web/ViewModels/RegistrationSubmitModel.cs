namespace Conduit.Web.ViewModels;

public record RegistrationSubmitModel
{
    public RegistrationUserSubmitModel User { get; set; }
}

public record RegistrationUserSubmitModel
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}