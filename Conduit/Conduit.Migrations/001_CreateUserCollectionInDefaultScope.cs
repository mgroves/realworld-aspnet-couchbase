using NoSqlMigrator.Infrastructure;

namespace Conduit.Migrations
{
    [Migration(1)]
    public class CreateUserCollectionInDefaultScope : Migrate
    {
        public override void Up()
        {
            Create.Collection("Users")
                .InScope("_default");
        }

        public override void Down()
        {
            Delete.Collection("Users")
                .FromDefaultScope();
        }
    }
}