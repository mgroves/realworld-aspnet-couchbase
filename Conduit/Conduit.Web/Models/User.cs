using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Conduit.Web.Models;

public class User
{
    [JsonIgnore] // ignoring this because the document ID is the username, don't store duplicated data
    public string Username { get; set; }

    public string Email { get; set; }
    public string Bio { get; set; }
    public string Image { get; set; }
    public string Password { get; set; }
    public string PasswordSalt { get; set; }
}
