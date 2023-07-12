using Conduit.Web.Models;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Users.Handlers;

public class UpdateUserHandlerTests : CouchbaseIntegrationTest
{
    [OneTimeSetUp]
    public override async Task Setup()
    {
        await base.Setup();

        ServiceCollection.AddCouchbaseBucket<IConduitBucketProvider>("ConduitIntegrationTests", b =>
        {
            b
                .AddScope("_default")
                .AddCollection<IConduitUsersCollectionProvider>("Users");
        });
    }

    [Test]
    public async Task UpdateUserHandler_Is_Successful()
    {
        // *** arrange
        // arrange collection provider
        var usersCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();

        // arrange handler
        var handler = new UpdateUserHandler(
            new UpdateUserRequestValidator(new UserDataService(usersCollectionProvider, new AuthService()), new SharedUserValidator<UpdateUserViewModelUser>()),
            new AuthService(),
            new UserDataService(usersCollectionProvider, new AuthService()));

        // arrange existing user in database to update
        var collection = await usersCollectionProvider.GetCollectionAsync();
        var salt = new AuthService().GenerateSalt();
        var username = $"validUsername{Path.GetRandomFileName()}";
        var existingUser = new User
        {
            Email = $"oldemail-{Path.GetRandomFileName()}@example.net",
            Password = new AuthService().HashPassword("ValidPassword1#", salt),
            PasswordSalt = salt,
            Bio = "lorem ipsum",
            Image = "http://example.net/profile.jpg"
        };
        await collection.InsertAsync(username, existingUser);

        // arrange request to change ONLY bio
        var request = new UpdateUserRequest(new UpdateUserSubmitModel
        {
            User = new UpdateUserViewModelUser
            {
                Username = username,
                Email = $"newemail-{Path.GetRandomFileName()}@example.net",     // new email
                Bio = "New bio " + Guid.NewGuid()                           // new bio
                                                                            // image is not specified, so it's unchanged
            }
        });

        // *** act
        var result = await handler.Handle(request, CancellationToken.None);

        // *** assert
        // get user directly from DB
        var userFromDb = await collection.GetAsync(username);
        var userFromDbDoc = userFromDb.ContentAs<User>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors == null || !result.ValidationErrors.Any(), Is.EqualTo(true));
        Assert.That(userFromDbDoc.Bio, Is.EqualTo(request.Model.User.Bio)); // assert that bio WAS changed
        Assert.That(userFromDbDoc.Image, Is.EqualTo(existingUser.Image));  // assert that image was NOT changed
        Assert.That(userFromDbDoc.Email, Is.EqualTo(request.Model.User.Email)); // assert that email WAS changed
;    }
}