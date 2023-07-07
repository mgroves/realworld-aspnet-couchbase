using System.Security.Claims;

namespace Conduit.Web.Users.Services;

public interface IAuthService
{
    string GenerateJwtToken(string email);
    bool DoesPasswordMatch(string submittedPassword, string passwordFromDatabase, string passwordSalt);
    string HashPassword(string password, string passwordSalt);
    string GenerateSalt();
    string GetTokenFromHeader(string bearerTokenHeader);
    AuthService.ClaimResult GetEmailClaim(string bearerToken);
}