using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Conduit.Web.Users.Services;

public class AuthService : IAuthService
{
    private readonly JwtSecrets _jwtSecrets;
    private const string CLAIMTYPE_USERNAME = "Username";

    public AuthService(IOptions<JwtSecrets> jwtSecrets)
    {
        _jwtSecrets = jwtSecrets.Value;
    }

    public string GenerateJwtToken(string email, string username)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(CLAIMTYPE_USERNAME, username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecrets.SecurityKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSecrets.Issuer,
            audience: _jwtSecrets.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(4), // Set the token expiration time
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool DoesPasswordMatch(string submittedPassword, string passwordFromDatabase, string passwordSalt)
    {
        return BCrypt.Net.BCrypt.Verify(submittedPassword, passwordFromDatabase);
    }

    public string HashPassword(string password, string passwordSalt)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, passwordSalt);
    }

    public string GenerateSalt()
    {
        return BCrypt.Net.BCrypt.GenerateSalt();
    }

    public string GetTokenFromHeader(string bearerTokenHeader)
    {
        if (string.IsNullOrEmpty(bearerTokenHeader))
            return null;

        var tokenPrefix = "Token ";
        return bearerTokenHeader.StartsWith(tokenPrefix)
            ? bearerTokenHeader.Substring(tokenPrefix.Length).Trim()
            : bearerTokenHeader.Trim();
    }

    public ClaimResult GetEmailClaim(string bearerToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var claims = handler.ReadJwtToken(bearerToken).Claims;
            var email = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value;
            return new ClaimResult
            {
                Value = email
            };
        }
        catch (ArgumentException)
        {
            return new ClaimResult
            {
                IsNotFound = true
            };
        }
    }
    
    public ClaimResult GetUsernameClaim(string bearerToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var claims = handler.ReadJwtToken(bearerToken).Claims;
            var email = claims.FirstOrDefault(claim => claim.Type == CLAIMTYPE_USERNAME)?.Value;
            return new ClaimResult
            {
                Value = email
            };
        }
        catch (ArgumentException)
        {
            return new ClaimResult
            {
                IsNotFound = true
            };
        }
    }

    public AllInfo GetAllAuthInfo(string authorizationHeader)
    {
        var bearerToken = GetTokenFromHeader(authorizationHeader);
        var all = new AllInfo
        {
            BearerToken = bearerToken,
            Email = GetEmailClaim(bearerToken),
            Username = GetUsernameClaim(bearerToken)
        };
        return all;
    }

    public bool IsUserAuthenticated(string bearerToken)
    {
        return !string.IsNullOrEmpty(bearerToken);
    }

    public class ClaimResult
    {
        public string Value { get; set; }
        public bool IsNotFound { get; set; }
    }

    public class AllInfo
    {
        public string BearerToken { get; set; }
        public ClaimResult Username { get; set; }
        public ClaimResult Email { get; set; }
    }
}