using Conduit.Tests.TestHelpers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Tests.TestHelpers.Dto.Handlers;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Users.Handlers;

public class UpdateUserHandlerTests : CouchbaseIntegrationTest
{
    private IConduitUsersCollectionProvider _usersCollectionProvider;
    private UpdateUserHandler _handler;

    [OneTimeSetUp]
    public override async Task Setup()
    {
        await base.Setup();

        _usersCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();

        // arrange handler and dependencies
        var authService = AuthServiceHelper.Create();
        _handler = new UpdateUserHandler(
            new UpdateUserRequestValidator(new UserDataService(_usersCollectionProvider, authService), new SharedUserValidator<UpdateUserViewModelUser>()),
            authService,
            new UserDataService(_usersCollectionProvider, authService));
    }

    [Test]
    public async Task UpdateUserHandler_Is_Successful()
    {
        // *** arrange
        // arrange existing user in database to update
        var user = await _usersCollectionProvider.CreateUserInDatabase();

        // arrange request to change bio and email (set the other values to null)
        var request = UpdateUserRequestHelper.Create(username: user.Username, makePasswordNull: true, makeImageNull: true);

        // *** act
        var result = await _handler.Handle(request, CancellationToken.None);

        // *** assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors == null || !result.ValidationErrors.Any(), Is.EqualTo(true));
        await _usersCollectionProvider.AssertExists(user.Username, u =>
        {
            Assert.That(u.Bio, Is.EqualTo(request.Model.User.Bio)); // assert that bio WAS changed
            Assert.That(u.Email, Is.EqualTo(request.Model.User.Email)); // assert that email WAS changed
            Assert.That(u.Image, Is.EqualTo(user.Image));  // assert that image was NOT changed
            Assert.That(u.Password, Is.EqualTo(user.Password));  // assert that password was NOT changed
        });
;    }
}