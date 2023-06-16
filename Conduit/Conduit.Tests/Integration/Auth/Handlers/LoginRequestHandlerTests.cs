using Conduit.Web.Auth.Handlers;
using Conduit.Web.Auth.Services;
using Conduit.Web.Auth.ViewModels;
using Conduit.Web.Models;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Couchbase;

namespace Conduit.Tests.Integration.Auth.Handlers;

[TestFixture]
public class LoginRequestHandlerTests
{
    private CouchbaseContainer _container;
    private IConduitUsersCollectionProvider _usersCollectionProvider;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _container = new CouchbaseBuilder()
            .WithImage("couchbase/server:7.2.0")
            .Build();

        await _container.StartAsync();

        // create a DI setup, in order to get Couchbase DI objects 
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCouchbase(options =>
            {
                options.ConnectionString = _container.GetConnectionString();
                options.UserName = "Administrator";
                options.Password = "password";
            });
        var bucketName = _container.Buckets.First().Name;
        services.AddCouchbaseBucket<IConduitBucketProvider>(bucketName, bucketBuilder =>
        {   
            bucketBuilder
                .AddScope("_default")
                .AddCollection<IConduitUsersCollectionProvider>("_default");
        });

        // build DI setup
        var builder = services.BuildServiceProvider();

        // get cluster objects to that WaitUntilReadyAsync can be called
        // just to make sure the cluster created in the testcontainers image is ready
        var clusterProvider = builder.GetRequiredService<IClusterProvider>();
        var cluster = await clusterProvider.GetClusterAsync();
        await cluster.WaitUntilReadyAsync(TimeSpan.FromSeconds(30));

        // need this Couchbase DI object for the test (consider moving into test)
        _usersCollectionProvider = builder.GetRequiredService<IConduitUsersCollectionProvider>();
    }

    [Test]
    public async Task LoginRequestHandler_Is_Successful()
    {
        // *** arrange
        // arrange the handler
        var authService = new AuthService();
        var loginRequestHandler = new LoginRequestHandler(_usersCollectionProvider, authService, new LoginRequestValidator());

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
        var collection = await _usersCollectionProvider.GetCollectionAsync();
        var salt = authService.GenerateSalt();
        await collection.InsertAsync(userViewModel.Email, new User
        {
            Username = "doesntmatter",
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

    [OneTimeTearDown]
    public async Task Teardown()
    {
        await _container.StopAsync();
    }
}