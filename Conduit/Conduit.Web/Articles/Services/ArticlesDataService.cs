using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Extensions;
using Couchbase;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;
using Couchbase.Transactions;
using Couchbase.Transactions.Config;

namespace Conduit.Web.Articles.Services;

public interface IArticlesDataService
{
    Task Create(Article articleToInsert);
    Task Favorite(string slug, string username);
    Task Unfavorite(string slug, string username);
    Task<bool> Exists(string slug);
    Task<DataServiceResult<Article>> Get(string slug);
    Task<bool> IsFavorited(string slug, string username);
    Task<bool> UpdateArticle(Article newArticle);
    Task<DataServiceResult<string>> DeleteArticle(string slug);
    Task<bool> IsArticleAuthor(string slug, string username);
}

public class ArticlesDataService : IArticlesDataService
{
    private readonly IConduitArticlesCollectionProvider _articlesCollectionProvider;
    private readonly IConduitFavoritesCollectionProvider _favoritesCollectionProvider;

    public ArticlesDataService(IConduitArticlesCollectionProvider articlesCollectionProvider, IConduitFavoritesCollectionProvider favoritesCollectionProvider)
    {
        _articlesCollectionProvider = articlesCollectionProvider;
        _favoritesCollectionProvider = favoritesCollectionProvider;
    }

    public async Task Create(Article articleToInsert)
    {
        var collection = await _articlesCollectionProvider.GetCollectionAsync();

        await collection.InsertAsync(articleToInsert.ArticleKey, articleToInsert);
    }

    public async Task<bool> Exists(string slug)
    {
        var articlesCollection = await _articlesCollectionProvider.GetCollectionAsync();
        var articleExists = await articlesCollection.ExistsAsync(slug.GetArticleKey());
        return articleExists.Exists;
    }

    public async Task<DataServiceResult<Article>> Get(string slug)
    {
        var articlesCollection = await _articlesCollectionProvider.GetCollectionAsync();
        var articleDoc = await articlesCollection.TryGetAsync(slug.GetArticleKey());
        if (!articleDoc.Exists)
            return new DataServiceResult<Article>(null, DataResultStatus.NotFound);
        var article = articleDoc.ContentAs<Article>();
        return new DataServiceResult<Article>(article, DataResultStatus.Ok);
    }

    public async Task<DataServiceResult<Article>> Get(string slug, string username = null)
    {
        var collection = await _articlesCollectionProvider.GetCollectionAsync();

        var articleDoc = await collection.GetAsync(slug.GetArticleKey());
        var article = articleDoc.ContentAs<Article>();
        if(!string.IsNullOrEmpty(username))
            article.Favorited = await IsFavorited(slug, username);
        return new DataServiceResult<Article>(article, DataResultStatus.Ok);
    }

    public async Task<bool> IsFavorited(string slug, string username)
    {
        var collection = await _favoritesCollectionProvider.GetCollectionAsync();
        var set = collection.Set<string>(FavoriteDocId(username));
        return await set.ContainsAsync(slug.GetArticleKey());
    }

    /// <summary>
    /// Update an existing article with new values.
    /// Only Title+Slug, Body, Description, and TagList can be updated
    /// </summary>
    /// <param name="newArticle">Article with values to update - only non-null properties will be updated</param>
    /// <returns>True if succeeded</returns>
    public async Task<bool> UpdateArticle(Article newArticle)
    {
        var collection = await _articlesCollectionProvider.GetCollectionAsync();

        try
        {
            await collection.MutateInAsync(newArticle.ArticleKey, spec =>
            {
                if (newArticle.Title != null)
                {
                    spec.Replace("title", newArticle.Title);
                    spec.Replace("slug", newArticle.Slug);
                }
                if (newArticle.Body != null)
                    spec.Replace("body", newArticle.Body);
                if (newArticle.Description != null)
                    spec.Replace("description", newArticle.Description);
                if (newArticle.TagList != null)
                    spec.Replace("tagList", newArticle.TagList);

                // always update updatedAt
                spec.Upsert("updatedAt", new DateTimeOffset(DateTime.Now));
            });
        }
        catch (Exception ex)
        {
            // TODO: maybe some better handling here, like DocumentNotFound?
            return false;
        }

        return true;
    }

    public async Task<DataServiceResult<string>> DeleteArticle(string slug)
    {
        var articlesCollection = await _articlesCollectionProvider.GetCollectionAsync();
        try
        {
            await articlesCollection.RemoveAsync(slug.GetArticleKey());
        }
        catch (DocumentNotFoundException ex)
        {
            return new DataServiceResult<string>(slug, DataResultStatus.NotFound);
        }
        return new DataServiceResult<string>(slug, DataResultStatus.Ok);
    }

