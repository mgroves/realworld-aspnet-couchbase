using System.Text;

namespace Conduit.Web.Users.Services;

public class JwtSecrets
{
    public string Issuer { get; set; }
    public string Audience { get; set; }

    private string _securityKey;

    public string SecurityKey
    {
        get => _securityKey;
        set
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(value);
            var numBits = byteArray.Length * 8;
            if (numBits <= 256)
                throw new ArgumentException($"SecurityKey must be greater than 256 bits. The security key '{value}' is only '{numBits}' bits.");
            _securityKey = value;
        }
    }
}