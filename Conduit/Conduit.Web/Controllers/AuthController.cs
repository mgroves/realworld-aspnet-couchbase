using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Conduit.Web.Models;
using Conduit.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Conduit.Web.Controllers;

public class AuthController : Controller
{
    public AuthController()
    {
        
    }

    [HttpPost("api/users/login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        // TODO: check database make sure credentials are valid

        var userFromDatabase = new User
        {
            Email = model.User.Email,
            Token = GenerateJwtToken(model.User.Email),
            Username = "test",
            Bio = "test",
            Image = "test"
        };

        return Ok(new
        {
            user = userFromDatabase
        });
    }

    private string GenerateJwtToken(string username)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username)
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
}