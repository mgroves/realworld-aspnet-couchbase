using Microsoft.Extensions.Configuration;
using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations
{
    [Migration(1)]
    public class CreateUserCollectionInDefaultScope : MigrateBase
    {
        private readonly string? _collectionName;
        private readonly string? _scopeName;

        public CreateUserCollectionInDefaultScope()
        {
            _collectionName = _config["Couchbase:UsersCollectionName"];
            _scopeName = _config["Couchbase:ScopeName"];
        }

        public override void Up()
        {
            Create.Collection(_collectionName)
                .InScope(_scopeName);
        }

        public override void Down()
        {
            Delete.Collection(_collectionName)
                .FromScope(_scopeName);
        }
    }
}