using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Articles.Services;
using Conduit.Web.Extensions;
using Slugify;

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
        var articleInDatabase = await collection.GetAsync(slug.GetArticleKey());
        var articleInDatabaseObj = articleInDatabase.ContentAs<Article>();

        if (assertions != null)
            assertions(articleInDatabaseObj);

        // if we made it this far, the article was retrieved
        // and the assertions passed
        Assert.That(true);
    }

    public static async Task AssertExists(this IConduitFavoritesCollectionProvider @this, string username,
        Action<List<string>>? assertions = null)
    {
        var collection = await @this.GetCollectionAsync();
        var favoritesInDatabase = await collection.GetAsync($"{username}::favorites");
        var favoritesInDatabaseObj = favoritesInDatabase.ContentAs<List<string>>();

        if (assertions != null)
            assertions(favoritesInDatabaseObj);

        // if we made it this far, the favorites was retrieved
        // and the assertions passed
        Assert.That(true);
    }

    public static async Task<Article> CreateArticleInDatabase(this IConduitArticlesCollectionProvider @this,
        int? favoritesCount = null,
        DateTimeOffset? createdAt = null,
        string? title = null,
        string? description = null,
        string? body = null,
        string? slug = null,
        List<string>? tagList = null,
        string? authorUsername = null,
        bool? favorited = null)
    {
        var collection = await @this.GetCollectionAsync();

        var article = CreateArticle(favoritesCount, createdAt, title, description, body, slug, tagList, authorUsername,
            favorited);

        await collection.InsertAsync(article.Slug.GetArticleKey(), article);

        return article;
    }

    public static Article CreateArticle(
        int? favoritesCount = null,
        DateTimeOffset? createdAt = null,
        string? title = null,
        string? description = null,
        string? body = null,
        string? slug = null,
        List<string>? tagList = null,
        string? authorUsername = null,
        bool? favorited = null)
    {
        var random = new Random();
        favoritesCount ??= 0;
        createdAt ??= DateTimeOffset.Now;
        title ??= "Title " + random.String(20);
        description ??= "Description " + random.String(256);
        authorUsername ??= "user-" + random.String(8);
        body ??= "Body " + random.String(10000);
        slug ??= new SlugService(new SlugHelper()).GenerateSlug(title);
        tagList ??= new List<string> { "Couchbase", "baseball"};
        favorited ??= false;
        
        var article = new Article();
        article.FavoritesCount = favoritesCount.Value;
        article.CreatedAt = createdAt.Value;
        article.Title = title;
        article.Description = description;
        article.Body = body;
        article.Slug = slug;
        article.TagList = tagList;
        article.AuthorUsername = authorUsername;
        article.Favorited = favorited.Value;

        return article;
    }
}