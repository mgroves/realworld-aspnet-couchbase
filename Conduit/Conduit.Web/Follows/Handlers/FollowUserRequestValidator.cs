using Conduit.Web.Models;
using Conduit.Web.Users.Services;
using FluentValidation;

namespace Conduit.Web.Follows.Handlers;

public class FollowUserRequestValidator : AbstractValidator<FollowUserRequest>
{
    public FollowUserRequestValidator()
    {
        RuleFor(x => x.UserToFollow)
            .NotEmpty().WithMessage("Username is required.");
    }
}