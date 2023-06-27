﻿namespace Conduit.Tests.Unit.Auth.Services.AuthService;

[TestFixture]
public class GetTokenFromHeaderTests
{
    [TestCase("thisisatoken", "thisisatoken")]
    [TestCase("Token thisisatoken", "thisisatoken")]
    public async Task Can_parse_header_down_to_just_token_value(string raw, string expectedParsed)
    {
        // arrange
        var authService = new Web.Users.Services.AuthService();

        // act
        var result = authService.GetTokenFromHeader(raw);

        // assert
        Assert.That(result, Is.EqualTo(expectedParsed));
    }
}