using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

// Manual alternative: TODO
[Migration(9)]
public class CreateAiBotUser : MigrateBase
{
    private readonly string? _scopeName;

    public CreateAiBotUser()
    {
        _scopeName = _config["Couchbase:ScopeName"];
    }

    public override void Up()
    {
        Insert.Into
            .Scope(_scopeName)
            .Collection("Users")
            .Document("summary-bot", new
            {
                bio = "I'm an AI bot that generates adaptive content.",
                email = "summary-bot@example.net",
                image = "https://picsum.photos/seed/summary-bot/512"
            });
    }

    public override void Down()
    {
        Delete.From
            .Scope(_scopeName)
            .Collection("Users")
            .Document("summary-bot");
    }
}