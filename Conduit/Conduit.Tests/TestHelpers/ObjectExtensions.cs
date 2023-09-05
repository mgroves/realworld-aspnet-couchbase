using System.Text;
using Newtonsoft.Json;

namespace Conduit.Tests.TestHelpers;

public static class ObjectExtensions
{
    public static StringContent ToJsonPayload(this object @this)
    {
        return new StringContent(JsonConvert.SerializeObject(@this),
            Encoding.UTF8, "application/json");
    }
}