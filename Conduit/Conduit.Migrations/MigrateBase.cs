using Microsoft.Extensions.Configuration;
using NoSqlMigrator.Infrastructure;
using System.Reflection;

namespace Conduit.Migrations;


public abstract class MigrateBase : Migrate
{
    protected readonly IConfigurationRoot _config;

    protected MigrateBase()
    {
        var builder = new ConfigurationBuilder();

        // *** this feels SUPER hacky ***
        // the goal is to reuse the config from Conduit.Web
        // so that stuff like scope name and collection names can be used in this migrations
        // perhaps environment variables will be better suited in production?
        // ******************************

        try
        {
            // get path that this assembly is in
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var fullPathFolder = Path.GetDirectoryName(path);

            // navigate back through folders to get to Conduit.Web
            // this assumes that the Conduit.Migrations dll will always be
            // in \Conduit\Conduit.Migrations\bin\Debug\net7.0
            // (or \Conduit\Conduit.Migrations\bin\Release\net7.0)
            var webFolder = Path.Combine(fullPathFolder, "..\\..\\..\\..\\Conduit.Web\\");

            builder.SetBasePath(webFolder)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.Development.json", optional: true);
        }
        catch
        {
            // we're probably running on github workflow, so the above hack won't work
        }

        _config = builder
            .AddEnvironmentVariables()
            .Build();
    }
}
