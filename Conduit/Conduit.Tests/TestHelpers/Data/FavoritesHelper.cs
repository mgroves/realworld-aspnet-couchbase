using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Extensions;
using Couchbase.KeyValue;

namespace Conduit.Tests.TestHelpers.Data;

public static class FavoritesHelper
{
    public static async Task AddFavoriteInDatabase(this IConduitFavoritesCollectionProvider @this,
        string? username = "",
        string? articleSlug = "")
    {
        var random = new Random();
        username ??= $"valid-username-{random.String(8)}";
        articleSlug ??= $"valid-slug-{random.String(8)}::{random.String(10)}";

        var collection = await @this.GetCollectionAsync();

        var set = collection.Set<string>(ArticlesDataService.FavoriteDocId(username));
        await set.AddAsync(articleSlug.GetArticleKey());

        return;
    }
}