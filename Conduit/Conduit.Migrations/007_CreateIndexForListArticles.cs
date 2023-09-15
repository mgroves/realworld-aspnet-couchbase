using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

// Manual alternative: CREATE INDEX `ix_articles_authorUsername` ON `Conduit`.`_default`.`Articles`(`authorUsername`)
[Migration(7)]
public class CreateIndexForListArticles : MigrateBase
{
    private readonly string? _scopeName;

    public CreateIndexForListArticles()
    {
        _scopeName = _config["Couchbase:ScopeName"];
    }

    public override void Up()
    {
        // there are other options for more complex and/or covering indexes
        // but this index is the simplest for the List Articles SQL++ query
        Create.Index("ix_articles_authorUsername")
            .OnScope(_scopeName)
            .OnCollection("Articles")
            .OnField("authorUsername");
    }

    public override void Down()
    {
        Delete.Index("ix_articles_authorUsername")
            .FromScope(_scopeName)
            .FromCollection("Articles");
    }
}