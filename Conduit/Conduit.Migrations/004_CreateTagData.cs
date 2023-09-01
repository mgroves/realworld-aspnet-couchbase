using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

// Manual alternative: create a Tags collection
// and create a single document with key of "tagData"
// that contains an array with all the tags you want to allow
[Migration(4)]
public class CreateTagData : MigrateBase
{
    private readonly string? _scopeName;

    public CreateTagData()
    {
        _scopeName = _config["Couchbase:ScopeName"];
    }

    public override void Up()
    {
        Create.Collection("Tags")
            .InScope(_scopeName);

        Insert.Into
            .Scope(_scopeName)
            .Collection("Tags")
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
        Delete.Collection("Tags")
            .FromScope(_scopeName);
    }
}