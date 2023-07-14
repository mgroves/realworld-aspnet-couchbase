using Conduit.Web.Follows.Handlers;
using Conduit.Web.Follows.Services;
using Conduit.Web.Models;
using Conduit.Web.Users.Services;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Conduit.Tests.Integration.Follows.Handlers;

[TestFixture]
public class FollowUserHandlerIntegrationTest : CouchbaseIntegrationTest
{
    private FollowUserRequest _request;
    private FollowUserHandler _handler;
    private IConduitFollowsCollectionProvider _followsCollectionProvider;
    private IConduitUsersCollectionProvider _usersCollectionProvider;

    [SetUp]
    public async Task SetUp()
    {
        await base.Setup();

        ServiceCollection.AddCouchbaseBucket<IConduitBucketProvider>("ConduitIntegrationTests", b =>
        {
            b
                .AddScope("_default")
                .AddCollection<IConduitFollowsCollectionProvider>("Follows");
            b
                .AddScope("_default")
                .AddCollection<IConduitUsersCollectionProvider>("Users");
        });

        _followsCollectionProvider = ServiceProvider.GetRequiredService<IConduitFollowsCollectionProvider>();
        _usersCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();

        var authService = new AuthService();
        _handler = new FollowUserHandler(
            new UserDataService(_usersCollectionProvider, authService),
            new FollowsDataService(_followsCollectionProvider),
            new FollowUserRequestValidator());
    }

    [Test]
    public async Task FollowUserHandler_Handle_ReturnsSuccess()
    {
        // *** arrange
        // put some users into the database to begin with
        var userToFollow = $"{Path.GetRandomFileName()}";
        var userFollower = $"{Path.GetRandomFileName()}";
        var userCollection = await _usersCollectionProvider.GetCollectionAsync();
        await userCollection.InsertAsync(userToFollow, new User()
        {
            Bio = "I love being followed!",
            Email = "followed@example.net"
        });
        await userCollection.InsertAsync(userFollower, new User()
        {
            Bio = "I love following users!",
            Email = "follower@example.net"
        });

        // arrange request to follow
        var request = new FollowUserRequest(userToFollow, userFollower);

        // *** act
        var result = await _handler.Handle(request, CancellationToken.None);

        // get the "following" document directly from database
        var followCollection = await _followsCollectionProvider.GetCollectionAsync();
        var followDocId = $"{userFollower}::follows";
        var followResult = await followCollection.TryGetAsync(followDocId);

        // *** assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Profile.Username, Is.EqualTo(userToFollow));
        Assert.That(followResult.Exists, Is.True);
        Assert.That(await followCollection.Set<string>(followDocId).ContainsAsync(userToFollow), Is.True);
    }
}