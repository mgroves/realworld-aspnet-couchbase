using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

// Manual alternative: Create a Users collection in _default scope
[Migration(1)]
public class CreateUserCollectionInDefaultScope : MigrateBase
{
    private readonly string? _scopeName;

    public CreateUserCollectionInDefaultScope()
    {
        _scopeName = _config["Couchbase:ScopeName"];
    }

    public override void Up()
    {
        Create.Collection("Users")
            .InScope(_scopeName);
    }

    public override void Down()
    {
        Delete.Collection("Users")
            .FromScope(_scopeName);
    }
}
