using Conduit.Tests.Fakes.Couchbase;
using Conduit.Web.Auth.Handlers;
using Conduit.Web.Auth.Services;
using Conduit.Web.Models;
using Couchbase.Core.Exceptions.KeyValue;
using Moq;

namespace Conduit.Tests.Unit.Auth.Handlers;

public class GetUserRequestHandlerTest : WithCouchbaseMocks
{
    [SetUp]
    public override void SetUp()
    {
        base.SetUp();

        _mockAuthService = new Mock<IAuthService>();
    }

    private Mock<IAuthService> _mockAuthService { get; set; }

    [Test]
    public async Task ValidToken_is_Handled()
    {
        // arrange
        // arrange request from API
        var email = "myfakeemail@example.net";
        var fakeToken = new AuthService().GenerateJwtToken(email);
        var request = new GetUserRequest(fakeToken);

        // arrange user that would be in db
        var userFromDatabase = new User
        {
            Email = email,
            Username = "doesntmatter",
            Bio = "lorem ipsum",
            Image = "https://example.com/profile.jpg",
            Password = "doesntmatterPassword",
            PasswordSalt = "doesntmatterSalt"
        };

        // arrange handler
        var handler = new GetUserRequestHandler(UsersCollectionProviderMock.Object, _mockAuthService.Object);

        // arrange mocks
        _mockAuthService.Setup(m => m.GenerateJwtToken(It.IsAny<string>()))
            .Returns(fakeToken);
        UsersCollectionMock.Setup(m => m.GetAsync(email, null))
            .ReturnsAsync(new FakeGetResult(userFromDatabase));

        // act
        var result = await handler.Handle(request, CancellationToken.None);

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
        var request = new GetUserRequest(fakeToken);

        // arrange handler
        var handler = new GetUserRequestHandler(UsersCollectionProviderMock.Object, _mockAuthService.Object);

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
        var fakeToken = new AuthService().GenerateJwtToken(email);
        var request = new GetUserRequest(fakeToken);

        // arrange handler
        var handler = new GetUserRequestHandler(UsersCollectionProviderMock.Object, _mockAuthService.Object);

        // arrange mocks
        _mockAuthService.Setup(m => m.GenerateJwtToken(It.IsAny<string>()))
            .Returns(fakeToken);
        UsersCollectionMock.Setup(m => m.GetAsync(email, null))
            .Throws<DocumentExistsException>();

        // act
        var result = await handler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsInvalidToken, Is.True);
    }
}