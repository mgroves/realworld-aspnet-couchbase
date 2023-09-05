using Conduit.Tests.TestHelpers.Dto;
using Conduit.Web.DataAccess.Providers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Users.Services;
using Microsoft.Extensions.Options;
using Conduit.Tests.TestHelpers;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.Users.ViewModels;

namespace Conduit.Tests.Integration.Users.Controllers.UserController;

[TestFixture]
public class GetCurrentUserTests : WebIntegrationTest
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

        // setup an authorized header
        _user = await _usersCollectionProvider.CreateUserInDatabase();
        var jwtToken = AuthSvc.GenerateJwtToken(_user.Email, _user.Username);
        WebClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", jwtToken);
    }

    [TestCase("")]
    [TestCase(null)]
    public async Task Unauthenticated_user_returns_401(string authorizationHeader)
    {
        // Arrange
        WebClient.DefaultRequestHeaders.Clear();
        WebClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Authorization", authorizationHeader);

        // Act
        var response = await WebClient.GetAsync( "/api/user");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Authenticated_get_current_user_returns_user()
    {
        // Arrange
        // no arranging needed, Setup puts auth token in header

        // Act
        var response = await WebClient.GetAsync("/api/user");
        var responseString = await response.Content.ReadAsStringAsync();
        var userViewModel = responseString.SubDoc<UserViewModel>("user");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(userViewModel.Email, Is.EqualTo(_user.Email));
        Assert.That(userViewModel.Username, Is.EqualTo(_user.Username));
        Assert.That(userViewModel.Bio, Is.EqualTo(_user.Bio));
    }

    [Test]
    public async Task Get_current_user_returns_unprocessable_entity_when_user_not_found()
    {
        // Arrange - this would be weird, a client has a token for a user that doesn't exist anymore
        // and there's nothing in the spec about deleting users
        // still, testing it for the sake of completeness
        await _usersCollectionProvider.DeleteUserFromDatabase(_user);

        // Act
        var response = await WebClient.GetAsync("/api/user");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Get_current_user_returns_unprocessable_entity_when_jwt_not_valid()
    {
        // I'm not sure this is possible?
        // It would be some very weird edge case that I can't figure out how to reproduce
    }
}