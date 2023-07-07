using Conduit.Web.Models;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Moq;

namespace Conduit.Tests.Unit.Auth.Handlers;

public class Tests
{
    [TestFixture]
    public class LoginRequestHandlerTests
    {
        private Mock<IAuthService> _authServiceMock;
        private LoginRequestHandler _loginHandler;
        private Mock<IUserDataService> _userDataServiceMock;

        [SetUp]
        public void SetUp()
        {
            _authServiceMock = new Mock<IAuthService>();
            _userDataServiceMock = new Mock<IUserDataService>();

            _loginHandler = new LoginRequestHandler(_authServiceMock.Object, new LoginRequestValidator(), _userDataServiceMock.Object);
        }

        [Test]
        public async Task Handle_ValidCredentials_ReturnsAuthorizedLoginResult()
        {
            // Arrange
            // setup a user that will be returned from database
            var userInDatabase = new User
            {
                Email = "test@test.com",
                Password = "hashedPassword",
                PasswordSalt = "salt",
                Username = "testUser",
                Bio = "testBio",
                Image = "testImage"
            };            
            
            // setup the login information submitted through the API
            var loginSubmitModel = new LoginSubmitModel
            {
                User = new LoginUserViewModel
                {
                    Email = userInDatabase.Email,
                    Password = userInDatabase.Password
                }
            };
            var request = new LoginRequest(loginSubmitModel);

            // setup database and auth service mocks
            _userDataServiceMock.Setup(m => m.GetUserByEmail(request.Model.User.Email))
                .ReturnsAsync(new DataServiceResult<User>(userInDatabase, DataResultStatus.Ok));
            _authServiceMock.Setup(a => a.DoesPasswordMatch(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            _authServiceMock.Setup(a => a.GenerateJwtToken(userInDatabase.Email))
                .Returns("token");

            // Act
            var result = await _loginHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsUnauthorized, Is.False);
            Assert.That(result.UserView, Is.Not.Null);
            Assert.That(result.UserView.Email, Is.EqualTo(userInDatabase.Email));
            Assert.That(result.UserView.Token, Is.EqualTo("token"));
            Assert.That(result.UserView.Username, Is.EqualTo(userInDatabase.Username));
            Assert.That(result.UserView.Bio, Is.EqualTo(userInDatabase.Bio));
            Assert.That(result.UserView.Image, Is.EqualTo(userInDatabase.Image));
        }

        [Test]
        public async Task Handle_UserDoesntExist_ReturnsUnAuthorized()
        {
            // arrange
            // setup the login information submitted through the API
            var loginSubmitModel = new LoginSubmitModel
            {
                User = new LoginUserViewModel
                {
                    Email = "doesntexist@balloons.gov",
                    Password = "doesntexist"
                }
            };
            var request = new LoginRequest(loginSubmitModel);

            // setup database and auth service mocks
            _userDataServiceMock.Setup(m => m.GetUserByEmail(request.Model.User.Email))
                .ReturnsAsync(new DataServiceResult<User>(null, DataResultStatus.NotFound));
            
            // act
            var result = await _loginHandler.Handle(request, CancellationToken.None);
            
            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsUnauthorized, Is.True);
        }
        
        [Test]
        public async Task Handle_InvalidCredentials_ReturnsUnAuthorized()
        {
            // arrange
            // setup a user that will be returned from database
            var userInDatabase = new User
            {
                Email = "someuser@balloons.gov",
                Password = "hashedPassword",
                PasswordSalt = "salt",
                Username = "testUser",
                Bio = "testBio",
                Image = "testImage"
            };                     
            
            // setup the login information submitted through the API
            var loginSubmitModel = new LoginSubmitModel
            {
                User = new LoginUserViewModel
                {
                    Email = userInDatabase.Email,
                    Password = userInDatabase.Password + "doesntmatch"
                }
            };
            var request = new LoginRequest(loginSubmitModel);

            // setup database and auth service mocks
            _userDataServiceMock.Setup(m => m.GetUserByEmail(request.Model.User.Email))
                .ReturnsAsync(new DataServiceResult<User>(userInDatabase, DataResultStatus.Ok));
            _authServiceMock.Setup(a => a.DoesPasswordMatch(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);
            
            // act
            var result = await _loginHandler.Handle(request, CancellationToken.None);
            
            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsUnauthorized, Is.True);
        }

        [TestCase("", "Email address must not be empty.")]
        [TestCase(null, "Email address must not be empty.")]
        public async Task Handle_EmailEmpty_ReturnsErrorMessages(string email, string message)
        {
            // arrange
            // setup the login information submitted through the API
            var loginSubmitModel = new LoginSubmitModel
            {
                User = new LoginUserViewModel
                {
                    Email = email,
                    Password = "doesntmatter"
                }
            };
            var request = new LoginRequest(loginSubmitModel);

            // act
            var result = await _loginHandler.Handle(request, CancellationToken.None);

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ValidationErrors, Is.Not.Null);
            Assert.That(result.ValidationErrors.Any(), Is.True);
            Assert.That(result.ValidationErrors.First().ErrorMessage, Is.EqualTo(message));
        }

        [TestCase("", "Password must not be empty.")]
        [TestCase(null, "Password must not be empty.")]
        public async Task Handle_PasswordEmpty_ReturnsErrorMessages(string password, string message)
        {
            // arrange
            // setup the login information submitted through the API
            var loginSubmitModel = new LoginSubmitModel
            {
                User = new LoginUserViewModel
                {
                    Email = "validemail@balloons.gov",
                    Password = password
                }
            };
            var request = new LoginRequest(loginSubmitModel);

            // act
            var result = await _loginHandler.Handle(request, CancellationToken.None);

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ValidationErrors, Is.Not.Null);
            Assert.That(result.ValidationErrors.Any(), Is.True);
            Assert.That(result.ValidationErrors.First().ErrorMessage, Is.EqualTo(message));
        }
    }
}