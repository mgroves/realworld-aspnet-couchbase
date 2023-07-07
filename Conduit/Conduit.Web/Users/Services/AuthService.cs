using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Conduit.Web.Users.Services;

public class AuthService : IAuthService
{
    public string GenerateJwtToken(string email)
    {
        // TODO: put username in claim too?

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email)
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
}