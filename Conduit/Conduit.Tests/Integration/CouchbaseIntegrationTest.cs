using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tests.Integration;

public abstract class CouchbaseIntegrationTest
{
    protected ServiceProvider ServiceProvider;

    [OneTimeSetUp]
    public virtual async Task Setup()
    {
        ServiceProvider = GlobalCouchbaseIntegrationSetUp.ServiceProvider;
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        // tear down happens in GlobalCouchbaseIntegrationSetUp
    }
}