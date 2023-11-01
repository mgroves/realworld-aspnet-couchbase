using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;
using Couchbase.Query;
using Conduit.Web.Users.ViewModels;
using Microsoft.AspNetCore.Mvc.Formatters;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Dto;

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

        // bringing scope/bucket/cluster in to avoid hardcoding in the SQL++ query
        var scope = collection.Scope;
        var bucket = scope.Bucket;
        var cluster = bucket.Cluster;

        // TODO: consider revisiting this, and spinning out a separate DoesUserExist method on this data service (for performance/scale reasons)
        var getUserByUsernameSql = @$"
        SELECT u AS Document, META(u).id
        FROM `{bucket.Name}`.`{scope.Name}`.`{collection.Name}` u
        WHERE u.email = $email";

        // Can potentially be switched to use NotBounded scan consistency
        // for reduced latency, if the risk of two people trying to get the
        // same username within a very small window of time is small
        var queryOptions = new QueryOptions()
            .Parameter("email", email)
            .ScanConsistency(QueryScanConsistency.RequestPlus);

        // use KeyWrapper<User> here instead of User directly
        // but still return DataServiceResult<User>, and just map the ID in code
        // this is a workaround for [JsonIgnore] being honored by QueryAsync
        var queryResult = await cluster.QueryAsync<KeyWrapper<User>>(getUserByUsernameSql, queryOptions);
        var result = await queryResult.ToListAsync();

        if (!result.Any())
            return new DataServiceResult<User>(null, DataResultStatus.NotFound);

        var userToReturn = result.First();
        userToReturn.Document.Username = userToReturn.Id;

        return new DataServiceResult<User>(userToReturn.Document, DataResultStatus.Ok);
    }

    public async Task<DataServiceResult<User>> RegisterNewUser(User userToInsert)
    {
        var collection = await _usersCollectionProvider.GetCollectionAsync();

        try
        {
            await collection.InsertAsync(userToInsert.Username, userToInsert);
        }
        catch (DocumentExistsException)
        {
            // couchbase keys must be unique
            // registration shouldn't work if the username is already in use
            return new DataServiceResult<User>(null, DataResultStatus.FailedToInsert);
        }

        return new DataServiceResult<User>(userToInsert, DataResultStatus.Ok);
    }

    public async Task<DataServiceResult<User>> GetUserByUsername(string username)
    {
        var collection = await _usersCollectionProvider.GetCollectionAsync();
        
        var userResult = await collection.TryGetAsync(username);
        
        var doesUserExist = userResult.Exists;
        if (!doesUserExist)
            return new DataServiceResult<User>(null, DataResultStatus.NotFound);
        
        var user = userResult.ContentAs<User>();
        user.Username = username;
        
        return new DataServiceResult<User>(user, DataResultStatus.Ok);
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
        await collection.MutateInAsync(fieldsToUpdate.Username, specs =>
        {
            if (!string.IsNullOrEmpty(fieldsToUpdate.Email))
                specs.Upsert("email", fieldsToUpdate.Email);
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
            WHERE u.email == $email
            AND META(u).id != $username";

        // Can potentially be switched to use NotBounded scan consistency
        // for reduced latency, if the risk of two people trying to get the
        // same username within a very small window of time is small
        var queryOptions = new QueryOptions()
            .Parameter("username", username)
            .Parameter("email", email)
            .ScanConsistency(QueryScanConsistency.RequestPlus);

        var result = await cluster.QueryAsync<int>(checkForClearUsername, queryOptions);

        var countResult = await result.ToListAsync();

        // if no results at all, then it's okay
        if (!countResult.Any())
            return false;

        // if there's a result, but it's less than 1, then it's okay
        var howManyMatches = countResult.First();

        return howManyMatches < 1;
    }

    public async Task<DataServiceResult<User>> GetProfileByUsername(string username)
    {
        var collection = await _usersCollectionProvider.GetCollectionAsync();

        var userResult = await collection.GetAsync(username);
        var user = userResult.ContentAs<User>();

        return new DataServiceResult<User>(new User
        {
            Bio = user.Bio,
            Image = user.Image,
            Username = username
        }, DataResultStatus.Ok);
    }
}