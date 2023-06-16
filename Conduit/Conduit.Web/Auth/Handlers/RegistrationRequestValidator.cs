using Couchbase.Extensions.DependencyInjection;
using Couchbase.Query;
using EmailValidation;
using FluentValidation;

namespace Conduit.Web.Auth.Handlers;

public class RegistrationRequestValidator : AbstractValidator<RegistrationRequest>
{
    private readonly IClusterProvider _clusterProvider;

    public RegistrationRequestValidator(IClusterProvider clusterProvider)
    {
        _clusterProvider = clusterProvider;

        RuleFor(x => x.Model.User.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email address must not be empty.")
            .Must(BeAValidEmailAddress).WithMessage("Email address must be valid.");
        
        RuleFor(x => x.Model.User.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password must not be empty.")
            .MinimumLength(10).WithMessage("Password must be at least 10 characters long.")
            .Must(ContainDigit).WithMessage("Password must contain at least one digit.")
            .Must(ContainUppercaseLetter).WithMessage("Password must contain at least one uppercase letter.")
            .Must(ContainLowercaseLetter).WithMessage("Password must contain at least one lowercase letter.")
            .Must(ContainSymbol).WithMessage("Password must contain at least one symbol.");

        RuleFor(x => x.Model.User.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Username must not be empty.")
            .MaximumLength(100).WithMessage("Username must be at most 100 characters long.")
            .MustAsync(async (username, cancellation) => await NotAlreadyExist(username, cancellation))
                .WithMessage("That username is already in use.");

        // TODO: consider using zxcvbn library to provide a better measure of password strength
        // as the above password policy may be weak
    }
    
    private async Task<bool> NotAlreadyExist(string username, CancellationToken cancellationToken)
    {
        var cluster = await _clusterProvider.GetClusterAsync();

        // TODO: revist this hard coding of bucket/scope/collection names
        var checkForExistingUsernameSql = @"
        SELECT RAW COUNT(*)
        FROM `Conduit`.`_default`.`Users` u
        WHERE u.username = $username";

        // Can potentially be switched to use NotBounded scan consistency
        // for reduced latency, if the risk of two people trying to get the
        // same username within a very small window of time is small
        var queryOptions = new QueryOptions()
            .Parameter("username", username)
            .ScanConsistency(QueryScanConsistency.RequestPlus);

        var result = await cluster.QueryAsync<int>(checkForExistingUsernameSql, queryOptions);

        var countResult = await result.ToListAsync(cancellationToken);

        if (!countResult.Any())
            return true;

        var howManyMatches = countResult.First();

        return howManyMatches < 1;
    }

    private bool BeAValidEmailAddress(string password)
    {
        // using EmailValidation library because I find the EmailAddress for FluentValidation to be too naive
        // wanted something a little stronger to catch typos
        // YES tld email addresses exist, but I assert that it's more likely to be a typo than
        // it is for someone to actually have a tld email address
        return EmailValidator.Validate(password, false, true);
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