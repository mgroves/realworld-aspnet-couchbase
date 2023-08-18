using FluentValidation;

namespace Conduit.Web.Follows.Handlers;

public class FollowUserRequestValidator : AbstractValidator<FollowUserRequest>
{
    public FollowUserRequestValidator()
    {
        RuleFor(x => x.UserToFollow)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.UserToFollow)
            .Must(NotBeMyself).WithMessage("You can't follow yourself.");
    }

    private bool NotBeMyself(string username)
    {
        throw new NotImplementedException();
    }
}