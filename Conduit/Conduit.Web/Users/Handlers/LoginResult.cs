using Conduit.Web.Users.ViewModels;
using FluentValidation.Results;

namespace Conduit.Web.Users.Handlers;

public class LoginResult
{
    public bool IsUnauthorized { get; set; }
    public UserViewModel UserView { get; set; }
    public List<ValidationFailure> ValidationErrors { get; set; }
}