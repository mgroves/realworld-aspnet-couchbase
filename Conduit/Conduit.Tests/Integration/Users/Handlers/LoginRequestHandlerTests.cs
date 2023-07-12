using Conduit.Web.Models;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration.Users.Handlers;

[TestFixture]
public class LoginRequestHandlerTests : CouchbaseIntegrationTest
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
    public async Task LoginRequestHandler_Is_Successful()
    {
        // *** arrange
        // arrange collections provider
        var usersCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();

        // arrange the handler
        var authService = new AuthService();
        var loginRequestHandler = new LoginRequestHandler(authService, new LoginRequestValidator(), new UserDataService(usersCollectionProvider, authService));

        // arrange the request
        var userViewModel = new LoginUserViewModel
        {
            Email = $"matt{Path.GetRandomFileName()}@example.com",
            Password = $"{Path.GetRandomFileName()}{Guid.NewGuid()}"
        };
        var request = new LoginRequest(new LoginSubmitModel
        {
            User = userViewModel
        });

        // arrange for a user to already be in the database
        var collection = await usersCollectionProvider.GetCollectionAsync();
        var salt = authService.GenerateSalt();
        await collection.InsertAsync("doesntmatter", new User
        {
            Email = userViewModel.Email,
            Bio = "Lorem ipsum",
            Image = "http://google.com/image.jpg",
            Password = authService.HashPassword(userViewModel.Password, salt),
            PasswordSalt = salt
        });

        // *** act
        var result = await loginRequestHandler.Handle(request, CancellationToken.None);

        // *** assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsUnauthorized, Is.False);
        Assert.That(result.UserView, Is.Not.Null);
        Assert.That(result.UserView.Email, Is.EqualTo(userViewModel.Email));
    }
}