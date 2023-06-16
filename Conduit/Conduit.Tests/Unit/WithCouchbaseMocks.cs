using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Moq;

namespace Conduit.Tests.Unit;

public abstract class WithCouchbaseMocks
{
    protected Mock<IBucketProvider> BucketProviderMock;
    protected Mock<IClusterProvider> ClusterProviderMock;
    protected Mock<ICluster> ClusterMock;
    protected Mock<IBucket> BucketMock;
    protected Mock<ICouchbaseCollection> CollectionMock;

    [SetUp]
    public virtual void SetUp()
    {
        BucketProviderMock = new Mock<IBucketProvider>();
        ClusterProviderMock = new Mock<IClusterProvider>();
        ClusterMock = new Mock<ICluster>();
        BucketMock = new Mock<IBucket>();
        CollectionMock = new Mock<ICouchbaseCollection>();

        BucketProviderMock.Setup(b => b.GetBucketAsync(It.IsAny<string>()))
            .ReturnsAsync(BucketMock.Object);
        ClusterProviderMock.Setup(c => c.GetClusterAsync())
            .ReturnsAsync(ClusterMock.Object);
        ClusterMock.Setup(cl => cl.BucketAsync(It.IsAny<string>()))
            .ReturnsAsync(BucketMock.Object);
        BucketMock.Setup(b => b.Collection(It.IsAny<string>()))
            .Returns(CollectionMock.Object);
    }
}
