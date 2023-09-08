using Conduit.Tests.TestHelpers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Users.Handlers;

public class GetCurrentUserRequestHandlerTests : CouchbaseIntegrationTest
{
    private IConduitUsersCollectionProvider _usersCollectionProvider;
    private GetCurrentUserHandler _getCurrentUserHandler;

    [OneTimeSetUp]
    public override async Task Setup()
    {
        await base.Setup();

        // setup handler and dependencies
        _usersCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();
        var authService = AuthServiceHelper.Create();
        _getCurrentUserHandler = new GetCurrentUserHandler(authService, new UserDataService(_usersCollectionProvider, authService));
    }

    [Test]
    public async Task GetCurrentUserRequestHandler_Is_Successful()
    {
        // *** arrange

        // arrange the request
        var email = $"valid{Path.GetRandomFileName()}@example.net";
        var username = $"valid{Path.GetRandomFileName()}";
        var fakeToken = AuthServiceHelper.Create().GenerateJwtToken(email, username);
        var request = new GetCurrentUserRequest(fakeToken);

        // arrange the current user already in the database
        await _usersCollectionProvider.CreateUserInDatabase(username: username, email: email);

        // *** act
        var result = await _getCurrentUserHandler.Handle(request, CancellationToken.None);

        // *** assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsInvalidToken, Is.False);
        Assert.That(result.UserView.Email, Is.EqualTo(email));
    }
}