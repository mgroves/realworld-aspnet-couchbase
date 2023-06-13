using FluentValidation;

namespace Conduit.Web.Auth.Handlers;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        // validation is more forgiving here than in registration
        // since login will be checking the database anyway
        // although I suppose adding similar rules might cut down on database usage?

        RuleFor(x => x.Model.User.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email address must not be empty.");

        RuleFor(x => x.Model.User.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password must not be empty.");
    }
}