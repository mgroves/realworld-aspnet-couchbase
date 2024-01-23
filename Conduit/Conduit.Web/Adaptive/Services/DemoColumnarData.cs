using Conduit.Web.Adaptive.Handlers;
using Conduit.Web.DataAccess.Providers;
using Newtonsoft.Json.Linq;

namespace Conduit.Web.Adaptive.Services;

public interface IDemoColumnarData
{
    Task<GenerateSummaryRequest> GetReportRawData();
}

public class DemoColumnarData : IDemoColumnarData
{
    private readonly IConduitArticlesCollectionProvider _articlesCollectionProvider;

    public DemoColumnarData(IConduitArticlesCollectionProvider articlesCollectionProvider)
    {
        _articlesCollectionProvider = articlesCollectionProvider;
    }

    public async Task<GenerateSummaryRequest> GetReportRawData()
    {
        var collection = await _articlesCollectionProvider.GetCollectionAsync();
        var scope = collection.Scope;
        var bucket = scope.Bucket;
        var cluster = collection.Scope.Bucket.Cluster;

        var sql = @$"SELECT DISTINCT a.title, a.body, a.viewsCount, a.tagList
            FROM `{bucket.Name}`.`{scope.Name}`.`Articles` AS a, a.tagList[0] AS t
            WHERE t IN [""Artificial Intelligence"", ""Data Science"", ""Machine Learning""]
            ORDER BY a.viewsCount DESC
            LIMIT 5;";

        var query = await cluster.QueryAsync<JObject>(sql);

        var rows = await query.Rows.ToListAsync();
        var json =
            string.Join(' ',
                rows.Select(r => r.ToString(Newtonsoft.Json.Formatting.None)));

        var req = new GenerateSummaryRequest();
        req.RawData = json;
        req.Tag = "\"Artificial Intelligence\", \"Data Science\", \"Machine Learning\"";
        return req;
    }
}