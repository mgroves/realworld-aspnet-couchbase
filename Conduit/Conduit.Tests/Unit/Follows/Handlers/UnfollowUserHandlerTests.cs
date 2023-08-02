using Conduit.Web.Follows.Handlers;
using Conduit.Web.Follows.Services;
using Conduit.Web.Models;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Moq;

namespace Conduit.Tests.Unit.Follows.Handlers;

[TestFixture]
public class UnfollowUserHandlerTests
{
    private Mock<IUserDataService> _userDataServiceMock;
    private Mock<IFollowDataService> _followDataServiceMock;
    private UnfollowUserHandler _handler;

    [SetUp]
    public async Task Setup()
    {
        _userDataServiceMock = new Mock<IUserDataService>();
        _followDataServiceMock = new Mock<IFollowDataService>();

        _handler = new UnfollowUserHandler(_userDataServiceMock.Object, _followDataServiceMock.Object, new UnfollowUserRequestValidator());
    }

    [Test]
    public async Task Handle_ShouldInvokeUnfollowService_WhenUserExists()
    {
        // arrange
        var request = new UnfollowUserRequest("userToUnfollow", "userDoingTheUnfollowing");

        // arrange what profile is expected to be returned
        var expectedProfile = new ProfileViewModel
        {
            Username = "userToUnfollow",
            Bio = "Lorem ipsum",
            Image = "http://example.net/image.jpg",
            Following = false
        };

        // arrange the user that will be in the database
        var userToUnfollow = new User
        {
            Username = expectedProfile.Username,
            Bio = expectedProfile.Bio,
            Image = expectedProfile.Image
        };

        _userDataServiceMock.Setup(svc => svc.GetUserByUsername(It.IsAny<string>()))
            .ReturnsAsync(new DataServiceResult<User>(userToUnfollow, DataResultStatus.Ok));
        _followDataServiceMock.Setup(svc => svc.UnfollowUser(request.UserToUnfollow, request.UserToUnfollow))
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
        var request = new UnfollowUserRequest(invalidUsername, "doesntmatter");
        // TODO: why was this here?
        // var expectedValidationErrors = new[]
        // {
        //     new FluentValidation.Results.ValidationFailure("UnfollowingUsername", "Username must not be empty."),
        //     new FluentValidation.Results.ValidationFailure("UserToUnfollow", "Username must not be empty.")
        // };

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors[0].ErrorMessage, Is.EqualTo("Username is required."));
    }

    [Test]
    public async Task Handle_ShouldReturnUserNotFound_WhenUserToUnfollowDoesNotExist()
    {
        // arrange
        var request = new UnfollowUserRequest("userThatDoesntExist", "userDoingTheUnfollowing");

        _userDataServiceMock.Setup(svc => svc.GetUserByUsername(It.IsAny<string>()))
            .ReturnsAsync(new DataServiceResult<User>(null, DataResultStatus.NotFound));

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserNotFound, Is.True);
    }
}
