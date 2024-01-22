using Conduit.Migrations;
using Conduit.Web.Adaptive.Services;
using Couchbase.Extensions.DependencyInjection;
using Couchbase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NoSqlMigrator.Runner;
using Conduit.Web.DataAccess.Providers;
using Conduit.Web.Users.Services;
using OpenAI_API;

namespace Conduit.Tests.Integration;

[SetUpFixture]
public class GlobalCouchbaseIntegrationSetUp
{
    public static ServiceCollection ServiceCollection;
    private static ServiceProvider? _serviceProvider;
    public static ServiceProvider ServiceProvider
    {
        get { return _serviceProvider ??= ServiceCollection.BuildServiceProvider(); }
    }

    private IConfigurationRoot _config;
    private IBucket _bucket;
    private ICluster _cluster;

    [OneTimeSetUp]
    public async Task RunBeforeAllTests()
    {
        _config = new ConfigurationBuilder()
            .AddUserSecrets<CouchbaseIntegrationTest>()
            .AddEnvironmentVariables()
            .Build();

        _cluster = await Cluster.ConnectAsync(
            _config["Couchbase:ConnectionString"],
            _config["Couchbase:Username"],
            _config["Couchbase:Password"]);

        await _cluster.WaitUntilReadyAsync(TimeSpan.FromSeconds(30));

        var allBuckets = await _cluster.Buckets.GetAllBucketsAsync();
        var doesBucketExist = allBuckets.Any(b => b.Key == _config["Couchbase:BucketName"]);
        if (!doesBucketExist)
            throw new Exception($"There is no bucket for integration testing named '{_config["Couchbase:BucketName"]}'.");

        _bucket = await _cluster.BucketAsync(_config["Couchbase:BucketName"]);

        // run any outstanding migrations
        var runner = new MigrationRunner();
        var upSettings = new RunSettings();
        upSettings.Direction = DirectionEnum.Up;
        upSettings.Bucket = _bucket;
        await runner.Run(typeof(CreateUserCollectionInDefaultScope).Assembly, upSettings);

        // create a DI setup, in order to get Couchbase DI objects 
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCouchbase(options =>
        {
            options.ConnectionString = _config["Couchbase:ConnectionString"];
            options.UserName = _config["Couchbase:Username"];
            options.Password = _config["Couchbase:Password"];
        });

        services.AddCouchbaseBucket<IConduitBucketProvider>(_config["Couchbase:BucketName"], b =>
        {
            b
                .AddScope(_config["Couchbase:ScopeName"])
                .AddCollection<IConduitTagsCollectionProvider>("Tags");
            b
                .AddScope(_config["Couchbase:ScopeName"])
                .AddCollection<IConduitArticlesCollectionProvider>("Articles");
            b
                .AddScope(_config["Couchbase:ScopeName"])
                .AddCollection<IConduitFavoritesCollectionProvider>("Favorites");
            b
                .AddScope(_config["Couchbase:ScopeName"])
                .AddCollection<IConduitUsersCollectionProvider>("Users");
            b
                .AddScope(_config["Couchbase:ScopeName"])
                .AddCollection<IConduitFollowsCollectionProvider>("Follows");
            b
                .AddScope(_config["Couchbase:ScopeName"])
                .AddCollection<IConduitCommentsCollectionProvider>("Comments");
        });

        services.AddTransient<IGenerativeAiService, OpenAiService>();
        services.AddTransient<IOpenAIAPI>(x => new OpenAIAPI(_config["OpenAIApiKey"]));
        services.AddTransient<IAdaptiveDataService, AdaptiveDataService>();

        ServiceCollection = services;
    }

    [OneTimeTearDown]
    public async Task RunAfterAllTests()
    {
        // dispose couchbase services
        await _cluster.DisposeAsync();
        await ServiceProvider.GetRequiredService<ICouchbaseLifetimeService>().CloseAsync();
    }
}
