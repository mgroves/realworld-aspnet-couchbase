using Conduit.Web.DataAccess.Providers;
using Couchbase.Query;

namespace Conduit.Tests.TestHelpers;

internal class ArticlesDataServiceWrapper : Web.Articles.Services.ArticlesDataService
{
    public ArticlesDataServiceWrapper(IConduitArticlesCollectionProvider articlesCollectionProvider, IConduitFavoritesCollectionProvider favoritesCollectionProvider)
        : base(articlesCollectionProvider, favoritesCollectionProvider)
    {
    }
    public QueryScanConsistency ScanConsistencyValue { private get; set; }

    protected override QueryScanConsistency ScanConsistency => ScanConsistencyValue;
}