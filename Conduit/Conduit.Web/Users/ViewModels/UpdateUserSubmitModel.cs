using Conduit.Web.Users.Handlers;

namespace Conduit.Web.Users.ViewModels;

public class UpdateUserSubmitModel
{
    public UpdateUserViewModelUser User { get; set; }
}

public class UpdateUserViewModelUser : IHasUserPropertiesForValidation
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Image { get; set; }
    public string Bio { get; set; }
}