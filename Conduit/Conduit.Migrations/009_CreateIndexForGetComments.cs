using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

// Manual alternative: CREATE INDEX `ix_get_comments` ON `Conduit`.`_default`.`Comments`(META().id, DISTINCT ARRAY c.authorUsername FOR c IN c2 END);
[Migration(9)]
public class CreateIndexForGetComments : MigrateBase
{
    private readonly string? _scopeName;

    public CreateIndexForGetComments()
    {
        _scopeName = _config["Couchbase:ScopeName"];
    }

    public override void Up()
    {
        // there are other options for more complex and/or covering indexes
        // but this index is the simplest for the List Articles SQL++ query
        Create.Index("ix_get_comments")
            .OnScope(_scopeName)
            .OnCollection("Comments")
            .OnFieldRaw("META().`id`")
            .OnFieldRaw($"DISTINCT ARRAY c.authorUsername FOR c IN c2 END");
    }

    public override void Down()
    {
        Delete.Index("ix_get_comments")
            .FromScope(_scopeName)
            .FromCollection("Comments");
    }
}