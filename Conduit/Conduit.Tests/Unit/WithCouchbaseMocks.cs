using Conduit.Web.Models;
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
    protected Mock<IConduitUsersCollectionProvider> UsersCollectionProviderMock;
    protected Mock<ICouchbaseCollection> UsersCollectionMock;
    protected Mock<IScope> ScopeMock { get; set; }

    [SetUp]
    public virtual void SetUp()
    {
        ScopeMock = new Mock<IScope>();
        BucketProviderMock = new Mock<IBucketProvider>();
        ClusterProviderMock = new Mock<IClusterProvider>();
        ClusterMock = new Mock<ICluster>();
        BucketMock = new Mock<IBucket>();
        CollectionMock = new Mock<ICouchbaseCollection>();
        UsersCollectionProviderMock = new Mock<IConduitUsersCollectionProvider>();
        UsersCollectionMock = new Mock<ICouchbaseCollection>();

        BucketProviderMock.Setup(b => b.GetBucketAsync(It.IsAny<string>()))
            .ReturnsAsync(BucketMock.Object);
        ClusterProviderMock.Setup(c => c.GetClusterAsync())
            .ReturnsAsync(ClusterMock.Object);
        ClusterMock.Setup(cl => cl.BucketAsync(It.IsAny<string>()))
            .ReturnsAsync(BucketMock.Object);
        BucketMock.Setup(b => b.CollectionAsync(It.IsAny<string>()))
            .ReturnsAsync(CollectionMock.Object);
        BucketMock.Setup(m => m.Cluster)
            .Returns(ClusterMock.Object);

        UsersCollectionMock.Setup(m => m.Scope)
            .Returns(ScopeMock.Object);
        ScopeMock.Setup(m => m.Bucket)
            .Returns(BucketMock.Object);
        UsersCollectionProviderMock.Setup(u => u.GetCollectionAsync())
            .ReturnsAsync(UsersCollectionMock.Object);
    }

}
