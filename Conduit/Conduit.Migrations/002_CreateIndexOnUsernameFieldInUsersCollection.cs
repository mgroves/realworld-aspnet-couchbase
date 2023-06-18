using Microsoft.Extensions.Configuration;
using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations;

[Migration(2)]
public class CreateIndexOnUsernameFieldInUsersCollection : MigrateBase
{
    private readonly string? _collectionName;
    private readonly string? _scopeName;

    public CreateIndexOnUsernameFieldInUsersCollection()
    {
        _collectionName = _config["Couchbase:UsersCollectionName"];
        _scopeName = _config["Couchbase:ScopeName"];
    }

    public override void Up()
    {
        Create.Index("ix_users_username")
            .OnScope(_scopeName)
            .OnCollection(_collectionName)
            .OnField("username");
    }

    public override void Down()
    {
        Delete.Index("ix_users_username")
            .FromScope(_scopeName)
            .FromCollection(_collectionName);
    }
}