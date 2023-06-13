using Conduit.Web.Auth.ViewModels;
using FluentValidation.Results;

namespace Conduit.Web.Auth.Handlers;

public class RegistrationResult
{
    public bool UserAlreadyExists { get; set; }
    public UserViewModel UserView { get; set; }
    public List<ValidationFailure> ValidationErrors { get; set; }
}