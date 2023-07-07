using Conduit.Tests.Fakes.Couchbase;
using Conduit.Web.Models;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Couchbase.Query;
using Moq;

namespace Conduit.Tests.Unit.Auth.Handlers;

[TestFixture]
public class GetProfileHandlerTests : WithCouchbaseMocks
{
    [SetUp]
    public override void SetUp()
    {
        base.SetUp();

        //_mockAuthService = new Mock<IAuthService>();
    }

    [Test]
    public async Task Username_not_found()
    {
        // arrange
        // arrange request
        var username = "napalm684doesntexist";
        var request = new GetProfileRequest(username, null);

        // arrange handler
        var handler = new GetProfileHandler(UsersCollectionProviderMock.Object, new GetProfileRequestValidator());

        // arrange for user to be in mock database
        var noUsersFoundResult = new FakeQueryResult<User>(new List<User> { });
        ClusterMock.Setup(m => m.QueryAsync<User>(It.IsAny<string>(), It.IsAny<QueryOptions>()))
            .ReturnsAsync(noUsersFoundResult);

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
        var handler = new GetProfileHandler(UsersCollectionProviderMock.Object, new GetProfileRequestValidator());

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
        var userInDatabase = new FakeQueryResult<User>(new List<User> { user });
        ClusterMock.Setup(m => m.QueryAsync<User>(It.IsAny<string>(), It.IsAny<QueryOptions>()))
            .ReturnsAsync(userInDatabase);

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