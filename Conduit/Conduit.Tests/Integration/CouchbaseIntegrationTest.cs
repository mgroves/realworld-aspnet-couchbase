using System.Net.Http.Headers;
using System.Text;
using Conduit.Migrations;
using Couchbase;
using Couchbase.Diagnostics;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Management.Buckets;
using Microsoft.Extensions.DependencyInjection;
using NoSqlMigrator.Runner;
using Testcontainers.Couchbase;

namespace Conduit.Tests.Integration;

public abstract class CouchbaseIntegrationTest
{
    protected CouchbaseContainer CouchbaseTestContainer;
    protected ServiceCollection ServiceCollection;

    [OneTimeSetUp]
    public virtual async Task Setup()
    {
        CouchbaseTestContainer = new CouchbaseBuilder()
            .WithImage("couchbase/server:7.2.0")
            .Build();

        await CouchbaseTestContainer.StartAsync();

        var cluster = await Cluster.ConnectAsync(CouchbaseTestContainer.GetConnectionString(), "Administrator", "password");

        // don't need the built-in bucket
        var existingBucketName = CouchbaseTestContainer.Buckets.First().Name;
        var bucketManager = cluster.Buckets;
        await bucketManager.DropBucketAsync(existingBucketName);

        // create "Conduit" bucket that works with Migrations
        // TODO: get this bucket name from appsettings/environment variable
        await bucketManager.CreateBucketAsync(new BucketSettings()
        {
            Name = "Conduit",
            BucketType = BucketType.Couchbase,
            NumReplicas = 0,
            RamQuotaMB = 100
        });

        // make sure new bucket is ready
        // var bucketProvider = ServiceProvider.GetRequiredService<IConduitBucketProvider>();
        // var bucket = await bucketProvider.GetBucketAsync();
        var bucket = await cluster.BucketAsync("Conduit");
        await bucket.WaitUntilReadyAsync(TimeSpan.FromSeconds(30));

        await WaitForQueryService(bucket);
        await EnsureIndexEngineSet();

        // since this is a brand new couchbase instance
        // that only has a bucket and _default scope/collection
        // run the migrations
        var runner = new MigrationRunner();
        var settings = new RunSettings();
        settings.Direction = DirectionEnum.Up;
        settings.Bucket = bucket;
        await runner.Run(typeof(CreateUserCollectionInDefaultScope).Assembly, settings);

        await cluster.DisposeAsync();

        await BuildServiceCollection();
    }

    private async Task BuildServiceCollection()
    {
        // create a DI setup, in order to get Couchbase DI objects 
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCouchbase(options =>
        {
            options.ConnectionString = CouchbaseTestContainer.GetConnectionString();
            options.UserName = "Administrator";
            options.Password = "password";
        });
        ServiceCollection = services;
    }

    private async Task EnsureIndexEngineSet()
    {
        var httpClient = new HttpClient();
        var url = $"http://{CouchbaseTestContainer.Hostname}:{CouchbaseTestContainer.GetMappedPublicPort(8091)}/settings/indexes";
        var data = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "storageMode", "plasma" }
        });
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("Administrator:password")));
        var response = await httpClient.PostAsync(url, data);
        var responseBody = await response.Content.ReadAsStringAsync();
    }

    private async Task WaitForQueryService(IBucket bucket)
    {
        bool queryServiceReady = false;

        do
        {
            // Use PingAsync to get the status of all services in the bucket
            var report = await bucket.PingAsync();

            var queryServices = report.Services
                .Where(s => s.Key == "n1ql");

            if (queryServices.Any())
            {
                var queryService = queryServices.First();
                var endpoints = queryService.Value;
                queryServiceReady = endpoints.Any(e => e.State == ServiceState.Ok);
            }

            // Wait for a while before checking again if services are not ready yet
            if (!queryServiceReady)
                Thread.Sleep(5000);
        } while (!queryServiceReady);
    }

    [OneTimeTearDown]
    public async Task Teardown()
    {
        await CouchbaseTestContainer.StopAsync();
    }
}