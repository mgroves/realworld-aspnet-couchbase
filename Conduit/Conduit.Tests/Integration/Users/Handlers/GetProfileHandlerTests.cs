using Conduit.Tests.TestHelpers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Follows.Services;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Users.Handlers;

public class GetProfileHandlerTests : CouchbaseIntegrationTest
{
    private IConduitUsersCollectionProvider _usersCollectionProvider;
    private GetProfileHandler _getProfileHandler;
    private IConduitFollowsCollectionProvider _followsCollectionProvider;
    private AuthService _authService;

    [OneTimeSetUp]
    public override async Task Setup()
    {
        await base.Setup();

        // setup handler
        _usersCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();
        _followsCollectionProvider = ServiceProvider.GetRequiredService<IConduitFollowsCollectionProvider>();
        var genAiService = ServiceProvider.GetRequiredService<IGenerativeAiService>();

        _authService = AuthServiceHelper.Create();
        _getProfileHandler = new GetProfileHandler(new UserDataService(_usersCollectionProvider, _authService, genAiService),
            new GetProfileRequestValidator(),
            new FollowsDataService(_followsCollectionProvider),
            _authService);
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

    [Test]
    public async Task GetCurrentUserRequestHandler_When_Following_AnotherUser()
    {
        // *** arrange
        // arrange the user already in the database
        // arrange them following another user
        var userInDatabaseDoingTheFollowing = await _usersCollectionProvider.CreateUserInDatabase(username: "doingTheFollowing-" + Path.GetRandomFileName());
        var userInDatabaseBeingFollowed = await _usersCollectionProvider.CreateUserInDatabase(username: "theFollowed-" + Path.GetRandomFileName());
        await _followsCollectionProvider.CreateFollow(userInDatabaseBeingFollowed.Username,
            userInDatabaseDoingTheFollowing.Username);

        // arrange the request
        var bearerToken = _authService.GenerateJwtToken(userInDatabaseDoingTheFollowing.Email, userInDatabaseDoingTheFollowing.Username);
        var request = new GetProfileRequest(userInDatabaseBeingFollowed.Username, bearerToken);

        // *** act
        var result = await _getProfileHandler.Handle(request, CancellationToken.None);

        // *** assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ProfileView, Is.Not.Null);
        Assert.That(result.ProfileView.Following, Is.EqualTo(true));
    }

    [Test]
    public async Task GetCurrentUserRequestHandler_When_NOT_Following_AnotherUser()
    {
        // *** arrange
        // arrange the users already in the database
        // but they aren't following anyone
        var userInDatabaseDoingTheFollowing = await _usersCollectionProvider.CreateUserInDatabase(username: "doingTheFollowing-" + Path.GetRandomFileName());
        var userInDatabaseBeingFollowed = await _usersCollectionProvider.CreateUserInDatabase(username: "theFollowed-" + Path.GetRandomFileName());

        // arrange the request
        var bearerToken = _authService.GenerateJwtToken(userInDatabaseDoingTheFollowing.Email, userInDatabaseDoingTheFollowing.Username);
        var request = new GetProfileRequest(userInDatabaseBeingFollowed.Username, bearerToken);

        // *** act
        var result = await _getProfileHandler.Handle(request, CancellationToken.None);

        // *** assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ProfileView, Is.Not.Null);
        Assert.That(result.ProfileView.Following, Is.EqualTo(false));
    }
}