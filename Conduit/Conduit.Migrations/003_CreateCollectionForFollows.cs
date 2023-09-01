using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

// Manual alternative: create a Follows collection in _default scope
[Migration(3)]
public class CreateCollectionForFollows : MigrateBase
{
    private readonly string? _scopeName;

    public CreateCollectionForFollows()
    {
        _scopeName = _config["Couchbase:ScopeName"];
    }

    public override void Up()
    {
        Create.Collection("Follows")
            .InScope(_scopeName);
    }

    public override void Down()
    {
        Delete.Collection("Follows")
            .FromScope(_scopeName);
    }
}