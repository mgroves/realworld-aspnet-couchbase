using Newtonsoft.Json.Linq;

namespace Conduit.Tests.TestHelpers;

public static class JsonTestHelpers
{
    public static T SubDoc<T>(this string @this, string subDocKey)
    {
        var obj = JObject.Parse(@this);
        return obj[subDocKey].ToObject<T>();
    }
}
