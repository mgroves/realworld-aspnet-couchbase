using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Providers;
using Couchbase.KeyValue;

namespace Conduit.Tests.TestHelpers.Data;

public static class CommentHelper
{
    public static async Task AssertExists(this IConduitCommentsCollectionProvider @this, string key, Action<IList<Comment>>? assertions = null)
    {
        var collection = await @this.GetCollectionAsync();

        var doesExist = await collection.ExistsAsync(key);
        Assert.That(doesExist.Exists, Is.True);

        var listOfComments = collection.List<Comment>(key);
        if (assertions != null)
            assertions(listOfComments);
    }
}