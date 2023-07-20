using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

// Manual alternative: CREATE INDEX ix_users_email ON <bucketname>._default.Users (email)
[Migration(2)]
public class CreateIndexOnEmailFieldInUsersCollection : MigrateBase
{
    private readonly string? _collectionName;
    private readonly string? _scopeName;

    public CreateIndexOnEmailFieldInUsersCollection()
    {
        _collectionName = _config["Couchbase:UsersCollectionName"];
        _scopeName = _config["Couchbase:ScopeName"];
    }

    public override void Up()
    {
        Create.Index("ix_users_email")
            .OnScope(_scopeName)
            .OnCollection(_collectionName)
            .OnField("email");
    }

    public override void Down()
    {
        Delete.Index("ix_users_email")
            .FromScope(_scopeName)
            .FromCollection(_collectionName);
    }
}