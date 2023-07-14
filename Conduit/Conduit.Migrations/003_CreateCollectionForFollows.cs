using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

[Migration(3)]
public class CreateCollectionForFollows : MigrateBase
{
    private readonly string? _collectionName;
    private readonly string? _scopeName;

    public CreateCollectionForFollows()
    {
        _collectionName = _config["Couchbase:FollowsCollectionName"];
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