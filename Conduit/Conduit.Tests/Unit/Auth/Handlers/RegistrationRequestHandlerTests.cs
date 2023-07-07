using Conduit.Web.Models;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Conduit.Web.Users.ViewModels;
using Moq;

namespace Conduit.Tests.Unit.Auth.Handlers;

[TestFixture]
public class RegistrationRequestHandlerTests
{
    private Mock<IAuthService> _authServiceMock;
    private RegistrationRequestHandler _registrationHandler;
    private Mock<IUserDataService> _userDataServiceMock;

    [SetUp]
    public void SetUp()
    {
        _authServiceMock = new Mock<IAuthService>();
        _userDataServiceMock = new Mock<IUserDataService>();

        // by default, user with the requested username does not already exist
        _userDataServiceMock.Setup(m => m.GetUserByUsername(It.IsAny<string>()))
            .ReturnsAsync(new DataServiceResult<User>(null, DataResultStatus.NotFound));

        _registrationHandler = new RegistrationRequestHandler(_authServiceMock.Object, new RegistrationRequestValidator(new SharedUserValidator<RegistrationUserSubmitModel>(), _userDataServiceMock.Object), _userDataServiceMock.Object);
    }

    [Test]
    public async Task Handle_ValidRegistration_ReturnsRegistrationResult()
    {
        // arrange
        // setup registration info submitted through the API
        var submittedInfo = new RegistrationUserSubmitModel
        {
            Username = "doesntmatter",
            Password = "AStrongAndValid!!!Password99",
            Email = "doesntmatter@example.net"
        };
        var request = new RegistrationRequest(new RegistrationSubmitModel
        {
            User = submittedInfo
        });

        // arrange user object expected to be returned by database
        var newUser = new User
        {
            Bio = null,
            Email = submittedInfo.Email,
            Password = "dontcare",
            PasswordSalt = "doesntmatter",
            Username = submittedInfo.Username
        };

        // setup database and auth service mocks
        _authServiceMock.Setup(a => a.GenerateSalt()).Returns("salt");
        _authServiceMock.Setup(a => a.HashPassword(It.IsAny<string>(), It.IsAny<string>())).Returns("hashedPassword");
        _authServiceMock.Setup(a => a.GenerateJwtToken(It.IsAny<string>())).Returns("jwttoken");
        _userDataServiceMock.Setup(m => m.RegisterNewUser(It.IsAny<User>()))
            .ReturnsAsync(new DataServiceResult<User>(newUser, DataResultStatus.Ok));
        
        // act
        var result = await _registrationHandler.Handle(request, CancellationToken.None);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserView, Is.Not.Null);
        Assert.That(result.UserView.Username, Is.EqualTo(submittedInfo.Username));
        Assert.That(result.UserView.Email, Is.EqualTo(submittedInfo.Email));
        Assert.That(result.UserView.Bio, Is.EqualTo(null));
        Assert.That(result.UserView.Image, Is.EqualTo(null));
        Assert.That(result.UserView.Token, Is.EqualTo("jwttoken"));
    }

    [Test]
    public async Task Handle_UserAlreadyExistsInDbByEmail_ReturnsUserAlreadyExistsResult()
    {
        // arrange
        // setup registration info submitted through the API
        var submittedInfo = new RegistrationUserSubmitModel
        {
            Username = "doesntmatter",
            Password = "AStrongAndValid!!!Password99",
            Email = "ialreadyexist@example.net"
        };
        var request = new RegistrationRequest(new RegistrationSubmitModel
        {
            User = submittedInfo
        });
        
        _authServiceMock.Setup(a => a.GenerateSalt()).Returns("salt");
        _authServiceMock.Setup(a => a.HashPassword(It.IsAny<string>(), It.IsAny<string>())).Returns("hashedPassword");
        _userDataServiceMock.Setup(m => m.RegisterNewUser(It.IsAny<User>()))
            .ReturnsAsync(new DataServiceResult<User>(null, DataResultStatus.FailedToInsert));
        
        // act
        var result = await _registrationHandler.Handle(request, CancellationToken.None);
        
        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserAlreadyExists, Is.EqualTo(true));
    }

    [TestCase("", "Password must not be empty.")]
    [TestCase(null,"Password must not be empty.")]
    [TestCase("password", "Password must be at least 10 characters long.")]
    [TestCase("passwordpassword", "Password must contain at least one digit.")]
    [TestCase("passwordpassword1", "Password must contain at least one uppercase letter.")]
    [TestCase("PASSWORDPASSWORD1", "Password must contain at least one lowercase letter.")]
    [TestCase("PasswordPassword1", "Password must contain at least one symbol.")]
    public async Task Handle_InvalidPassword_ReturnsError(string password, string errorMessage)
    {
        // arrange
        // setup registration info submitted through the API
        var submittedInfo = new RegistrationUserSubmitModel
        {
            Username = "doesntmatter",
            Password = password,
            Email = "ialreadyexist@example.net"
        };
        var request = new RegistrationRequest(new RegistrationSubmitModel
        {
            User = submittedInfo
        });
        
        // act
        var result = await _registrationHandler.Handle(request, CancellationToken.None);
        
        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors.First().ErrorMessage, Is.EqualTo(errorMessage));
    }

    [TestCase("", "Username must not be empty.")]
    [TestCase(null, "Username must not be empty.")]
    [TestCase("usernametoolongusernametoolongusernametoolongusernametoolongusernametoolongusernametoolongusernametoolong", "Username must be at most 100 characters long.")]
    public async Task Handle_InvalidUsername_ReturnsError(string username, string errorMessage)
    {
        // arrange
        // setup registration info submitted through the API
        var submittedInfo = new RegistrationUserSubmitModel
        {
            Username = username,
            Password = "AStrongAndValid!!!Password99",
            Email = "avalidemailaddress@example.net"
        };
        var request = new RegistrationRequest(new RegistrationSubmitModel
        {
            User = submittedInfo
        });
        
        // act
        var result = await _registrationHandler.Handle(request, CancellationToken.None);
        
        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors.First().ErrorMessage, Is.EqualTo(errorMessage));        
    }

    [Test]
    public async Task Handle_UsernameAlreadyExists_ReturnsError()
    {
        // Arrange
        // setup registration info submitted through the API
        var submittedInfo = new RegistrationUserSubmitModel
        {
            Username = "iamausernamethatalreadyexists",
            Password = "PasswordPassword1Strong!Password",
            Email = "doesntmatter@example.net"
        };
        var request = new RegistrationRequest(new RegistrationSubmitModel
        {
            User = submittedInfo
        });

        // setup database mock for user with same username already exists
        _userDataServiceMock.Setup(m => m.GetUserByUsername(submittedInfo.Username))
            .ReturnsAsync(new DataServiceResult<User>(new User(), DataResultStatus.Ok));

        // Act
        var result = await _registrationHandler.Handle(request, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors.First().ErrorMessage, Is.EqualTo("That username is already in use."));
    }

    [TestCase("","Email address must not be empty.")]
    [TestCase(null,"Email address must not be empty.")]
    [TestCase("notarealemailaddress","Email address must be valid.")]
    [TestCase("someone@notldaddressesallowed","Email address must be valid.")]
    public async Task Handle_InvalidEmail_ReturnsError(string email, string errorMessage)
    {
        // arrange
        // setup registration info submitted through the API
        var submittedInfo = new RegistrationUserSubmitModel
        {
            Username = "validusername",
            Password = "AStrongAndValid!!!Password99",
            Email = email
        };
        var request = new RegistrationRequest(new RegistrationSubmitModel
        {
            User = submittedInfo
        });
        
        // act
        var result = await _registrationHandler.Handle(request, CancellationToken.None);
        
        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ValidationErrors, Is.Not.Null);
        Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
        Assert.That(result.ValidationErrors.First().ErrorMessage, Is.EqualTo(errorMessage));          
    }
}