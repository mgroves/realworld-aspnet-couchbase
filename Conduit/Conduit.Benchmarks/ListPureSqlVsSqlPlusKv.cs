using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Conduit.Tests.TestHelpers.Data;
using Conduit.Web.Articles.Handlers;
using Conduit.Web.Articles.Services;
using Conduit.Web.DataAccess.Models;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Follows.Services;
using Conduit.Web.Users.Services;
using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Conduit.Benchmarks;

[SimpleJob(RuntimeMoniker.HostProcess)]
[RPlotExporter]
public class ListPureSqlVsSqlPlusKv
{
    private ICluster _cluster;
    private IBucket _bucket;
    private IScope _scope;
    private ICouchbaseCollection _usersCollection;
    private ICouchbaseCollection _articlesCollection;
    private User _currentUser;
    private ICouchbaseCollection _favoritesCollection;
    private ICouchbaseCollection _followsCollection;
    private List<string> _authorUsernames;
    private Random _random;
    private IMediator _mediator;

    [GlobalSetup]
    public async Task Setup()
    {
        _cluster = await Cluster.ConnectAsync("couchbase://localhost", "Administrator", "password");
        _bucket = await _cluster.BucketAsync("Conduit");
        _scope = await _bucket.ScopeAsync("_default");
        _usersCollection = await _scope.CollectionAsync("Users");
        _articlesCollection = await _scope.CollectionAsync("Articles");
        _favoritesCollection = await _scope.CollectionAsync("Favorites");
        _followsCollection = await _scope.CollectionAsync("Follows");
        _currentUser = await _usersCollection.CreateUserInDatabase();
        _authorUsernames = new List<string>();
        _random = new Random();

        var serviceCollection = new ServiceCollection()
            .AddTransient<IAuthService,AuthServiceDummy>()
            .AddTransient<IFollowDataService, FollowsDataService>()
            .AddTransient<IUserDataService, UserDataService>()
            .AddTransient<IArticlesDataService, ArticlesDataService>()
            .AddCouchbase(options =>
            {
                options.ConnectionString = "couchbase://localhost";
                options.UserName = "Administrator";
                options.Password = "password";
            })
            .AddCouchbaseBucket<IConduitBucketProvider>("Conduit", b =>
            {
                b
                    .AddScope("_default")
                    .AddCollection<IConduitUsersCollectionProvider>("Users");
                b
                    .AddScope("_default")
                    .AddCollection<IConduitFollowsCollectionProvider>("Follows");
                b
                    .AddScope("_default")
                    .AddCollection<IConduitTagsCollectionProvider>("Tags");
                b
                    .AddScope("_default")
                    .AddCollection<IConduitArticlesCollectionProvider>("Articles");
                b
                    .AddScope("_default")
                    .AddCollection<IConduitFavoritesCollectionProvider>("Favorites");
            })
            .AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<GetArticleHandler>();
            })
            .AddLogging(builder => new NullLoggerFactory())
            .BuildServiceProvider();

        _mediator = serviceCollection.GetRequiredService<IMediator>();

        // mediator smoke test
        var request = new GetArticleRequest("slug-doest-exist::199af99as9sdaf", _currentUser.Username);
        var response = await _mediator.Send(request, CancellationToken.None);

        // create a few thousand articles
        await CreateTestData(1000, 100);
    }

    private async Task CreateTestData(int numberOfArticles, int numberOfAuthors)
    {
        // create authors, keep track of their usernames for later
        for (var i = 0; i < numberOfAuthors; i++)
        {
            var user = await _usersCollection.CreateUserInDatabase();
            _authorUsernames.Add(user.Username);
            Console.WriteLine("Created author: " + i);
        }


        // add some random following
        for (var i = 0; i < 50; i++)
        {
            var randomFollower = _authorUsernames
                .OrderBy(a => _random.Next(numberOfAuthors))
                .First();
            var randomFollowee = _authorUsernames
                .OrderBy(a => _random.Next(numberOfAuthors))
                .First();
            await _followsCollection.CreateFollow(randomFollower, randomFollowee);
            Console.WriteLine("Created some following: " + i);
        }

        for (var i = 0; i < numberOfArticles; i++)
        {
            var randomAuthor = _authorUsernames
                .OrderBy(a => _random.Next(numberOfAuthors))
                .First();
            var article = await _articlesCollection.CreateArticleInDatabase(authorUsername: randomAuthor);

            // logged in user should favorite every 1/5
            // random user favoriting it elsewhere
            var shouldFavorite = (_random.Next(5) % 3) == 0;
            if (shouldFavorite)
                await _favoritesCollection.AddFavoriteInDatabase(_currentUser.Username, article.Slug);
            else
            {
                var randomUser = _authorUsernames
                    .OrderBy(a => _random.Next(numberOfAuthors))
                    .First();
                await _favoritesCollection.AddFavoriteInDatabase(randomUser, article.Slug);
            }
            Console.WriteLine("Created articles: " + i);
        }
    }

    /// <summary>
    /// List - do 50 reads
    /// </summary>
    [Benchmark]
    [IterationCount(50)]
    public async Task PureSql()
    {
        // SQL stripped down from filters to a "base" query for authenticated user
        var sql = @"SELECT 
                   a.slug,
                   a.title,
                   a.description,
                   a.body,
                   a.tagList,
                   a.createdAt,
                   a.updatedAt,
                   ARRAY_CONTAINS(favCurrent, articleKey) AS favorited,
                   a.favoritesCount,
                   {
                        ""username"": META(u).id,
                        u.bio,
                        u.image,
                        ""following"": ARRAY_CONTAINS(COALESCE(fol,[]), META(u).id)
                   } AS author

                FROM Conduit._default.Articles a
                JOIN Conduit._default.Users u ON a.authorUsername = META(u).id

                /* these next lines are only for authenticated users */
                /* usernames need parameterized */
                LEFT JOIN Conduit._default.Favorites favCurrent ON META(favCurrent).id = ($currentUser || ""::favorites"")
                LEFT JOIN Conduit._default.`Follows` fol ON META(fol).id = ($currentUser || ""::follows"")

                /* for use with optional filter */
                /* username need parameterized */
                LEFT JOIN Conduit._default.Favorites favFilter ON META(favFilter).id = ($randomUser || ""::favorites"")

                /* convenience variable for getting the ArticleKey from slug */
                LET articleKey = SPLIT(a.slug, ""::"")[1]

                ORDER BY COALESCE(a.updatedAt, a.createdAt) DESC

                /* needs parameterized */
                LIMIT 20
                OFFSET 0";
        var queryResult = await _cluster.QueryAsync<dynamic>(sql, options =>
        {
            var randomAuthor = _authorUsernames
                .OrderBy(a => _random.Next(1000))
                .First();
            options.Parameter("currentUser", _currentUser.Username);
            options.Parameter("randomUser", randomAuthor);
        });

        Console.WriteLine("Query Results");
        await foreach (var result in queryResult)
        {
            Console.WriteLine($"Returned {result.title}.");
        }
    }

    [Benchmark]
    [IterationCount(50)]
    public async Task SqlPlusKv()
    {
        // SQL stripped down from filters to a "base" query for authenticated user
        // AND stripped down to just slug/article key
        var sql = @"SELECT 
                   a.slug

                FROM Conduit._default.Articles a
                JOIN Conduit._default.Users u ON a.authorUsername = META(u).id

                /* these next lines are only for authenticated users */
                /* usernames need parameterized */
                LEFT JOIN Conduit._default.Favorites favCurrent ON META(favCurrent).id = ($currentUser || ""::favorites"")
                LEFT JOIN Conduit._default.`Follows` fol ON META(fol).id = ($currentUser || ""::follows"")

                /* for use with optional filter */
                /* username need parameterized */
                LEFT JOIN Conduit._default.Favorites favFilter ON META(favFilter).id = ($randomUser || ""::favorites"")

                ORDER BY COALESCE(a.updatedAt, a.createdAt) DESC

                /* needs parameterized */
                LIMIT 20
                OFFSET 0";

        var queryResult = await _cluster.QueryAsync<dynamic>(sql, options =>
        {
            var randomAuthor = _authorUsernames
                .OrderBy(a => _random.Next(1000))
                .First();
            options.Parameter("currentUser", _currentUser.Username);
            options.Parameter("randomUser", randomAuthor);
        });

        Console.WriteLine("Query Results");
        await foreach (var result in queryResult)
        {
            var slug = result.slug.ToString();
            var request = new GetArticleRequest(slug, _currentUser.Username);
            var response = await _mediator.Send(request, CancellationToken.None);
            Console.WriteLine($"Returned: {response.ArticleView.Title}");
        }

    }

    #region AuthServiceDummy
    public class AuthServiceDummy : IAuthService
    {
        public string GenerateJwtToken(string email, string username)
        {
            throw new NotImplementedException();
        }

        public bool DoesPasswordMatch(string submittedPassword, string passwordFromDatabase, string passwordSalt)
        {
            throw new NotImplementedException();
        }

        public string HashPassword(string password, string passwordSalt)
        {
            throw new NotImplementedException();
        }

        public string GenerateSalt()
        {
            throw new NotImplementedException();
        }

        public string GetTokenFromHeader(string bearerTokenHeader)
        {
            throw new NotImplementedException();
        }

        public AuthService.ClaimResult GetEmailClaim(string bearerToken)
        {
            throw new NotImplementedException();
        }

        public AuthService.ClaimResult GetUsernameClaim(string bearerToken)
        {
            throw new NotImplementedException();
        }

        public AuthService.AllInfo GetAllAuthInfo(string authorizationHeader)
        {
            throw new NotImplementedException();
        }

        public bool IsUserAuthenticated(string bearerToken)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}