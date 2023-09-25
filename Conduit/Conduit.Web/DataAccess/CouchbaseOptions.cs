using Couchbase.Query;

namespace Conduit.Web.DataAccess;

public class CouchbaseOptions
{
    public QueryScanConsistency ScanConsistency { get; set; }
}