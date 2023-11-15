using Conduit.Tests.TestHelpers;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.Follows.Services;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace Conduit.Tests.Unit.Users.Handlers;

[TestFixture]
public class GetProfileHandlerTests
{
    private Mock<IUserDataService> _userDataServiceMock;
    private Mock<IFollowDataService> _followDataServiceMock;
    private GetProfileHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _userDataServiceMock = new Mock<IUserDataService>();
        _followDataServiceMock = new Mock<IFollowDataService>();

        // setup handler
        var jwtSecrets = new JwtSecrets
        {
            Audience = "doesntmatter-audience",
            Issuer = "doesntmatter-issuer",
            SecurityKey = "doesntmatter-securityKey-doesntmatter-securityKey-doesntmatter-securityKey-doesntmatter-securityKey"
        };
        _handler = new GetProfileHandler(_userDataServiceMock.Object,
            new GetProfileRequestValidator(), 
            _followDataServiceMock.Object,
            new AuthService(new OptionsWrapper<JwtSecrets>(jwtSecrets)));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Profile_for_a_user_following_the_user_in_the_profile(bool isUserFollowing)
    {
        // arrange
        var currentUserToken = AuthServiceHelper.Create().GenerateJwtToken("matt.groves@example.net", "MattGroves");

        // arrange for user to NOT be followed
        var user = UserHelper.CreateUser(username: "SurlyDev");
        _followDataServiceMock.Setup(m => m.IsCurrentUserFollowing("MattGroves", user.Username))
            .ReturnsAsync(isUserFollowing);
        _userDataServiceMock.Setup(m => m.GetProfileByUsername(user.Username))
            .ReturnsAsync(new DataServiceResult<User>(user, DataResultStatus.Ok));
        var request = new GetProfileRequest(user.Username, currentUserToken);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result.ProfileView.Following, Is.EqualTo(isUserFollowing));
    }

    [Test]
    public async Task Username_not_found()
    {
        // arrange
        // arrange request
        var username = "napalm684doesntexist";
        var request = new GetProfileRequest(username, null);

        // arrange for user to be in mock database
        _userDataServiceMock.Setup(m => m.GetProfileByUsername(username))
            .ReturnsAsync(new DataServiceResult<User>(null, DataResultStatus.NotFound));

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ProfileView, Is.Null);
        Assert.That(result.UserNotFound, Is.True);
    }

    [Test]
    public async Task Valid_username_means_user_profile_is_returned()
    {
        // arrange
        // arrange request
        var username = "napalm684";
        var request = new GetProfileRequest(username, null);

        // arrange for user to be in mock database
        var user = UserHelper.CreateUser(username: username);
        _userDataServiceMock.Setup(m => m.GetProfileByUsername(username))
            .ReturnsAsync(new DataServiceResult<User>(user, DataResultStatus.Ok));

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ProfileView, Is.Not.Null);
        Assert.That(result.ProfileView.Username, Is.EqualTo(username));
        Assert.That(result.ProfileView.Image, Is.EqualTo(user.Image));
        Assert.That(result.ProfileView.Bio, Is.EqualTo(user.Bio));
        Assert.That(result.ProfileView.Following, Is.EqualTo(false));
    }
}