using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Users.Services;

namespace Conduit.Tests.TestHelpers.Data;

public static class UserHelper
{
    /// <summary>
    /// Create a User object (does not write to database)
    /// Values will be populate with well-formed values
    /// </summary>
    public static User CreateUser(
        string? username = null,
        string? email = null,
        string? bio = null,
        string? image = null,
        string? password = null)
    {
        username ??= "user-" + Path.GetRandomFileName();
        email ??= "email-" + Path.GetRandomFileName() + "@example.net";
        bio ??= "Lorem Ipsum bio " + Path.GetRandomFileName();
        image ??= "http://example.net/" + Path.GetRandomFileName() + ".jpg";
        password ??= "ValidPassword1#-" + Path.GetRandomFileName();

        var authService = AuthServiceHelper.Create();
        var salt = authService.GenerateSalt();

        var user = new User
        {
            Email = email,
            Password = authService.HashPassword(password, salt),
            PasswordSalt = salt,
            Bio = bio,
            Image = image,
            Username = username
        };

        return user;
    }

    /// <summary>
    /// Assert that a user exists
    /// </summary>
    /// <param name="username">Required: the username (key) of the user document</param>
    /// <param name="assertions">Optional: additional assertions to run on the user</param>
    public static async Task AssertExists(this IConduitUsersCollectionProvider @this, string username, Action<User>? assertions = null)
    {
        var collection = await @this.GetCollectionAsync();
        var userInDatabaseResult = await collection.GetAsync(username);
        var userInDatabaseObj = userInDatabaseResult.ContentAs<User>();

        if(assertions != null)
            assertions(userInDatabaseObj);

        // if we made it this far, the user was retrieved
        // and the assertions passed
        Assert.That(true);
    }

    /// <summary>
    /// Create a user in the database. All optional parameters
    /// will be populated with a well-formed, at least partially random value
    /// </summary>
    public static async Task<User> CreateUserInDatabase(this IConduitUsersCollectionProvider @this,
        string? username = null,
        string? email = null,
        string? bio = null,
        string? image = null,
        string? password = null)
    {
        var user = CreateUser(username, email, bio, image, password);

        var collection = await @this.GetCollectionAsync();

        await collection.InsertAsync(user.Username, user);

        return user;
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="user">User entity</param>
    public static async Task DeleteUserFromDatabase(this IConduitUsersCollectionProvider @this, User user)
    {
        var collection = await @this.GetCollectionAsync();

        await collection.RemoveAsync(user.Username);
    }
}