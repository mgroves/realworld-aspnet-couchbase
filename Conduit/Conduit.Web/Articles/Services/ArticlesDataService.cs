using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Providers;

namespace Conduit.Web.Articles.Services;

public interface IArticlesDataService
{
    Task Create(Article articleToInsert);
}

public class ArticlesDataService : IArticlesDataService
{
    private readonly IConduitArticlesCollectionProvider _articlesCollectionProvider;

    public ArticlesDataService(IConduitArticlesCollectionProvider articlesCollectionProvider)
    {
        _articlesCollectionProvider = articlesCollectionProvider;
    }

    public async Task Create(Article articleToInsert)
    {
        var collection = await _articlesCollectionProvider.GetCollectionAsync();

        await collection.InsertAsync(articleToInsert.Slug, articleToInsert);
    }
}