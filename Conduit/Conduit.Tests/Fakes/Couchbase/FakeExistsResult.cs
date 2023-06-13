using Couchbase.KeyValue;

namespace Conduit.Tests.Fakes.Couchbase;

public class FakeExistsResult : IExistsResult
{
    public FakeExistsResult(bool exists)
    {
        Exists = exists;
    }
    
    public ulong Cas => throw new NotImplementedException();

    public bool Exists { get; private set; }
}