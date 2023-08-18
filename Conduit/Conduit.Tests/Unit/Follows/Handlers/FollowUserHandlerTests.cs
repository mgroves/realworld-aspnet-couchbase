namespace Conduit.Tests.Unit.Follows.Handlers;

using System.Threading;
using System.Threading.Tasks;
using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.Follows.Handlers;
using Conduit.Web.Follows.Services;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Moq;
using NUnit.Framework;


[TestFixture]
public class FollowUserHandlerTests
{
    private Mock<IUserDataService> _userDataServiceMock;
    private Mock<IFollowDataService> _followDataServiceMock;
    private FollowUserHandler _handler;

    [SetUp]
    public async Task Setup()
    {
        _userDataServiceMock = new Mock<IUserDataService>();
        _followDataServiceMock = new Mock<IFollowDataService>();

        _handler = new FollowUserHandler(_userDataServiceMock.Object, _followDataServiceMock.Object, new FollowUserRequestValidator());
    }

    [Test]
    public async Task Handle_ShouldInvokeFollowService_WhenUserExists()
    {
        // arrange
        var request = new FollowUserRequest("userToFollow", "userDoingTheFollowing");

        // arrange what profile is expected to be returned
        var expectedProfile = new ProfileViewModel
        {
            Username = "userToFollow",
            Bio = "Lorem ipsum",
            Image = "http://example.net/image.jpg",
            Following = true
        };

        // arrange the user that will be in the database
        var userToFollow = new User
        {
            Username = expectedProfile.Username,
            Bio = expectedProfile.Bio,
            Image = expectedProfile.Image
        };

        _userDataServiceMock.Setup(svc => svc.GetUserByUsername(It.IsAny<string>()))
            .ReturnsAsync(new DataServiceResult<User>(userToFollow, DataResultStatus.Ok));
        _followDataServiceMock.Setup(svc => svc.FollowUser(request.UserToFollow, request.FollowingUsername))
            .Returns(Task.CompletedTask);

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Profile.Username, Is.EqualTo(expectedProfile.Username));
        Assert.That(result.Profile.Bio, Is.EqualTo(expectedProfile.Bio));
        Assert.That(result.Profile.Image, Is.EqualTo(expectedProfile.Image));
        Assert.That(result.Profile.Following, Is.EqualTo(expectedProfile.Following));
    }

    [TestCase("")]
    [TestCase(null)]
    public async Task Handle_ShouldFailValidation_WhenRequestIsInvalid(string invalidUsername)
    {
        // arrange
        var request = new FollowUserRequest(invalidUsername, "doesntmatter");

        // TODO: why is this here?
        var expectedValidationErrors = new[]
        {
            new FluentValidation.Results.ValidationFailure("FollowingUsername", "Username must not be empty."),
            new FluentValidation.Results.ValidationFailure("UserToFollow", "Username must not be empty.")
        };

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors[0].ErrorMessage, Is.EqualTo("Username is required."));
    }

    [Test]
    public async Task Handle_ShouldReturnUserNotFound_WhenUserToFollowDoesNotExist()
    {
        // arrange
        var request = new FollowUserRequest("userThatDoesntExist", "userDoingTheFollowing");

        _userDataServiceMock.Setup(svc => svc.GetUserByUsername(It.IsAny<string>()))
            .ReturnsAsync(new DataServiceResult<User>(null, DataResultStatus.NotFound));

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserNotFound, Is.True);
    }

    [Test]
    public async Task Cant_follow_yourself()
    {
        // arrange
        var username = Path.GetRandomFileName();
        var request = new FollowUserRequest(username, username);
        
        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Any(e => e.ErrorMessage == "You can't follow yourself."), Is.True);
    }
}
