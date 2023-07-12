using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Moq;

namespace Conduit.Tests.Unit.Users.Handlers;

[TestFixture]
public class UpdateUserHandlerTests
{
    private UpdateUserHandler _updateUserHandler;
    private Mock<IAuthService> _authServiceMock;
    private Mock<IUserDataService> _userDataServiceMock;

    [SetUp]
    public async Task Setup()
    {
        _authServiceMock = new Mock<IAuthService>();
        _userDataServiceMock = new Mock<IUserDataService>();

        _updateUserHandler =
            new UpdateUserHandler(
                new UpdateUserRequestValidator(_userDataServiceMock.Object,
                    new SharedUserValidator<UpdateUserViewModelUser>()),
                _authServiceMock.Object,
                _userDataServiceMock.Object);
    }

    [Test]
    public async Task Username_can_only_be_changed_if_new_name_is_not_already_taken()
    {
        // arrange
        var submit = new UpdateUserSubmitModel
        {
            User = new UpdateUserViewModelUser
            {
                Email = "valid.email@example.net",
                Password = "Valid00Password!",
                Username = "SomeoneElseAlreadyHasThisUsername",
                Image = "https://example.net/valid.jpg",
                Bio = "Valid bio lorem ipsum"
            }
        };
        var request = new UpdateUserRequest(submit);

        // arrange mocks for database
        // so username is already taken
        _userDataServiceMock.Setup(m => m.DoesExistUserByEmailAndUsername(submit.User.Email, submit.User.Username))
            .ReturnsAsync(true);

        // act
        var result = await _updateUserHandler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors.First().ErrorMessage, Is.EqualTo("That email is already in use."));
    }

    [TestCase("", "Username is required.")]
    [TestCase(null, "Username is required.")]
    [TestCase("really_long_username_really_long_username_really_long_username_really_long_username_really_long_username_really_long_username", "Username must be at most 100 characters long.")]
    public async Task Valid_username_must_be_specified(string username, string message)
    {
        // arrange
        var submit = new UpdateUserSubmitModel
        {
            User = new UpdateUserViewModelUser
            {
                Email = "valid@example.net",
                Password = "Valid00Password!",
                Username = username,
                Image = "https://example.net/valid.jpg",
                Bio = "Valid bio lorem ipsum"
            }
        };
        var request = new UpdateUserRequest(submit);

        // arrange username not taken
        _userDataServiceMock.Setup(m => m.DoesExistUserByEmailAndUsername(submit.User.Email, submit.User.Username))
            .ReturnsAsync(false);

        // act
        var result = await _updateUserHandler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors.First().ErrorMessage, Is.EqualTo(message));
    }

    [TestCase("", "", "", "", "You must specify a value for at least one of: username, password, image, bio.")]
    [TestCase("", null, "", "", "You must specify a value for at least one of: username, password, image, bio.")]
    [TestCase("", "", null, "", "You must specify a value for at least one of: username, password, image, bio.")]
    [TestCase("", "", "", null, "You must specify a value for at least one of: username, password, image, bio.")]
    [TestCase(null, null, null, null, "You must specify a value for at least one of: username, password, image, bio.")]
    public async Task At_least_one_field_must_be_specified_to_update(string email, string password, string image,
        string bio, string errorMessage)
    {
        // arrange
        var submit = new UpdateUserSubmitModel
        {
            User = new UpdateUserViewModelUser
            {
                Email = email,
                Password = password,
                Username = "valid_username",
                Image = image,
                Bio = bio
            }
        };
        var request = new UpdateUserRequest(submit);

        // arrange username not taken
        _userDataServiceMock.Setup(m => m.DoesExistUserByEmailAndUsername(submit.User.Email, submit.User.Username))
            .ReturnsAsync(false);

        // act
        var result = await _updateUserHandler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors.First().ErrorMessage,
            Is.EqualTo("You must specify a value for at least one of: email, password, image, bio."));
    }

    [Test]
    public async Task Bio_must_not_be_longer_than_500_chars()
    {
        // arrange
        var submit = new UpdateUserSubmitModel
        {
            User = new UpdateUserViewModelUser
            {
                Email = "valid_email@example.net",
                Password = "Valid!!Password00",
                Username = "valid_username",
                Image = "https://example.net/valid.jpg",
                Bio = new string('a', 501)
            }
        };
        var request = new UpdateUserRequest(submit);

        // arrange username is NOT already taken
        _userDataServiceMock.Setup(m => m.DoesExistUserByEmailAndUsername(submit.User.Email, submit.User.Username))
            .ReturnsAsync(false);
        // ClusterMock.Setup(x => x.QueryAsync<int>(It.IsAny<string>(), It.IsAny<QueryOptions>()))
        //     .ReturnsAsync(new FakeQueryResult<int>(new List<int> { 0 }));

        // act
        var result = await _updateUserHandler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors.First().ErrorMessage,
            Is.EqualTo("Bio is limited to 500 characters."));
    }

    [TestCase("invalid_url", "Image URL must be valid.")]
    [TestCase("test@example.com", "Image URL must be valid.")]
    [TestCase("http://yahoo.com/foo/bar/baz", "Image URL must be JPG, JPEG, or PNG.")]
    [TestCase("http://yahoo.com/foo/bar/baz.exe", "Image URL must be JPG, JPEG, or PNG.")]
    public async Task Image_url_must_be_a_valid_JPG_PNG_JPEG_url(string url, string message)
    {
        // arrange
        var submit = new UpdateUserSubmitModel
        {
            User = new UpdateUserViewModelUser
            {
                Email = "valid_email@example.net",
                Password = "Valid!!Password00",
                Username = "valid_username",
                Image = url,
                Bio = new string('a', 499)
            }
        };
        var request = new UpdateUserRequest(submit);

        // arrange username not taken
        _userDataServiceMock.Setup(m => m.DoesExistUserByEmailAndUsername(submit.User.Email, submit.User.Username))
            .ReturnsAsync(false);
        // ClusterMock.Setup(x => x.QueryAsync<int>(It.IsAny<string>(), It.IsAny<QueryOptions>()))
        //     .ReturnsAsync(new FakeQueryResult<int>(new List<int> { 0 }));

        // act
        var result = await _updateUserHandler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors.First().ErrorMessage, Is.EqualTo(message));
    }
}