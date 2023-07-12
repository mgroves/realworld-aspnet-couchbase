using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Conduit.Web.Users.Services;

public class AuthService : IAuthService
{
    private const string CLAIMTYPE_USERNAME = "Username";

    public string GenerateJwtToken(string email, string username)
    {
        // TODO: put username in claim too?

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(CLAIMTYPE_USERNAME, username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("6B{DqP5aT,3b&!YRgk29m@j$L7uvnxE"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "ConduitAspNetCouchbase_Issuer",
            audience: "ConduitAspNetCouchbase_Audience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(4), // Set the token expiration time
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool DoesPasswordMatch(string submittedPassword, string passwordFromDatabase, string passwordSalt)
    {
        return HashPassword(submittedPassword, passwordSalt) == passwordFromDatabase;
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

    public class ClaimResult
    {
        public string Value { get; set; }
        public bool IsNotFound { get; set; }
    }
}