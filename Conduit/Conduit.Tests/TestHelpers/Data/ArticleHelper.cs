using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Providers;

namespace Conduit.Tests.TestHelpers.Data;

public static class ArticleHelper
{
    /// <summary>
    /// Assert that a user exists
    /// </summary>
    /// <param name="username">Required: the username (key) of the user document</param>
    /// <param name="assertions">Optional: additional assertions to run on the user</param>
    public static async Task AssertExists(this IConduitArticlesCollectionProvider @this, string slug, Action<Article>? assertions = null)
    {
        var collection = await @this.GetCollectionAsync();
        var articleInDatabase = await collection.GetAsync(slug);
        var articleInDatabaseObj = articleInDatabase.ContentAs<Article>();

        if (assertions != null)
            assertions(articleInDatabaseObj);

        // if we made it this far, the user was retrieved
        // and the assertions passed
        Assert.That(true);
    }
}