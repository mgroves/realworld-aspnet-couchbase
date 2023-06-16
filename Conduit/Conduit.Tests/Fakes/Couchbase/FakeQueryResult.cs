using Couchbase.Core.Retry;
using Couchbase.Query;

namespace Conduit.Tests.Fakes.Couchbase;

public class FakeQueryResult<T> : IQueryResult<T>
{
    public FakeQueryResult(List<T> fakeData)
    {
        Rows = fakeData.ToAsyncEnumerable();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        return Rows.GetAsyncEnumerator(cancellationToken);
    }

    public RetryReason RetryReason => throw new NotImplementedException();

    public IAsyncEnumerable<T> Rows { get; private set; }

    public QueryMetaData? MetaData => throw new NotImplementedException();
    public List<Error> Errors => throw new NotImplementedException();
}