using Conduit.Web.Users.ViewModels;
using FluentValidation.Results;

namespace Conduit.Web.Users.Handlers;

public class RegistrationResult
{
    public bool UserAlreadyExists { get; set; }
    public UserViewModel UserView { get; set; }
    public List<ValidationFailure> ValidationErrors { get; set; }
}