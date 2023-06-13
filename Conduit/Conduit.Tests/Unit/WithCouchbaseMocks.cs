using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Moq;

namespace Conduit.Tests.Unit;

public abstract class WithCouchbaseMocks
{
    protected Mock<IBucketProvider> BucketProviderMock;
    protected Mock<IBucket> BucketMock;
    protected Mock<ICouchbaseCollection> CollectionMock;

    [SetUp]
    public virtual void SetUp()
    {
        BucketProviderMock = new Mock<IBucketProvider>();
        BucketMock = new Mock<IBucket>();
        CollectionMock = new Mock<ICouchbaseCollection>();
        
        BucketProviderMock.Setup(b => b.GetBucketAsync(It.IsAny<string>()))
            .ReturnsAsync(BucketMock.Object);
        BucketMock.Setup(b => b.CollectionAsync(It.IsAny<string>()))
            .ReturnsAsync(CollectionMock.Object);
    }    
}