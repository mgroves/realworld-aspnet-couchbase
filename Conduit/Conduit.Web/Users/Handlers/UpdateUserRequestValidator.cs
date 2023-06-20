using Conduit.Web.Models;
using Conduit.Web.Users.ViewModels;
using Couchbase.Query;
using FluentValidation;

namespace Conduit.Web.Users.Handlers;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    private readonly IConduitUsersCollectionProvider _usersCollectionProvider;

    public UpdateUserRequestValidator(IConduitUsersCollectionProvider usersCollectionProvider, SharedUserValidator<UpdateUserViewModelUser> sharedUser)
    {
        _usersCollectionProvider = usersCollectionProvider;

        RuleFor(x => x.Model.User)
            .SetValidator(sharedUser);

        RuleFor(u => u)
            .Cascade(CascadeMode.Stop)
            .Must(HaveAtLeastOneChange)
                .WithMessage("You must specify a value for at least one of: username, password, image, bio.");

        RuleFor(u => u.Model)
            .Cascade(CascadeMode.Stop)
            .MustAsync(NotMatchAnyOtherUsername)
            .WithMessage("That username is already taken.");

        RuleFor(u => u.Model.User.Bio)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(500).When(r => !string.IsNullOrEmpty(r.Model.User.Bio))
                .WithMessage("Bio is limited to 500 characters.");

        RuleFor(u => u.Model.User.Image)
            .Cascade(CascadeMode.Stop)
            .Must(BeAValidUrl).When(r => !string.IsNullOrEmpty(r.Model.User.Image)).WithMessage("Image URL must be valid.")
            .Must(BeAValidImageUrl).When(r => !string.IsNullOrEmpty(r.Model.User.Image)).WithMessage("Image URL must be JPG, JPEG, or PNG.");
    }

    private async Task<bool> NotMatchAnyOtherUsername(UpdateUserSubmitModel model, CancellationToken cancellationToken)
    {
        var collection = await _usersCollectionProvider.GetCollectionAsync();
        var scope = collection.Scope;
        var bucket = scope.Bucket;
        var cluster = bucket.Cluster;

        // SQL++ query
        // are there any usernames OTHER than the one currently in use by this email
        // address that match the new username?
        // TODO: extension method to build the fully qualified collection name?
        var checkForClearUsername = $@"
            SELECT RAW COUNT(*)
            FROM `{bucket.Name}`.`{scope.Name}`.`{collection.Name}` u
            WHERE u.username == $username
            AND META(u).id != $email";

        // Can potentially be switched to use NotBounded scan consistency
        // for reduced latency, if the risk of two people trying to get the
        // same username within a very small window of time is small
        var queryOptions = new QueryOptions()
            .Parameter("username", model.User.Username)
            .Parameter("email", model.User.Email)
            .ScanConsistency(QueryScanConsistency.RequestPlus);

        var result = await cluster.QueryAsync<int>(checkForClearUsername, queryOptions);

        var countResult = await result.ToListAsync(cancellationToken);

        if (!countResult.Any())
            return true;

        var howManyMatches = countResult.First();

        return howManyMatches < 1;
    }

    private bool BeAValidImageUrl(string url)
    {
        var uri = new Uri(url);

        var fileName = Path.GetFileName(uri.LocalPath);
        var extension = Path.GetExtension(fileName).ToLower();

        return extension is ".jpg" or ".png" or ".jpeg";
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult);
    }

    private bool HaveAtLeastOneChange(UpdateUserRequest model)
    {
        if (!string.IsNullOrEmpty(model.Model.User.Password))
            return true;
        if (!string.IsNullOrEmpty(model.Model.User.Bio))
            return true;
        if (!string.IsNullOrEmpty(model.Model.User.Image))
            return true;
        if (!string.IsNullOrEmpty(model.Model.User.Username))
            return true;
        return false;
    }
}