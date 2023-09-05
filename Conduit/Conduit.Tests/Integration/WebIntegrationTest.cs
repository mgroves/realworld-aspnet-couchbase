using Conduit.Migrations;
using Conduit.Web;
using Conduit.Web.Users.Services;
using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NoSqlMigrator.Runner;

namespace Conduit.Tests.Integration;

public abstract class WebIntegrationTest
{
    protected WebApplicationFactory<Program> WebAppFactory;
    protected HttpClient WebClient;
    protected IAuthService AuthSvc;

    private ICluster _cluster;
    private IBucket _bucket;

    [OneTimeSetUp]
    public virtual async Task Setup()
    {
        WebAppFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    // Add environment variables
                    configBuilder.AddEnvironmentVariables();
                    configBuilder.AddUserSecrets<WebIntegrationTest>();
                });
            });

        WebClient = WebAppFactory.CreateClient();

        var clusterProvider = WebAppFactory.Services.GetService<IClusterProvider>();
        _cluster = await clusterProvider.GetClusterAsync();
        var config = WebAppFactory.Services.GetService<IConfiguration>();

        await _cluster.WaitUntilReadyAsync(TimeSpan.FromSeconds(30));
        
        var allBuckets = await _cluster.Buckets.GetAllBucketsAsync();
        var doesBucketExist = allBuckets.Any(b => b.Key == config["Couchbase:BucketName"]);
        if (!doesBucketExist)
            throw new Exception($"There is no bucket for integration testing named '{config["Couchbase:BucketName"]}'.");
        
        _bucket = await _cluster.BucketAsync(config["Couchbase:BucketName"]);
        
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

        // setup authservice
        AuthSvc = WebAppFactory.Services.GetService<IAuthService>();
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
        await WebAppFactory.Services.GetService<ICouchbaseLifetimeService>().CloseAsync();

        // clean up web test stuff
        WebClient.Dispose();
        await WebAppFactory.DisposeAsync();
    }
}