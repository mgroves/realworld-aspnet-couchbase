using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Conduit.Web.Models;
using Conduit.Web.ViewModels;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Conduit.Web.Controllers;

public class AuthController : Controller
{
    private readonly IBucketProvider _bucketProvider;

    public AuthController(IBucketProvider bucketProvider)
    {
        _bucketProvider = bucketProvider;
    }

    [HttpPost("api/users/login")]
    public async Task<IActionResult> Login([FromBody] LoginSubmitModel model)
    {
        // get a couchbase collection
        var bucket = await _bucketProvider.GetBucketAsync("Conduit");
        var collection = await bucket.CollectionAsync("Users");

        // make sure credentials match
        var userExists = await collection.ExistsAsync(model.User.Email);
        if (!userExists.Exists)
            return Unauthorized();

        var userDoc = await collection.GetAsync(model.User.Email);
        var userObj = userDoc.ContentAs<User>();
        if (!userObj.DoesPasswordMatch(model.User.Password))
            return Unauthorized();

        // return a user view object WITH a JWT token
        var userView = new UserViewModel
        {
            Email = userObj.Email,
            Token = GenerateJwtToken(userObj.Username),
            Username = userObj.Username,
            Bio = userObj.Bio,
            Image = userObj.Image
        };

        return Ok(new
        {
            user = userView
        });
    }

    [HttpPost("api/users")]
    public async Task<IActionResult> Registration([FromBody] RegistrationSubmitModel model)
    {
        // insert this registration into database
        var bucket = await _bucketProvider.GetBucketAsync("Conduit");
        var collection = await bucket.CollectionAsync("Users");

        var passwordSalt = Models.User.GenerateSalt();
        var userToInsert = new User
        {
            Username = model.User.Username,
            Password = Models.User.HashPassword(model.User.Password, passwordSalt),
            PasswordSalt = passwordSalt
        };

        try
        {
            // couchbase keys must be unique
            // registration shouldn't work if the email address is already in use
            await collection.InsertAsync(model.User.Email, userToInsert);
        }
        catch (DocumentExistsException ex)
        {
            return Forbid();
        }

        var userView = new UserViewModel
        {
            Email = model.User.Email,
            Username = model.User.Username,
            Image = null,
            Bio = null,
            Token = GenerateJwtToken(model.User.Username)
        };

        return Ok(new { user = userView });
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