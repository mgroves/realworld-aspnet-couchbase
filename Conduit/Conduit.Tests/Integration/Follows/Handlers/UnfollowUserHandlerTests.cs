using Conduit.Tests.TestHelpers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Follows.Handlers;
using Conduit.Web.Follows.Services;
using Conduit.Web.Models;
using Conduit.Web.Users.Services;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Follows.Handlers;

[TestFixture]
public class UnfollowUserHandlerIntegrationTest : CouchbaseIntegrationTest
{
    private UnfollowUserRequest _request;
    private UnfollowUserHandler _handler;
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

        // setup handler and dependencies
        var authService = AuthServiceHelper.Create();
        _handler = new UnfollowUserHandler(
            new UserDataService(_usersCollectionProvider, authService),
            new FollowsDataService(_followsCollectionProvider, authService),
            new UnfollowUserRequestValidator());
    }

    [Test]
    public async Task UnfollowUserHandler_Handle_ReturnsSuccess()
    {
        // in this test, userFollower will be arranged to follow TWO
        // users, and then unfollow #1
        // then both are tested for: this is to make sure there's no false positives
        // *** arrange
        var userToFollow1 = await _usersCollectionProvider.CreateUserInDatabase();
        var userToFollow2 = await _usersCollectionProvider.CreateUserInDatabase();
        var userFollower = await _usersCollectionProvider.CreateUserInDatabase();
        await _followsCollectionProvider.CreateFollow(userToFollow1.Username, userFollower.Username);
        await _followsCollectionProvider.CreateFollow(userToFollow2.Username, userFollower.Username);
        var request = new UnfollowUserRequest(userToFollow1.Username, userFollower.Username);

        // *** act
        var result = await _handler.Handle(request, CancellationToken.None);

        // *** assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Profile.Username, Is.EqualTo(userToFollow1.Username));
        await _followsCollectionProvider.AssertExists(userFollower.Username, f =>
        {
            Assert.That(f.Contains(userToFollow1.Username), Is.False);
            Assert.That(f.Contains(userToFollow2.Username), Is.True);
        });
    }
}