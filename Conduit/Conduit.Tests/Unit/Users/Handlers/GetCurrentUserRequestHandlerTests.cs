using Conduit.Tests.TestHelpers;
using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Moq;

namespace Conduit.Tests.Unit.Users.Handlers;

public class GetCurrentUserRequestHandlerTests
{
    private Mock<IUserDataService> _mockUserDataService;
    private Mock<IAuthService> _mockAuthService;
    private GetCurrentUserHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockUserDataService = new Mock<IUserDataService>();
        _handler = new GetCurrentUserHandler(_mockAuthService.Object, _mockUserDataService.Object);
    }

    [Test]
    public async Task ValidToken_is_Handled()
    {
        // arrange
        // arrange request from API
        var email = "myfakeemail@example.net";
        var username = "doesntmatter";
        var fakeToken = AuthServiceHelper.Create().GenerateJwtToken(email, username);
        var request = new GetCurrentUserRequest(fakeToken);

        // arrange user that would be in db
        var userFromDatabase = new User
        {
            Email = email,
            Username = username,
            Bio = "lorem ipsum",
            Image = "https://example.com/profile.jpg",
            Password = "doesntmatterPassword",
            PasswordSalt = "doesntmatterSalt"
        };

        // arrange mocks
        _mockAuthService.Setup(m => m.GetUsernameClaim(fakeToken))
            .Returns(new AuthService.ClaimResult { IsNotFound = false, Value = username });
        _mockAuthService.Setup(m => m.GenerateJwtToken(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(fakeToken);
        _mockUserDataService.Setup(m => m.GetUserByUsername(username))
            .ReturnsAsync(new DataServiceResult<User>(userFromDatabase, DataResultStatus.Ok));

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserView, Is.Not.Null);
        Assert.That(result.UserView.Email, Is.EqualTo(userFromDatabase.Email));
        Assert.That(result.UserView.Username, Is.EqualTo(userFromDatabase.Username));
        Assert.That(result.UserView.Bio, Is.EqualTo(userFromDatabase.Bio));
        Assert.That(result.UserView.Image, Is.EqualTo(userFromDatabase.Image));
        Assert.That(result.UserView.Token, Is.EqualTo(fakeToken));
    }

    [TestCase("thisisaninvalidtoken")]
    [TestCase("")]
    [TestCase(null)]
    public async Task InvalidToken_is_Handled(string fakeToken)
    {
        // arrange
        // arrange request from API
        var email = "myfakeemail@example.net";
        var request = new GetCurrentUserRequest(fakeToken);

        // arrange handler
        var handler = new GetCurrentUserHandler(_mockAuthService.Object, _mockUserDataService.Object);
        // arrange email claim
        _mockAuthService.Setup(m => m.GetUsernameClaim(It.IsAny<string>()))
            .Returns(new AuthService.ClaimResult { IsNotFound = true});

        // act
        var result = await handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result.IsInvalidToken, Is.True);
    }

    [Test]
    public async Task UserIsMissingFromDatabase_is_Handled()
    {
        // arrange
        // arrange request from API
        var email = "myfakeemail@example.net";
        var fakeToken = AuthServiceHelper.Create().GenerateJwtToken(email, "doesntmatter");
        var request = new GetCurrentUserRequest(fakeToken);


        // arrange mocks
        _mockAuthService.Setup(m => m.GetUsernameClaim(fakeToken))
            .Returns(new AuthService.ClaimResult { Value = email });
        _mockUserDataService.Setup(m => m.GetUserByUsername(email))
            .ReturnsAsync(new DataServiceResult<User>(null, DataResultStatus.NotFound));

        // act
        var result = await _handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserNotFound, Is.True);
    }
}