    public async Task<bool> IsArticleAuthor(string slug, string username)
    {
        var articlesCollection = await _articlesCollectionProvider.GetCollectionAsync();

        try
        {
            var result =
                await articlesCollection.LookupInAsync(slug.GetArticleKey(), spec => { spec.Get("authorUsername"); });

            var authorUsername = result.ContentAs<string>(0);

            return username == authorUsername;
        }
        catch (DocumentNotFoundException ex)
        {
            return false;
        }
        catch (ArgumentException ex)
        {
            return false;
        }
    }

    public async Task Favorite(string slug, string username)
    {
        // create favorites document if necessary
        await EnsureFavoritesDocumentExists(username);

        // start transaction
        var articlesCollection = await _articlesCollectionProvider.GetCollectionAsync();
        var favoriteCollection = await _favoritesCollectionProvider.GetCollectionAsync();
        var cluster = favoriteCollection.Scope.Bucket.Cluster;

        var config = TransactionConfigBuilder.Create();

        // for single-node Couchbase, like for development, you must use None
        // otherwise, use AT LEAST Majority durability
#if DEBUG
        config.DurabilityLevel(DurabilityLevel.None);
#else
        config.DurabilityLevel(DurabilityLevel.Majority);
#endif

        var transaction = Transactions.Create(cluster, config);

        await transaction.RunAsync(async (context) =>
        {
            var favoriteKey = FavoriteDocId(username);

            // check to see if user has already favorited this article (if they have, bail out)
            var favoritesDoc = await context.GetAsync(favoriteCollection, favoriteKey);
            var favorites = favoritesDoc.ContentAs<List<string>>();
            // BUG? https://issues.couchbase.com/browse/TXNN-134
            if (favorites.Contains(slug.GetArticleKey()))
            {
                await context.RollbackAsync();
                return;
            }

            // add article key (subset of slug) to favorites document
            favorites.Add(slug.GetArticleKey());
            await context.ReplaceAsync(favoritesDoc, favorites);

            // increment favorite count in article
            var articleDoc = await context.GetAsync(articlesCollection, slug.GetArticleKey());
            var article = articleDoc.ContentAs<Article>();
            article.FavoritesCount++;
            await context.ReplaceAsync(articleDoc, article);
            await context.CommitAsync();
        });

        await transaction.DisposeAsync();
    }

    public async Task Unfavorite(string slug, string username)
    {
        // if there is no favorites document yet, then we're already done
        var favoritesExist = await DoesFavoriteDocumentExist(username);
        if (!favoritesExist)
            return;

        // start transaction
        var articlesCollection = await _articlesCollectionProvider.GetCollectionAsync();
        var favoriteCollection = await _favoritesCollectionProvider.GetCollectionAsync();
        var cluster = favoriteCollection.Scope.Bucket.Cluster;

        var config = TransactionConfigBuilder.Create();

        // for single-node Couchbase, like for development, you must use None
        // otherwise, use AT LEAST Majority durability
#if DEBUG
        config.DurabilityLevel(DurabilityLevel.None);
#else
        config.DurabilityLevel(DurabilityLevel.Majority);
#endif

        var transaction = Transactions.Create(cluster, config);

        await transaction.RunAsync(async (context) =>
        {
            var favoriteKey = FavoriteDocId(username);

            // check to see if user has already favorited this article (if they have NOT, bail out)
            var favoritesDoc = await context.GetAsync(favoriteCollection, favoriteKey);
            var favorites = favoritesDoc.ContentAs<List<string>>();
            // BUG? https://issues.couchbase.com/browse/TXNN-134
            if (!favorites.Contains(slug.GetArticleKey()))
            {
                await context.RollbackAsync();
                return;
            }

            // remove article key (subset of slug) to favorites document
            favorites.Remove(slug.GetArticleKey());
            await context.ReplaceAsync(favoritesDoc, favorites);

            // decrement favorite count in article
            var articleDoc = await context.GetAsync(articlesCollection, slug.GetArticleKey());
            var article = articleDoc.ContentAs<Article>();
            article.FavoritesCount--;
            await context.ReplaceAsync(articleDoc, article);
            await context.CommitAsync();
        });

        await transaction.DisposeAsync();
    }

    public static string FavoriteDocId(string username)
    {
        return $"{username}::favorites";
    }

    private async Task<bool> DoesFavoriteDocumentExist(string username)
    {
        var favoriteDocId = FavoriteDocId(username);
        var collection = await _favoritesCollectionProvider.GetCollectionAsync();
        var favoritesDoc = await collection.ExistsAsync(favoriteDocId);
        return favoritesDoc.Exists;
    }

    private async Task EnsureFavoritesDocumentExists(string username)
    {
        var doesFavoriteDocExist = await DoesFavoriteDocumentExist(username);
        if (doesFavoriteDocExist)
            return;

        try
        {
            var favoriteDocId = FavoriteDocId(username);
            var collection = await _favoritesCollectionProvider.GetCollectionAsync();
            await collection.InsertAsync(favoriteDocId, new List<string>());
        }
        catch (DocumentExistsException ex)
        {
            // if this exception happens, that's fine! I just want the document to exist
        }
    }
}