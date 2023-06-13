using Conduit.Web.Auth.Handlers;
using Conduit.Web.Auth.Services;
using Conduit.Web.Auth.ViewModels;
using Conduit.Web.Models;
using Couchbase.Core.Exceptions.KeyValue;
using Moq;

namespace Conduit.Tests.Unit.Auth.Handlers;

[TestFixture]
public class RegistrationRequestHandlerTests : WithCouchbaseMocks
{
    private Mock<IAuthService> _authServiceMock;
    private RegistrationRequestHandler _registrationHandler;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
            
        _authServiceMock = new Mock<IAuthService>();

        _registrationHandler = new RegistrationRequestHandler(BucketProviderMock.Object, _authServiceMock.Object, new RegistrationRequestValidator());
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

        // setup database and auth service mocks
        _authServiceMock.Setup(a => a.GenerateSalt()).Returns("salt");
        _authServiceMock.Setup(a => a.HashPassword(It.IsAny<string>(), It.IsAny<string>())).Returns("hashedPassword");
        _authServiceMock.Setup(a => a.GenerateJwtToken(It.IsAny<string>())).Returns("jwttoken");
        CollectionMock.Setup(c => c.InsertAsync(submittedInfo.Email, It.IsAny<User>(), null));
        
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
        CollectionMock.Setup(c => c.InsertAsync(submittedInfo.Email, It.IsAny<User>(), null))
            .Throws<DocumentExistsException>();
        
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
    
    // TODO: test for username already exists

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