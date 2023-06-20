using Conduit.Web.Models;
using Conduit.Web.Users.ViewModels;
using Couchbase.Query;
using FluentValidation;

namespace Conduit.Web.Users.Handlers;

public class RegistrationRequestValidator : AbstractValidator<RegistrationRequest>
{
    private readonly IConduitUsersCollectionProvider _usersCollectionProvider;

    public RegistrationRequestValidator(SharedUserValidator<RegistrationUserSubmitModel> sharedUser, IConduitUsersCollectionProvider usersCollectionProvider)
    {
        _usersCollectionProvider = usersCollectionProvider;

        RuleFor(x => x.Model.User)
            .SetValidator(sharedUser);

        RuleFor(x => x.Model.User.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Username must not be empty.")
            .MustAsync(async (username, cancellation) => await NotAlreadyExist(username, cancellation))
            .WithMessage("That username is already in use.");

        RuleFor(x => x.Model.User.Password)
            .NotEmpty().WithMessage("Password must not be empty.");

        // TODO: consider using zxcvbn library to provide a better measure of password strength
        // as the above password policy may be weak
    }

    private async Task<bool> NotAlreadyExist(string username, CancellationToken cancellationToken)
    {
        var collection = await _usersCollectionProvider.GetCollectionAsync();

        // bringing scope/bucket/cluster in to avoid hardcoding in the SQL++ query
        var scope = collection.Scope;
        var bucket = scope.Bucket;
        var cluster = bucket.Cluster;

        var checkForExistingUsernameSql = @$"
        SELECT RAW COUNT(*)
        FROM `{bucket.Name}`.`{scope.Name}`.`{collection.Name}` u
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
}