using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

[Migration(2)]
public class CreateIndexOnUsernameFieldInUsersCollection : Migrate
{
    public override void Up()
    {
        Create.Index("ix_users_username")
            .OnDefaultScope()
            .OnCollection("Users")
            .OnField("username");
    }

    public override void Down()
    {
        Delete.Index("ix_users_username")
            .FromScope("_default")
            .FromCollection("Users");
    }
}