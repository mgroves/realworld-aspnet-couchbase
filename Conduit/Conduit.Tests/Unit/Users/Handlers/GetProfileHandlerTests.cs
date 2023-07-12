using Conduit.Web.Models;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Moq;

namespace Conduit.Tests.Unit.Users.Handlers;

[TestFixture]
public class GetProfileHandlerTests
{
    private Mock<IUserDataService> _userDataServiceMock;

    [SetUp]
    public void SetUp()
    {
        _userDataServiceMock = new Mock<IUserDataService>();
    }

    // TODO: test when the user IS following

    // TODO: test when the user IS NOT following

    [Test]
    public async Task Username_not_found()
    {
        // arrange
        // arrange request
        var username = "napalm684doesntexist";
        var request = new GetProfileRequest(username, null);

        // arrange handler
        var handler = new GetProfileHandler(_userDataServiceMock.Object, new GetProfileRequestValidator());

        // arrange for user to be in mock database
        _userDataServiceMock.Setup(m => m.GetProfileByUsername(username))
            .ReturnsAsync(new DataServiceResult<User>(null, DataResultStatus.NotFound));

        // act
        var result = await handler.Handle(request, CancellationToken.None);

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

        // arrange handler
        var handler = new GetProfileHandler(_userDataServiceMock.Object, new GetProfileRequestValidator());

        // arrange for user to be in mock database
        var user = new User
        {
            Username = username,
            Bio = "doesntmatter",
            Email = "doesntmatter@example.net",
            Image = "https://doesntmatter.example.net/whatever.jpg",
            Password = "doesntmatter",
            PasswordSalt = "doesntmatter"
        };
        _userDataServiceMock.Setup(m => m.GetProfileByUsername(username))
            .ReturnsAsync(new DataServiceResult<User>(user, DataResultStatus.Ok));

        // act
        var result = await handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ProfileView, Is.Not.Null);
        Assert.That(result.ProfileView.Username, Is.EqualTo(username));
        Assert.That(result.ProfileView.Image, Is.EqualTo(user.Image));
        Assert.That(result.ProfileView.Bio, Is.EqualTo(user.Bio));
        Assert.That(result.ProfileView.Following, Is.EqualTo(false));
    }
}