using Conduit.Web.Models;
using Conduit.Web.Users.ViewModels;
using MediatR;
using Couchbase.Query;
using FluentValidation;

namespace Conduit.Web.Users.Handlers;

public class GetProfileHandler : IRequestHandler<GetProfileRequest, GetProfileResult>
{
    private readonly IConduitUsersCollectionProvider _usersCollectionProvider;
    private readonly IValidator<GetProfileRequest> _validator;

    public GetProfileHandler(IConduitUsersCollectionProvider usersCollectionProvider, IValidator<GetProfileRequest> validator)
    {
        _usersCollectionProvider = usersCollectionProvider;
        _validator = validator;
    }

    public async Task<GetProfileResult> Handle(GetProfileRequest request, CancellationToken cancellationToken)
    {
        // validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new GetProfileResult
            {
                ValidationErrors = validationResult.Errors
            };
        }

        // TODO: if JWT is specified, use that to determine if the logged-in user
        // is following this profile

        // TODO: Refactor into a UserRepository/UserDal

        var collection = await _usersCollectionProvider.GetCollectionAsync();
        var scope = collection.Scope;
        var bucket = scope.Bucket;
        var cluster = bucket.Cluster;
        var query = $@"
            SELECT u.*
            FROM `{bucket.Name}`.`{scope.Name}`.`{collection.Name}` u
            WHERE u.username = $username";
        var queryOptions = new QueryOptions()
            .Parameter("username", request.Username)
            .ScanConsistency(QueryScanConsistency.RequestPlus);
        var results = await cluster.QueryAsync<User>(query, queryOptions);

        var resultList = await results.ToListAsync(cancellationToken: cancellationToken);

        // should only ever be one or zero results
        var result = resultList.FirstOrDefault();

        // if user not found:
        if (result is null)
            return new GetProfileResult { UserNotFound = true };

        return new GetProfileResult
        {
            ProfileView = new ProfileViewModel
            {
                Username = result.Username,
                Bio = result.Bio,
                Image = result.Image,
                Following = false       // TODO: determine following or not
            }
        };
    }
}