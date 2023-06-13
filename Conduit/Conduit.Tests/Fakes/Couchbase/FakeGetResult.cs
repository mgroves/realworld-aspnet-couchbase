using Couchbase.KeyValue;

namespace Conduit.Tests.Fakes.Couchbase;

public class FakeGetResult : IGetResult
{
    private readonly object _content;

    public FakeGetResult(object content)
    {
        _content = content;
    }
    
    public ulong Cas => throw new NotImplementedException();

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public T? ContentAs<T>()
    {
        return (T)_content;
    }

    public TimeSpan? Expiry { get; }
    public DateTime? ExpiryTime { get; }
}