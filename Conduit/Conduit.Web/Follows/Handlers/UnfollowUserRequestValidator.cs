using FluentValidation;

namespace Conduit.Web.Follows.Handlers;

public class UnfollowUserRequestValidator : AbstractValidator<UnfollowUserRequest>
{
    public UnfollowUserRequestValidator()
    {
        RuleFor(x => x.UserToUnfollow)
            .NotEmpty().WithMessage("Username is required.");
    }
}