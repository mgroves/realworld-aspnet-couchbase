using System.Text;
using Conduit.Tests.TestHelpers.Data;
using System.Net;
using Conduit.Tests.TestHelpers;
using Conduit.Web.Users.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Conduit.Web.DataAccess.Providers;
using Conduit.Tests.TestHelpers.Dto.ViewModels;

namespace Conduit.Tests.Functional.Users.Controllers.UserController;

public class LoginTests : FunctionalTestBase
{
    private IConduitUsersCollectionProvider _usersCollectionProvider;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();

        // setup database objects for arranging
        var service = WebAppFactory.Services;
        _usersCollectionProvider = service.GetRequiredService<IConduitUsersCollectionProvider>();
    }

    [TestCase("", "")]
    [TestCase(null, "")]
    [TestCase("", null)]
    [TestCase(null, null)]
    public async Task LoginWithNullOrEmptyBothCredentials_Should_Fail(string? email, string? password)
    {
        // Arrange
        var payload = LoginSubmitModelHelper.Create(
            email: email,
            password: password,
            makeEmailNull: email == null,
            makePasswordNull: password == null);

        // Act
        var response = await WebClient.PostAsync("/api/users/login", payload.ToJsonPayload());

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

        // Act
        var response = await WebClient.PostAsync("/api/users/login", payload.ToJsonPayload());

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


        // Act
        var response = await WebClient.PostAsync("/api/users/login", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();
        var userViewModel = responseString.SubDoc<UserViewModel>("user");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(userViewModel.Email, Is.EqualTo(payload.User.Email));
        Assert.That(userViewModel.Username, Is.EqualTo(userInDatabase.Username));
    }
}
