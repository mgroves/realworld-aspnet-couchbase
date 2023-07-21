﻿using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Follows.Handlers;
using Conduit.Web.Follows.Services;
using Conduit.Web.Models;
using Conduit.Web.Users.Services;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

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

        // setup handler and dependencies
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
        var userToFollow = await _usersCollectionProvider.CreateUserInDatabase();
        var userFollower = await _usersCollectionProvider.CreateUserInDatabase();
        var request = new FollowUserRequest(userToFollow.Username, userFollower.Username);

        // *** act
        var result = await _handler.Handle(request, CancellationToken.None);

        // *** assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Profile.Username, Is.EqualTo(userToFollow.Username));
        await _followsCollectionProvider.AssertExists(userFollower.Username, f =>
        {
            Assert.That(f.Contains(userToFollow.Username), Is.True);
        });
    }
}