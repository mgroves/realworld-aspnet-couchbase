using FluentValidation;

namespace Conduit.Web.Users.Handlers;

public class GetProfileRequestValidator : AbstractValidator<GetProfileRequest>
{
    public GetProfileRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required");
    }
}