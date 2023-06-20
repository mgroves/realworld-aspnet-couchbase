using Conduit.Web.Models;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Auth.Handlers;

public class RegistrationRequestHandlerTests : CouchbaseIntegrationTest
{
    private ServiceProvider _serviceProvider;

    [OneTimeSetUp]
    public override async Task Setup()
    {
        await base.Setup();

        ServiceCollection.AddCouchbaseBucket<IConduitBucketProvider>("Conduit", b =>
        {
            b
                .AddScope("_default")
                .AddCollection<IConduitUsersCollectionProvider>("Users");
        });
        _serviceProvider = ServiceCollection.BuildServiceProvider();
    }

    [Test]
    public async Task RegistrationRequestHandler_Is_Successful()
    {
        // *** arrange
        // arrange collection provider
        var usersCollectionProvider = _serviceProvider.GetRequiredService<IConduitUsersCollectionProvider>();

        // arrange the handler
        var authService = new AuthService();
        var registrationRequestHandler = new RegistrationRequestHandler(usersCollectionProvider, authService, new RegistrationRequestValidator(new SharedUserValidator<RegistrationUserSubmitModel>(), usersCollectionProvider));

        // arrange the request
        var submittedInfo = new RegistrationUserSubmitModel
        {
            Username = $"validuser{Path.GetRandomFileName()}",
            Password = $"{Path.GetRandomFileName()}{Guid.NewGuid()}",
            Email = $"matt{Path.GetRandomFileName()}@example.com"
        };
        var request = new RegistrationRequest(new RegistrationSubmitModel
        {
            User = submittedInfo
        });

        // *** act
        var result = await registrationRequestHandler.Handle(request, CancellationToken.None);

        // *** assert
        // assert that the correct result was returned
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserView, Is.Not.Null);
        Assert.That(result.UserView.Username, Is.EqualTo(submittedInfo.Username));
        // assert that the data made it into the database
        var collection = await usersCollectionProvider.GetCollectionAsync();
        var userInDatabaseResult = await collection.GetAsync(submittedInfo.Email);
        var userInDatabaseObj = userInDatabaseResult.ContentAs<User>();
        Assert.That(userInDatabaseObj.Username, Is.EqualTo(submittedInfo.Username));
    }
}