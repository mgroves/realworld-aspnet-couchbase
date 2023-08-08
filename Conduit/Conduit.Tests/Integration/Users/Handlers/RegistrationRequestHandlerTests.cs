using Conduit.Tests.TestHelpers.Dto;
using Conduit.Web.Models;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Tests.TestHelpers;

namespace Conduit.Tests.Integration.Users.Handlers;

[TestFixture]
public class RegistrationRequestHandlerTests : CouchbaseIntegrationTest
{
    private IConduitUsersCollectionProvider _usersCollectionProvider;
    private RegistrationRequestHandler _registrationRequestHandler;

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

        // setup the handler and dependencies
        _usersCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();
        var authService = AuthServiceHelper.Create();
        _registrationRequestHandler = new RegistrationRequestHandler(authService, new RegistrationRequestValidator(new SharedUserValidator<RegistrationUserSubmitModel>(), new UserDataService(_usersCollectionProvider, authService)), new UserDataService(_usersCollectionProvider, authService));
    }

    [Test]
    public async Task RegistrationRequestHandler_Is_Successful()
    {
        // *** arrange
        var request = RegistrationRequestHelper.Create();
    
        // *** act
        var result = await _registrationRequestHandler.Handle(request, CancellationToken.None);
    
        // *** assert
        // assert that the correct result was returned
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserView, Is.Not.Null);
        Assert.That(result.UserView.Username, Is.EqualTo(request.Model.User.Username));
        // assert that the newly registered user made it into the database
        await _usersCollectionProvider.AssertExists(request.Model.User.Username, x =>
        {
            Assert.That(x.Email, Is.EqualTo(request.Model.User.Email));
        });
    }
}