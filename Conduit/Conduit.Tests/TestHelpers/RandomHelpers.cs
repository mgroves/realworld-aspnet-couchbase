namespace Conduit.Tests.TestHelpers;

public static class RandomHelpers
{
    const string _defaultCharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string String(this Random @this, int length, string? characterPool = null)
    {
        var chars = characterPool ?? _defaultCharacterSet;
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[@this.Next(s.Length)]).ToArray());
    }
}