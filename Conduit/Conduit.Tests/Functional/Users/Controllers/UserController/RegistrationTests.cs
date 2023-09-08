using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Users.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Conduit.Tests.TestHelpers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Tests.TestHelpers.Dto.ViewModels;

namespace Conduit.Tests.Functional.Users.Controllers.UserController;

public class RegistrationTests : FunctionalTestBase
{
    private IConduitUsersCollectionProvider _userCollectionProvider;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();

        // setup database objects for arranging
        var service = WebAppFactory.Services;
        _userCollectionProvider = service.GetService<IConduitUsersCollectionProvider>();
    }

    [Test]
    public async Task valid_registration_returns_ok_with_correct_values()
    {
        // Arrange
        var payload = RegistrationSubmitModelHelper.Create();

        // Act
        var response = await WebClient.PostAsync("/api/users", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();
        var userViewModel = responseString.SubDoc<UserViewModel>("user");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(userViewModel.Username, Is.EqualTo(payload.User.Username));
        Assert.That(userViewModel.Email, Is.EqualTo(payload.User.Email));
        Assert.That(userViewModel.Token, Is.Not.Null);  // token should be generated
        Assert.That(userViewModel.Image, Is.EqualTo(null)); // image not created upon registration
        Assert.That(userViewModel.Bio, Is.EqualTo(null));   // bio not created upon registration
    }

    [Test]
    public async Task Registration_returns_forbidden_if_username_already_exists()
    {
        // Arrange
        var existingUser = await _userCollectionProvider.CreateUserInDatabase();
        var payload = RegistrationSubmitModelHelper.Create(username: existingUser.Username);

        // Act
        var response = await WebClient.PostAsync("/api/users", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(responseString, Contains.Substring("User already exists."));
    }

    [TestCase(null, "Username is required.")]
    [TestCase("", "Username is required.")]
    [TestCase("username_too_long_username_too_long_username_too_long_username_too_long_username_too_long_username_too_long", "Username must be at most 100 characters long")]
    public async Task registration_returns_unprocessable_entity_if_username_is_not_valid(string? username, string errorMessage)
    {
        // Arrange
        var payload = RegistrationSubmitModelHelper.Create(
            username: username,
            makeUsernameNull: username == null);

        // Act
        var response = await WebClient.PostAsync("/api/users", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        Assert.That(responseString, Contains.Substring(errorMessage));
    }

    [TestCase(null, "Email address must not be empty")]
    [TestCase("", "Email address must not be empty")]
    [TestCase("email@notvalid", "Email address must be valid")]
    [TestCase("this_isnt_email", "Email address must be valid")]
    [TestCase("email_too_long_email_too_long_email_too_long_email_too_long_email_too_long_email_too_long_email_too_long_email_too_long_email_too_long_email_too_long_email_too_long_email_too_long_email_too_long_email_too_long_email_too_long_email_too_long_email_too_long_email_too_long_@example.net", "Email address must be valid")]
    public async Task registration_returns_unprocessable_entity_if_email_is_not_valid(string? email, string errorMessage)
    {
        // Arrange
        var payload = RegistrationSubmitModelHelper.Create(
            email: email,
            makeEmailNull: email == null);

        // Act
        var response = await WebClient.PostAsync("/api/users", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        Assert.That(responseString, Contains.Substring(errorMessage));
    }

    [TestCase(null, "Password must not be empty")]
    [TestCase("", "Password must not be empty")]
    [TestCase("password", "Password must be at least 10 characters long")]
    [TestCase("passwordpassword", "Password must contain at least one digit")]
    [TestCase("passwordpassword1", "Password must contain at least one uppercase letter")]
    [TestCase("passwordpassword!", "Password must contain at least one digit")]
    [TestCase("a123456789012345", "Password must contain at least one uppercase letter")]
    [TestCase("PasswordPassword", "Password must contain at least one digit")]
    [TestCase("PasswordPassword1", "Password must contain at least one symbol")]
    [TestCase("PasswordPassword!", "Password must contain at least one digit")]
    public async Task registration_returns_unprocessable_entity_if_password_is_not_valid(string? password, string errorMessage)
    {
        // Arrange
        var payload = RegistrationSubmitModelHelper.Create(
            password: password,
            makePasswordNull: password == null);

        // Act
        var response = await WebClient.PostAsync("/api/users", payload.ToJsonPayload());
        var responseString = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        Assert.That(responseString, Contains.Substring(errorMessage));
    }
}