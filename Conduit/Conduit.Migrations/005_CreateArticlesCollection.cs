using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

// Manual alternative: create a Articles collection in _default scope
[Migration(5)]
public class CreateArticlesCollection : MigrateBase
{
    private readonly string? _scopeName;

    public CreateArticlesCollection()
    {
        _scopeName = _config["Couchbase:ScopeName"];
    }

    public override void Up()
    {
        Create.Collection("Articles")
            .InScope(_scopeName);
    }

    public override void Down()
    {
        Delete.Collection("Articles")
            .FromScope(_scopeName);
    }
}