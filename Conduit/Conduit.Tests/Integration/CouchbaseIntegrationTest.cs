using Conduit.Migrations;
using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NoSqlMigrator.Runner;

namespace Conduit.Tests.Integration;

public abstract class CouchbaseIntegrationTest
{
    protected ServiceCollection ServiceCollection;
    private IConfigurationRoot _config;
    private IBucket _bucket;
    private ICluster _cluster;
    private ServiceProvider? _serviceProvider;

    protected ServiceProvider ServiceProvider
    {
        get { return _serviceProvider ??= ServiceCollection.BuildServiceProvider(); }
    }


    [OneTimeSetUp]
    public virtual async Task Setup()
    {
        _config = new ConfigurationBuilder()
            .AddJsonFile("integrationtestsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        _cluster = await Cluster.ConnectAsync(
            _config["Couchbase:ConnectionString"],
            _config["Couchbase:Username"],
            _config["Couchbase:Password"]);

        var allBuckets = await _cluster.Buckets.GetAllBucketsAsync();
        var doesBucketExist = allBuckets.Any(b => b.Key == _config["Couchbase:BucketName"]);
        if (!doesBucketExist)
            throw new Exception($"There is no bucket for integration testing named '{_config["Couchbase:BucketName"]}'.");

        _bucket = await _cluster.BucketAsync(_config["Couchbase:BucketName"]);

        // *** nosql migrations
        var runner = new MigrationRunner();
        // run the migrations down just to be safe
        var downSettings = new RunSettings();
        downSettings.Direction = DirectionEnum.Down;
        downSettings.Bucket = _bucket;
        await runner.Run(typeof(CreateUserCollectionInDefaultScope).Assembly, downSettings);

        // run the migrations
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
        ServiceCollection = services;
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        // run the migrations down to clean up
        var runner = new MigrationRunner();
        var downSettings = new RunSettings();
        downSettings.Direction = DirectionEnum.Down;
        downSettings.Bucket = _bucket;
        await runner.Run(typeof(CreateUserCollectionInDefaultScope).Assembly, downSettings);

        // dispose couchbase services
        await _cluster.DisposeAsync();
        await ServiceProvider.GetRequiredService<ICouchbaseLifetimeService>().CloseAsync();
    }
}