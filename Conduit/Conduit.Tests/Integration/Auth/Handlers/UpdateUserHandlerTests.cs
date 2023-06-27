using Conduit.Web.Models;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Auth.Handlers;

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
            new UpdateUserRequestValidator(usersCollectionProvider, new SharedUserValidator<UpdateUserViewModelUser>()),
            usersCollectionProvider,
            new AuthService());

        // arrange existing user in database to update
        var collection = await usersCollectionProvider.GetCollectionAsync();
        var salt = new AuthService().GenerateSalt();
        var existingUser = new User
        {
            Username = $"validUsername{Path.GetRandomFileName()}",
            Password = new AuthService().HashPassword("ValidPassword1#", salt),
            PasswordSalt = salt,
            Bio = "lorem ipsum",
            Image = "http://example.net/profile.jpg"
        };
        var email = $"valid{Path.GetRandomFileName()}@example.net";
        await collection.InsertAsync(email, existingUser);

        // arrange request to change ONLY bio
        var request = new UpdateUserRequest(new UpdateUserSubmitModel
        {
            User = new UpdateUserViewModelUser
            {
                Email = email,
                Bio = "New bio " + Guid.NewGuid()
            }
        });

        // *** act
        var result = await handler.Handle(request, CancellationToken.None);

        // *** assert
        // get user directly from DB
        var userFromDb = await collection.GetAsync(email);
        var userFromDbDoc = userFromDb.ContentAs<User>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors == null || !result.ValidationErrors.Any(), Is.EqualTo(true));
        Assert.That(userFromDbDoc.Bio, Is.EqualTo(request.Model.User.Bio)); // assert that bio WAS changed
        Assert.That(userFromDbDoc.Image, Is.EqualTo(existingUser.Image));  // assert that image was NOT changed
        Assert.That(userFromDbDoc.Username, Is.EqualTo(existingUser.Username)); // assert that username was NOT changed
;    }
}