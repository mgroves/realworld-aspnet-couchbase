using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Models;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Users.Handlers;

public class GetProfileHandlerTests : CouchbaseIntegrationTest
{
    private IConduitUsersCollectionProvider _usersCollectionProvider;
    private GetProfileHandler _getProfileHandler;

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

        // setup handler
        _usersCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();
        var authService = new AuthService();
        _getProfileHandler = new GetProfileHandler(new UserDataService(_usersCollectionProvider, authService),
            new GetProfileRequestValidator());
    }

    [Test]
    public async Task GetCurrentUserRequestHandler_Is_Successful()
    {
        // *** arrange
        // arrange the request
        var username = $"user-{Path.GetRandomFileName()}";
        var request = new GetProfileRequest(username, null);

        // arrange the user already in the database
        var userInDatabase = await _usersCollectionProvider.CreateUserInDatabase(username: username);

        // *** act
        var result = await _getProfileHandler.Handle(request, CancellationToken.None);

        // *** assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ProfileView, Is.Not.Null);
        Assert.That(result.ProfileView.Username, Is.EqualTo(username));
        Assert.That(result.ProfileView.Bio, Is.EqualTo(userInDatabase.Bio));
        Assert.That(result.ProfileView.Image, Is.EqualTo(userInDatabase.Image));
        Assert.That(result.ProfileView.Following, Is.EqualTo(false));
    }
}