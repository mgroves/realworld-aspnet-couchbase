using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

// Manual alternative: create a Favorites collection in _default scope
[Migration(8)]
public class CreateCollectionForComments: MigrateBase
{
    private readonly string? _scopeName;

    public CreateCollectionForComments()
    {
        _scopeName = _config["Couchbase:ScopeName"];
    }

    public override void Up()
    {
        Create.Collection("Comments")
            .InScope(_scopeName);
    }

    public override void Down()
    {
        Delete.Collection("Comments")
            .FromScope(_scopeName);
    }
}