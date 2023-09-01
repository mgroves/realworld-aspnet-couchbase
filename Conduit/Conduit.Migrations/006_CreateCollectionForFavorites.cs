using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

// Manual alternative: create a Favorites collection in _default scope
[Migration(6)]
public class CreateCollectionForFavorites : MigrateBase
{
    private readonly string? _scopeName;

    public CreateCollectionForFavorites()
    {
        _scopeName = _config["Couchbase:ScopeName"];
    }

    public override void Up()
    {
        Create.Collection("Favorites")
            .InScope(_scopeName);
    }

    public override void Down()
    {
        Delete.Collection("Favorites")
            .FromScope(_scopeName);
    }
}