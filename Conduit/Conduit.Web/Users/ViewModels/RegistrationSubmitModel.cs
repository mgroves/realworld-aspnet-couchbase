using Conduit.Web.Users.Handlers;

namespace Conduit.Web.Users.ViewModels;

public record RegistrationSubmitModel
{
    public RegistrationUserSubmitModel User { get; set; }
}

public record RegistrationUserSubmitModel : IHasUserPropertiesForValidation
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}