using Conduit.Web.Models;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Users.Handlers;

public class GetProfileHandlerTests : CouchbaseIntegrationTest
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
    public async Task GetCurrentUserRequestHandler_Is_Successful()
    {
        // *** arrange
        // arrange collections provider
        var usersCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();

        // arrange the handler
        var authService = new AuthService();
        var getCurrentUserHandler = new GetProfileHandler(new UserDataService(usersCollectionProvider, authService),
            new GetProfileRequestValidator());

        // arrange the request
        var username = $"user-{Path.GetRandomFileName()}";
        var request = new GetProfileRequest(username, null);

        // arrange the user already in the database
        var collection = await usersCollectionProvider.GetCollectionAsync();
        var userForDatabase = new User
        {
            Email = "doesntmatter@example.net",
            Password = "doesntmatter",
            PasswordSalt = "doesntmatterhereeither",
            Bio = $"lorem ipsum {Guid.NewGuid()}",
            Image = $"http://example.net/profile-{Guid.NewGuid()}.jpg"
        }
        ;
        await collection.InsertAsync(username, userForDatabase);

        // *** act
        var result = await getCurrentUserHandler.Handle(request, CancellationToken.None);

        // *** assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ProfileView, Is.Not.Null);
        Assert.That(result.ProfileView.Username, Is.EqualTo(username));
        Assert.That(result.ProfileView.Bio, Is.EqualTo(userForDatabase.Bio));
        Assert.That(result.ProfileView.Image, Is.EqualTo(userForDatabase.Image));
        Assert.That(result.ProfileView.Following, Is.EqualTo(false));
    }
}