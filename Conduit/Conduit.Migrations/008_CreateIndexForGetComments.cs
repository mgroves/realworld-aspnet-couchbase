using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

// Manual alternative: CREATE INDEX `idx_comments_author_follows2` ON `Conduit`.`_default`.`Comments`((meta().`id`),(distinct (array (`c`.`authorUsername`) for `c` in ((`Conduit`.`_default`).`Comments`) end)))
[Migration(8)]
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
            .OnFieldRaw("DISTINCT (array (`c`.`authorUsername`) for `c` in ((`Conduit`.`_default`).`Comments`");
    }

    public override void Down()
    {
        Delete.Index("ix_get_comments")
            .FromScope(_scopeName)
            .FromCollection("Comments");
    }
}