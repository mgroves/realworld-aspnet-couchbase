using Newtonsoft.Json.Linq;

namespace Conduit.Tests.TestHelpers;

public static class JsonTestHelpers
{
    public static T SubDoc<T>(this string @this, string subDocKey)
    {
        var obj = JObject.Parse(@this);
        if (!obj.ContainsKey(subDocKey))
            throw new ArgumentException($"SubDoc '{subDocKey}' not found in JSON.");
        return obj[subDocKey].ToObject<T>();
    }
}
