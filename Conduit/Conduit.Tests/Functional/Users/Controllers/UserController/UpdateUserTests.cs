using Conduit.Web.DataAccess.Providers;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.DataAccess.Models;
using System.Net;
using Conduit.Tests.TestHelpers;
using Conduit.Web.Users.ViewModels;
using Conduit.Tests.TestHelpers.Dto.ViewModels;

namespace Conduit.Tests.Functional.Users.Controllers.UserController;

public class UpdateUserTests : FunctionalTestBase
{
    private IConduitUsersCollectionProvider _usersCollectionProvider;
    private User _user;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();

        // setup database objects for arranging
        var service = WebAppFactory.Services;
        _usersCollectionProvider = service.GetRequiredService<IConduitUsersCollectionProvider>();

        // setup an authorized header / existing user to update
        _user = await _usersCollectionProvider.CreateUserInDatabase();
        var jwtToken = AuthSvc.GenerateJwtToken(_user.Email, _user.Username);
        WebClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", jwtToken);
    }

    [Test]
    public async Task Update_user_email_when_valid_input_is_provided()
    {
        // arrange
        var payload = UpdateUserSubmitModelHelper.Create(username: _user.Username);

        // Act
        var response = await WebClient.PutAsync("api/user", payload.ToJsonPayload());

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await _usersCollectionProvider.AssertExists(_user.Username, x =>
        {
            Assert.That(x.Bio, Is.EqualTo(payload.User.Bio));
            Assert.That(x.Email, Is.EqualTo(payload.User.Email));
            Assert.That(x.Image, Is.EqualTo(payload.User.Image));
            Assert.That(AuthSvc.DoesPasswordMatch(payload.User.Password, x.Password, x.PasswordSalt));
        });
    }

    [Test]
    public async Task Update_only_email_if_only_email_is_given()
    {
        // arrange
        var payload = UpdateUserSubmitModelHelper.Create(username: _user.Username);
        payload.User.Bio = null;
        payload.User.Image = null;
        payload.User.Password = null;

        // Act
        var response = await WebClient.PutAsync("api/user", payload.ToJsonPayload());

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await _usersCollectionProvider.AssertExists(_user.Username, x =>
        {
            // email should have changed
            Assert.That(x.Email, Is.EqualTo(payload.User.Email));

            // others remain the same
            Assert.That(x.Bio, Is.EqualTo(_user.Bio));
            Assert.That(x.Image, Is.EqualTo(_user.Image));
            Assert.That(x.Password, Is.EqualTo(_user.Password));
        });
    }

    [Test]
    public async Task Update_only_bio_if_only_bio_is_given()
    {
        // arrange
        var payload = UpdateUserSubmitModelHelper.Create(username: _user.Username);
        payload.User.Image = null;
        payload.User.Password = null;
        payload.User.Email = null;

        // Act
        var response = await WebClient.PutAsync("api/user", payload.ToJsonPayload());

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await _usersCollectionProvider.AssertExists(_user.Username, x =>
        {
            // email should have changed
            Assert.That(x.Bio, Is.EqualTo(payload.User.Bio));

            // others remain the same
            Assert.That(x.Email, Is.EqualTo(_user.Email));
            Assert.That(x.Image, Is.EqualTo(_user.Image));
            Assert.That(x.Password, Is.EqualTo(_user.Password));
        });
    }

    [Test]
    public async Task return_NotFound_when_user_to_update_is_not_found()
    {
        // arrange
        // another weird edge case, client has token but user has been deleted
        var payload = UpdateUserSubmitModelHelper.Create(username: _user.Username);
        await _usersCollectionProvider.DeleteUserFromDatabase(_user);

        // act
        var response = await WebClient.PutAsync("api/user", payload.ToJsonPayload());

        // assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task return_UnprocessableEntity_when_request_body_is_null()
    {

    }

    [Test]
    public async Task return_UnprocessableEntity_when_username_is_not_provided()
    {

    }

    [Test]
    public async Task return_UnprocessableEntity_when_email_is_not_provided()
    {

    }

    [Test]
    public async Task return_UnprocessableEntity_when_username_is_not_unique()
    {

    }

    [Test]
    public async Task return_UnprocessableEntity_when_email_is_not_unique()
    {

    }

    [Test]
    public async Task return_UnprocessableEntity_when_a_password_is_not_provided()
    {

    }

    [Test]
    public async Task return_UnprocessableEntity_when_a_password_is_too_short()
    {

    }

    [Test]
    public async Task return_UnprocessableEntity_when_a_password_is_not_complex_enough()
    {

    }

    [Test]
    public async Task return_ValidationErrors_when_given_non_alphabetic_characters_in_username()
    {

    }

    [Test]
    public async Task return_ValidationErrors_when_given_non_alphanumeric_characters_in_password()
    {

    }

}