using Conduit.Migrations;
using Conduit.Web.Users.Services;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using NoSqlMigrator.Runner;
using Conduit.Web;
using Couchbase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Functional;

/// <summary>
/// This look weird to you?
/// It's NUnit's way of running something before ALL tests, just once
/// and then running something after ALL tests, just once.
/// This DRAMATICALLY speeds up the web integration tests--
/// no need to migrate up/down every time
/// no need to reinstantiate WebAppFactory and client every time
/// So I'm keeping this, unless something better comes along
/// The only weirdness are the static properties, which should only be used
/// in the WebIntergrationTest base class
///
/// </summary>
[SetUpFixture]
public class GlobalFunctionalSetUp
{
    public static WebApplicationFactory<Program> WebAppFactory { get; private set; }
    public static HttpClient WebClient { get; private set; }
    public static IAuthService AuthSvc { get; private set; }

    private ICluster _cluster;
    private IBucket _bucket;

    [OneTimeSetUp]
    public async Task RunBeforeAllTests()
    {
        WebAppFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    // Add environment variables
                    configBuilder.AddEnvironmentVariables();
                    configBuilder.AddUserSecrets<GlobalFunctionalSetUp>();
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
    public async Task RunAfterAllTests()
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