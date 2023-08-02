using Conduit.Web.Users.ViewModels;
using FluentValidation.Results;

namespace Conduit.Web.Follows.Handlers;

public class UnfollowUserResult
{
    public bool UserNotFound { get; set; }
    public ProfileViewModel Profile { get; set; }
    public List<ValidationFailure> ValidationErrors { get; set; }
}