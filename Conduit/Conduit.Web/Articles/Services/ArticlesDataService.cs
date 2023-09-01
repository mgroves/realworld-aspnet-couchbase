using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Extensions;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;
using Couchbase.Transactions;
using Couchbase.Transactions.Config;

namespace Conduit.Web.Articles.Services;

public interface IArticlesDataService
{
    Task Create(Article articleToInsert);
    Task Favorite(string slug, string username);
    Task<bool> Exists(string slug);
    Task<DataServiceResult<Article>> Get(string slug);
    Task<bool> IsFavorited(string slug, string username);
}

public class ArticlesDataService : IArticlesDataService
{
    private readonly IConduitArticlesCollectionProvider _articlesCollectionProvider;
    private readonly IConduitFavoritesCollectionProvider _favoritesCollectionProvider;

    private string FavoriteDocId(string username)
    {
        return $"{username}::favorites";
    }

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
        var articleDoc = await articlesCollection.GetAsync(slug.GetArticleKey());
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
        });
    }

    private async Task EnsureFavoritesDocumentExists(string username)
    {
        var favoriteDocId = FavoriteDocId(username);
        var collection = await _favoritesCollectionProvider.GetCollectionAsync();
        var favoritesDoc = await collection.ExistsAsync(favoriteDocId);
        if (favoritesDoc.Exists)
            return;

        try
        {
            await collection.InsertAsync(favoriteDocId, new List<string>());
        }
        catch (DocumentExistsException ex)
        {
            // if this exception happens, that's fine! I just want the document to exist
        }
    }
}