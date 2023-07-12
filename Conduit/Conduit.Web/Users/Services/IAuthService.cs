namespace Conduit.Web.Users.Services;

public interface IAuthService
{
    string GenerateJwtToken(string email, string username);
    bool DoesPasswordMatch(string submittedPassword, string passwordFromDatabase, string passwordSalt);
    string HashPassword(string password, string passwordSalt);
    string GenerateSalt();
    string GetTokenFromHeader(string bearerTokenHeader);
    AuthService.ClaimResult GetEmailClaim(string bearerToken);
    AuthService.ClaimResult GetUsernameClaim(string bearerToken);
}