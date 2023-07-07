using Conduit.Web.Users.ViewModels;
using FluentValidation.Results;

namespace Conduit.Web.Users.Handlers;

public class GetProfileResult
{
    public List<ValidationFailure> ValidationErrors { get; set; }
    public ProfileViewModel ProfileView { get; set; }
    public bool UserNotFound { get; set; }
}