using System.Text;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Tests.TestHelpers.Dto;
using Conduit.Web;
using System.Net;
using Conduit.Tests.TestHelpers;
using Conduit.Web.Models;
using Conduit.Web.Users.ViewModels;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Conduit.Tests.Integration.Users.Controllers.UserController;

public class LoginTests : CouchbaseIntegrationTest
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private IConduitUsersCollectionProvider _usersCollectionProvider;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();

        // arrange ASP.NET
        // TODO: pull from user secrets and environment variables
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    // Add environment variables
                    configBuilder.AddEnvironmentVariables();
                    configBuilder.AddUserSecrets<CouchbaseIntegrationTest>();
                });
            });

        _client = _factory.CreateClient();
        // arrange db access for arranging
        ServiceCollection.AddCouchbaseBucket<IConduitBucketProvider>("ConduitIntegrationTests", b =>
        {
            b
                .AddScope("_default")
                .AddCollection<IConduitUsersCollectionProvider>("Users");
        });

        // setup handler and dependencies
        _usersCollectionProvider = ServiceProvider.GetRequiredService<IConduitUsersCollectionProvider>();
    }

    [TestCase("","")]
    [TestCase(null,"")]
    [TestCase("",null)]
    [TestCase(null,null)]
    public async Task LoginWithNullOrEmptyBothCredentials_Should_Fail(string? email, string? password)
    {
        // Arrange
        var payload = LoginSubmitModelHelper.Create(
            email: email,
            password: password,
            makeEmailNull: email == null,
            makePasswordNull: password == null);
        var content = new StringContent(JsonConvert.SerializeObject(payload), 
            Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users/login", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.NotNull(responseString);
        Assert.That(responseString, Contains.Substring("Email address must not be empty."));
        Assert.That(responseString, Contains.Substring("Password must not be empty."));
    }

    [TestCase("validemail@example.net", "", "Password must not be empty.")]
    [TestCase("validemail@example.net", null, "Password must not be empty.")]
    [TestCase(null, "Valid!2#Password", "Email address must not be empty.")]
    [TestCase("", "Valid!2#Password", "Email address must not be empty.")]
    public async Task LoginWithNullOrEmptyOneCredentials_Should_Fail(string? email, string? password, string expectedErrorMessage)
    {
        // Arrange
        var payload = LoginSubmitModelHelper.Create(
            email: email,
            password: password,
            makeEmailNull: email == null,
            makePasswordNull: password == null);
        var content = new StringContent(JsonConvert.SerializeObject(payload),
            Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users/login", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.NotNull(responseString);
        Assert.That(responseString, Is.EqualTo(expectedErrorMessage));
    }

    [Test]
    public async Task LoginWithCorrectCredentialsShouldReturnSuccess()
    {
        // Arrange
        var payload = LoginSubmitModelHelper.Create();
        var userInDatabase = await _usersCollectionProvider.CreateUserInDatabase(
            email: payload.User.Email,
            password: payload.User.Password);
        var content = new StringContent(JsonConvert.SerializeObject(payload),
            Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users/login", content);
        var responseString = await response.Content.ReadAsStringAsync();
        var userViewModel = responseString.SubDoc<UserViewModel>("user");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(userViewModel.Email, Is.EqualTo(payload.User.Email));
        Assert.That(userViewModel.Username, Is.EqualTo(userInDatabase.Username));
    }

    [TearDown]
    public async Task Cleanup()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }
}
