using Conduit.Web.Users.Services;
using Microsoft.Extensions.Options;

namespace Conduit.Tests.TestHelpers;

public static class AuthServiceHelper
{
    public static AuthService Create()
    {
        // the issuer, audience, securitykey are hardcoded values
        // only meant for testing. Do not use these for your actual secrets!
        return new AuthService(new OptionsWrapper<JwtSecrets>(
            new JwtSecrets()
            {
                Issuer = "PlaceholderIssuer",
                Audience = "PlaceholderAudience",
                SecurityKey = "aaaaabbbbbcccccdddddeeeeefffffggggghhhhhiiiiijjjjjkkkkklllllmmmmmnn"
            }));
    }
}