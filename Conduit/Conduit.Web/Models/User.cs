using Newtonsoft.Json;

namespace Conduit.Web.Models;

public class User
{
    [JsonIgnore]    // ignoring this because the document ID is the email address, don't store duplicated data
    public string Email { get; set; }
    public string Username { get; set; }
    public string Bio { get; set; }
    public string Image { get; set; }
    public string Password { get; set; }
    public string PasswordSalt { get; set; }
}