namespace Conduit.Web.Auth.Services;

public interface IAuthService
{
    string GenerateJwtToken(string username);
    bool DoesPasswordMatch(string submittedPassword, string passwordFromDatabase, string passwordSalt);
    string HashPassword(string password, string passwordSalt);
    string GenerateSalt();
}