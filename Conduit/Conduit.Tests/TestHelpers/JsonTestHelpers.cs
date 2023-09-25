using System.Diagnostics;
using App.Metrics.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Conduit.Tests.TestHelpers;

public static class JsonTestHelpers
{
    public static T SubDoc<T>(this string @this, string subDocKey)
    {
        try
        {
            var obj = JObject.Parse(@this);
            if (!obj.ContainsKey(subDocKey))
                throw new ArgumentException($"SubDoc '{subDocKey}' not found in JSON.");
            return obj[subDocKey].ToObject<T>();
        }
        catch
        {   
            Trace.WriteLine("TRACE There was a problem getting the subdoc of '{subDocKey}' from this JSON string: '{@this}");
            Debug.Print($"DEBUG There was a problem getting the subdoc of '{subDocKey}' from this JSON string: '{@this}");
            var logfactory = (ILoggerFactory)new LoggerFactory();
            var logger = new Logger<Conduit.Web.Program>(logfactory);
            logger.LogError($"TRACE There was a problem getting the subdoc of '{subDocKey}' from this JSON string: '{@this}");
            throw;
        }
    }
}
