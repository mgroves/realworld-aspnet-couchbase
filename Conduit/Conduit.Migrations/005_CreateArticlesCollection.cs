using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

[Migration(5)]
public class CreateArticlesCollection : MigrateBase
{
    private readonly string? _collectionName;
    private readonly string? _scopeName;

    public CreateArticlesCollection()
    {
        _collectionName = _config["Couchbase:ArticlesCollectionName"];
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