using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

[Migration(4)]
public class CreateTagData : MigrateBase
{
    private readonly string? _collectionName;
    private readonly string? _scopeName;

    public CreateTagData()
    {
        _collectionName = _config["Couchbase:TagsCollectionName"];
        _scopeName = _config["Couchbase:ScopeName"];
    }

    public override void Up()
    {
        Create.Collection(_collectionName)
            .InScope(_scopeName);

        Insert.Into
            .Scope(_scopeName)
            .Collection(_collectionName)
            .Document("tagData", new
            {
                tags = new List<string>
                {
                    "Couchbase", "NoSQL", ".NET", "cruising", "baseball"
                }
            });
    }

    public override void Down()
    {
        Delete.Collection(_collectionName)
            .FromScope(_scopeName);
    }
}