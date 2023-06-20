using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Moq;
using Couchbase.Query;
using Conduit.Tests.Fakes.Couchbase;

namespace Conduit.Tests.Unit.Auth.Handlers;

[TestFixture]
public class UpdateUserHandlerTests : WithCouchbaseMocks
{
    private UpdateUserHandler _updateUserHandler;
    private Mock<IAuthService> _authServiceMock;

    [SetUp]
    public async Task Setup()
    {
        base.SetUp();

        _authServiceMock = new Mock<IAuthService>();

        _updateUserHandler =
            new UpdateUserHandler(new UpdateUserRequestValidator(UsersCollectionProviderMock.Object, new SharedUserValidator<UpdateUserViewModelUser>()), UsersCollectionProviderMock.Object, _authServiceMock.Object);

        // by default, username is already taken
        ClusterMock.Setup(x => x.QueryAsync<int>(It.IsAny<string>(), It.IsAny<QueryOptions>()))
            .ReturnsAsync(new FakeQueryResult<int>(new List<int> { 1 }));
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
        ClusterMock.Setup(m => m.QueryAsync<int>(It.IsAny<string>(), It.IsAny<QueryOptions>()))
            .ReturnsAsync(new FakeQueryResult<int>(new List<int> { 1 }));

        // act
        var result = await _updateUserHandler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors.First().ErrorMessage, Is.EqualTo("That username is already taken."));
    }

    [TestCase("", "Email address must not be empty.")]
    [TestCase(null, "Email address must not be empty.")]
    [TestCase("invalid_email_address", "Email address must be valid.")]
    public async Task Valid_email_must_be_specified(string givenEmailAddress, string message)
    {
        // arrange
        var submit = new UpdateUserSubmitModel
        {
            User = new UpdateUserViewModelUser
            {
                Email = null,
                Password = "Valid00Password!",
                Username = "valid_username",
                Image = "https://example.net/valid.jpg",
                Bio = "Valid bio lorem ipsum"
            }
        };
        var request = new UpdateUserRequest(submit);

        // act
        var result = await _updateUserHandler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors.First().ErrorMessage, Is.EqualTo("Email address must not be empty."));
    }

    [TestCase("", "", "", "", "You must specify a value for at least one of: username, password, image, bio.")]
    [TestCase("", null, "", "", "You must specify a value for at least one of: username, password, image, bio.")]
    [TestCase("", "", null, "", "You must specify a value for at least one of: username, password, image, bio.")]
    [TestCase("", "", "", null, "You must specify a value for at least one of: username, password, image, bio.")]
    [TestCase(null, null, null, null, "You must specify a value for at least one of: username, password, image, bio.")]
    public async Task At_least_one_field_must_be_specified_to_update(string username, string password, string image,
        string bio, string errorMessage)
    {
        // arrange
        var submit = new UpdateUserSubmitModel
        {
            User = new UpdateUserViewModelUser
            {
                Email = "valid_email@example.net",
                Password = password,
                Username = username,
                Image = image,
                Bio = bio
            }
        };
        var request = new UpdateUserRequest(submit);

        // act
        var result = await _updateUserHandler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors.First().ErrorMessage,
            Is.EqualTo("You must specify a value for at least one of: username, password, image, bio."));
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

        // act
        var result = await _updateUserHandler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors.First().ErrorMessage, Is.EqualTo(message));
    }
}