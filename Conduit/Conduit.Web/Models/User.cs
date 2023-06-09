using Newtonsoft.Json;

namespace Conduit.Web.Models;

public class User
{
    [JsonIgnore]    // ignoring this because the document ID is the email address, don't store duplicated data
    public string Email { get; set; }
    [JsonIgnore]    // ignoring this because JWT token should not be stored in database (for now)
    public string Token { get; set; }

    public string Username { get; set; }
    public string Bio { get; set; }
    public string Image { get; set; }
    public string Password { get; set; }
    public string PasswordSalt { get; set; }

    public bool DoesPasswordMatch(string submittedPassword)
    {
        return HashPassword(submittedPassword, PasswordSalt) == Password;
    }

    public static string HashPassword(string password, string passwordSalt)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, passwordSalt);
    }

    public static string GenerateSalt()
    {
        return BCrypt.Net.BCrypt.GenerateSalt();
    }
}