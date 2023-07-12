using EmailValidation;
using FluentValidation;

namespace Conduit.Web.Users.Handlers;

// This interface is used to apply shared rules
// Between different edit/view models
public interface IHasUserPropertiesForValidation
{
    public string Password { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
}

// This validator is for reuse of common validation rules for Password, Username, and Email
// Between UpdateUserRequestValidator and RegistrationRequestValidator
// So that rules do not have to be changed in two places
// Note that this shared validator ALLOWS password and username to be empty
// Because UpdateUserRequest doesn't require them to be non-empty
// Therefore RegistrationRequestValidator needs to check for non-empty, because they are all required there.
public class SharedUserValidator<T> : AbstractValidator<T> where T : IHasUserPropertiesForValidation
{
    public SharedUserValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .Must(BeAValidEmailAddress).When(r => !string.IsNullOrEmpty(r.Email)).WithMessage("Email address must be valid.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .MinimumLength(10).When(r => !string.IsNullOrEmpty(r.Password)).WithMessage("Password must be at least 10 characters long.")
            .Must(ContainDigit).When(r => !string.IsNullOrEmpty(r.Password)).WithMessage("Password must contain at least one digit.")
            .Must(ContainUppercaseLetter).When(r => !string.IsNullOrEmpty(r.Password)).WithMessage("Password must contain at least one uppercase letter.")
            .Must(ContainLowercaseLetter).When(r => !string.IsNullOrEmpty(r.Password)).WithMessage("Password must contain at least one lowercase letter.")
            .Must(ContainSymbol).When(r => !string.IsNullOrEmpty(r.Password)).WithMessage("Password must contain at least one symbol.");

        RuleFor(x => x.Username)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(100).When(r => !string.IsNullOrEmpty(r.Username)).WithMessage("Username must be at most 100 characters long.")
            .NotEmpty().WithMessage("Username is required.");
    }

    private bool BeAValidEmailAddress(string email)
    {
        // using EmailValidation library because I find the EmailAddress for FluentValidation to be too naive
        // wanted something a little stronger to catch typos
        // YES tld email addresses exist, but I assert that it's more likely to be a typo than
        // it is for someone to actually have a tld email address
        return EmailValidator.Validate(email, false, true);
    }

    private bool ContainDigit(string password)
    {
        return password.Any(char.IsDigit);
    }

    private bool ContainUppercaseLetter(string password)
    {
        return password.Any(char.IsUpper);
    }

    private bool ContainLowercaseLetter(string password)
    {
        return password.Any(char.IsLower);
    }

    private bool ContainSymbol(string password)
    {
        return password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}