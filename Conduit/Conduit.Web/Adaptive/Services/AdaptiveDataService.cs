using Conduit.Web.Articles.ViewModels;
using Conduit.Web.DataAccess.Dto.Articles;
using Conduit.Web.DataAccess.Dto;
using Conduit.Web.DataAccess.Providers;
using Couchbase.Query;
using Couchbase;
using Conduit.Web.Users.Services;
using Couchbase.KeyValue;

namespace Conduit.Web.Adaptive.Services;

public interface IAdaptiveDataService
{
    Task<DataServiceResult<ArticlesViewModel>> GetAdaptiveArticles(GetAdaptiveArticlesSpec spec);
    Task UpdateUserProfileAdaptiveTags(string username, string bio);
    Task<List<string>> GetAdaptiveProfileTags(string username);

}

public class AdaptiveDataService : IAdaptiveDataService
{
    private readonly IConduitArticlesCollectionProvider _articlesCollectionProvider;
    private readonly IGenerativeAiService _genAiService;
    private readonly IConduitUsersCollectionProvider _usersCollectionProvider;

    // a virtual method so it can be overridden by a testing class if necessary
    protected virtual QueryScanConsistency ScanConsistency => QueryScanConsistency.NotBounded;

    public AdaptiveDataService(
        IConduitArticlesCollectionProvider articlesCollectionProvider,
        IGenerativeAiService genAiService,
        IConduitUsersCollectionProvider usersCollectionProvider)
    {
        _articlesCollectionProvider = articlesCollectionProvider;
        _genAiService = genAiService;
        _usersCollectionProvider = usersCollectionProvider;
    }

    public async Task<List<string>> GetAdaptiveProfileTags(string username)
    {
        var collection = await _usersCollectionProvider.GetCollectionAsync();

        var userResult = await collection.LookupInAsync(username, specs =>
        {
            specs.Get("adaptiveTags");
        });

        return userResult.ContentAs<List<string>>(0);
    }

    public async Task UpdateUserProfileAdaptiveTags(string username, string bio)
    {
        var newAdaptiveTags = await _genAiService.GetTagsFromContent(bio);
        if (newAdaptiveTags.Any())
        {
            var collection = await _usersCollectionProvider.GetCollectionAsync();
            await collection.MutateInAsync(username, specs =>
            {
                specs.Upsert("adaptiveTags", newAdaptiveTags);
            });
        }
    }

    public async Task<DataServiceResult<ArticlesViewModel>> GetAdaptiveArticles(GetAdaptiveArticlesSpec spec)
    {
        var collection = await _articlesCollectionProvider.GetCollectionAsync();
        var cluster = collection.Scope.Bucket.Cluster;
        var bucketName = collection.Scope.Bucket.Name;
        var scopeName = collection.Scope.Name;

        var authenticatedUsersJoin = "";
        var authenticatedFavoriteProjection = "";
        var authenticatedFollowProjection = "";
        if (spec.Username != null)
        {
            authenticatedUsersJoin = $@"
            LEFT JOIN `{bucketName}`.`{scopeName}`.`Favorites` favCurrent ON META(favCurrent).id = ($loggedInUsername || ""::favorites"")
            LEFT JOIN `{bucketName}`.`{scopeName}`.`Follows` fol ON META(fol).id = ($loggedInUsername || ""::follows"")";

            authenticatedFavoriteProjection = @"ARRAY_CONTAINS(favCurrent, articleKey) AS favorited";

            authenticatedFollowProjection = @"""following"": ARRAY_CONTAINS(COALESCE(fol,[]), META(u).id)";
        }
        else
        {
            authenticatedFavoriteProjection = "false AS favorited";
            authenticatedFollowProjection = "\"following\" : false";
        }

        var filteringByTagPredicate = "";
        if (spec.Tags != null || spec.Tags.Any())
        {
            filteringByTagPredicate = @" AND ANY t IN a.tagList SATISFIES ARRAY_CONTAINS($tags, t) END ";
        }

        var sql = $@"SELECT 
           a.slug,
           a.title,
           a.description,
           a.body,
           a.tagList,
           a.createdAt,
           a.updatedAt,
           {authenticatedFavoriteProjection},
           a.favoritesCount,
           {{
                ""username"": META(u).id,
                u.bio,
                u.image,
                {authenticatedFollowProjection}
           }} AS author,
           COUNT(*) OVER() AS articlesCount

        FROM `{bucketName}`.`{scopeName}`.`Articles` a
        JOIN `{bucketName}`.`{scopeName}`.`Users` u ON a.authorUsername = META(u).id

        {authenticatedUsersJoin}

        /* convenience variable for getting the ArticleKey from slug */
        LET articleKey = SPLIT(a.slug, ""::"")[1]

        WHERE 1=1
          {filteringByTagPredicate}

        ORDER BY COALESCE(a.updatedAt, a.createdAt) DESC

        LIMIT $limit
        OFFSET $offset
        ";

        var result = await cluster.QueryAsync<ArticleViewModelWithCount>(sql, options =>
        {
            options.Parameter("loggedInUsername", spec.Username);
            options.Parameter("tags", spec.Tags);
            options.Parameter("limit", spec.Limit ?? 20);    // default page size of 20
            options.Parameter("offset", spec.Offset ?? 0);   // default to first page
            options.ScanConsistency(ScanConsistency);
        });

        // this next part is a little hacky, but it works
        // the goal is to get ArticlesCount (returned with each record)
        // separated
        var results = await result.Rows.ToListAsync();
        var articlesResults = new ArticlesViewModel();
        articlesResults.ArticlesCount = results.FirstOrDefault()?.ArticlesCount ?? 0;
        articlesResults.Articles = results.Cast<ArticleViewModel>().ToList();

        return new DataServiceResult<ArticlesViewModel>(articlesResults, DataResultStatus.Ok);
    }
}