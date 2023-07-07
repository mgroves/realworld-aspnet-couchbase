using Conduit.Web.Models;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;
using Couchbase.Query;
using Conduit.Web.Users.ViewModels;
using Conduit.Web.Users.Handlers;
using System.Threading;

namespace Conduit.Web.Users.Services;

public interface IUserDataService
{
    Task<DataServiceResult<User>> GetUserByEmail(string email);
    Task<DataServiceResult<User>> RegisterNewUser(User userToInsert);
    Task<DataServiceResult<User>> GetUserByUsername(string username);
    Task UpdateUserFields(UpdateUserViewModelUser fieldsToUpdate);
    Task<bool> DoesExistUserByEmailAndUsername(string userEmail, string userUsername);
    Task<DataServiceResult<User>> GetProfileByUsername(string requestUsername);
}

public class UserDataService : IUserDataService
{
    private readonly IConduitUsersCollectionProvider _usersCollectionProvider;
    private readonly IAuthService _authService;

    public UserDataService(IConduitUsersCollectionProvider usersCollectionProvider, IAuthService authService)
    {
        _usersCollectionProvider = usersCollectionProvider;
        _authService = authService;
    }

    public async Task<DataServiceResult<User>> GetUserByEmail(string email)
    {
        var collection = await _usersCollectionProvider.GetCollectionAsync();

        var userResult = await collection.TryGetAsync(email);

        var doesUserExist = userResult.Exists;
        if (!doesUserExist)
            return new DataServiceResult<User>(null, DataResultStatus.NotFound);

        var user = userResult.ContentAs<User>();

        return new DataServiceResult<User>(user, DataResultStatus.Ok);
    }

    public async Task<DataServiceResult<User>> RegisterNewUser(User userToInsert)
    {
        var collection = await _usersCollectionProvider.GetCollectionAsync();

        try
        {
            await collection.InsertAsync(userToInsert.Email, userToInsert);
        }
        catch (DocumentExistsException)
        {
            // couchbase keys must be unique
            // registration shouldn't work if the email address is already in use
            return new DataServiceResult<User>(null, DataResultStatus.FailedToInsert);
        }

        return new DataServiceResult<User>(userToInsert, DataResultStatus.Ok);
    }

    public async Task<DataServiceResult<User>> GetUserByUsername(string username)
    {
        var collection = await _usersCollectionProvider.GetCollectionAsync();

        // bringing scope/bucket/cluster in to avoid hardcoding in the SQL++ query
        var scope = collection.Scope;
        var bucket = scope.Bucket;
        var cluster = bucket.Cluster;

        // TODO: consider revisiting this, and spinning out a separate DoesUserExist method on this data service (for performance/scale reasons)
        var getUserByUsernameSql = @$"
        SELECT u.*
        FROM `{bucket.Name}`.`{scope.Name}`.`{collection.Name}` u
        WHERE u.username = $username";

        // Can potentially be switched to use NotBounded scan consistency
        // for reduced latency, if the risk of two people trying to get the
        // same username within a very small window of time is small
        var queryOptions = new QueryOptions()
            .Parameter("username", username)
            .ScanConsistency(QueryScanConsistency.RequestPlus);

        var queryResult = await cluster.QueryAsync<User>(getUserByUsernameSql, queryOptions);
        var result = await queryResult.ToListAsync();

        if (!result.Any())
            return new DataServiceResult<User>(null, DataResultStatus.NotFound);

        return new DataServiceResult<User>(result.First(), DataResultStatus.Ok);

    }

    /// <summary>
    /// This method will ONLY upsert values from the argument
    /// that are NOT null or empty. If the value you specify is null
    /// or empty, then that field will be unchanged.
    /// </summary>
    public async Task UpdateUserFields(UpdateUserViewModelUser fieldsToUpdate)
    {
        // update ONLY the fields that are not empty
        // leave the other fields alone
        var collection = await _usersCollectionProvider.GetCollectionAsync();
        await collection.MutateInAsync(fieldsToUpdate.Email, specs =>
        {
            // TODO: create UpsertIfNotEmpty extension method to make this less verbose?
            // TODO: and possibly avoid hardcoding field names?
            if (!string.IsNullOrEmpty(fieldsToUpdate.Username))
                specs.Upsert("username", fieldsToUpdate.Username);
            if (!string.IsNullOrEmpty(fieldsToUpdate.Bio))
                specs.Upsert("bio", fieldsToUpdate.Bio);
            if (!string.IsNullOrEmpty(fieldsToUpdate.Image))
                specs.Upsert("image", fieldsToUpdate.Image);
            if (!string.IsNullOrEmpty(fieldsToUpdate.Password))
            {
                var salt = _authService.GenerateSalt();
                specs.Upsert("password", _authService.HashPassword(fieldsToUpdate.Password, salt));
                specs.Upsert("passwordSalt", salt);
            }
        });
    }

    public async Task<bool> DoesExistUserByEmailAndUsername(string email, string username)
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
            SELECT u.*
            FROM `{bucket.Name}`.`{scope.Name}`.`{collection.Name}` u
            WHERE u.username == $username
            AND META(u).id != $email";

        // Can potentially be switched to use NotBounded scan consistency
        // for reduced latency, if the risk of two people trying to get the
        // same username within a very small window of time is small
        var queryOptions = new QueryOptions()
            .Parameter("username", username)
            .Parameter("email", email)
            .ScanConsistency(QueryScanConsistency.RequestPlus);

        var result = await cluster.QueryAsync<int>(checkForClearUsername, queryOptions);

        var countResult = await result.ToListAsync();

        if (!countResult.Any())
            return true;

        var howManyMatches = countResult.First();

        return howManyMatches < 1;
    }

    public async Task<DataServiceResult<User>> GetProfileByUsername(string username)
    {
        var collection = await _usersCollectionProvider.GetCollectionAsync();
        var scope = collection.Scope;
        var bucket = scope.Bucket;
        var cluster = bucket.Cluster;
        var query = $@"
            SELECT u.username, u.bio, u.image
            FROM `{bucket.Name}`.`{scope.Name}`.`{collection.Name}` u
            WHERE u.username = $username";
        var queryOptions = new QueryOptions()
            .Parameter("username", username)
            .ScanConsistency(QueryScanConsistency.RequestPlus);
        var results = await cluster.QueryAsync<User>(query, queryOptions);

        var resultList = await results.ToListAsync();

        // should only ever be one or zero results
        var result = resultList.FirstOrDefault();

        // if user not found
        if (result is null)
            return new DataServiceResult<User>(null, DataResultStatus.NotFound);

        return new DataServiceResult<User>(result, DataResultStatus.Ok);
    }
}