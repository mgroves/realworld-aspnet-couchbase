using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Conduit.Web.Articles.Services;
using Conduit.Web.Follows.Services;
using Conduit.Web.Users.Handlers;
using Conduit.Web.Users.Services;
using Couchbase.Extensions.DependencyInjection;
using FluentValidation;
using Microsoft.OpenApi.Models;
using Conduit.Web.DataAccess.Providers;
using Slugify;

namespace Conduit.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddConduitSwaggerSetup();

            builder.Services.AddConduitAuthenticationSetup(builder.Configuration);

            builder.Services.AddConduitServiceDependencies(builder.Configuration);

            // ****************************************************

            var app = builder.Build();

            app.UseAuthentication();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();

            // Add the following line to close the Couchbase connection inside the app.Run() method at the end of Program.cs
            app.Lifetime.ApplicationStopped.Register(() =>
            {
                app.Services.GetRequiredService<ICouchbaseLifetimeService>().Close();
            });
        }
    }

    public static class ConduitServiceSetupExtension
    {
        /// <summary>
        /// Services, validators, database
        /// </summary>
        public static void AddConduitServiceDependencies(this IServiceCollection @this, ConfigurationManager configManager)
        {
            @this.AddTransient<ISlugHelper, SlugHelper>();
            @this.AddTransient<ISlugService, SlugService>();
            @this.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
            @this.AddTransient(typeof(SharedUserValidator<>));
            @this.AddTransient<IAuthService, AuthService>();
            @this.AddTransient<IFollowDataService, FollowsDataService>();
            @this.AddTransient<IUserDataService, UserDataService>();
            @this.AddTransient<ITagsDataService, TagsDataService>();
            @this.AddTransient<IArticlesDataService, ArticlesDataService>();
            @this.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
            @this.AddCouchbase(configManager.GetSection("Couchbase"));
            @this.AddCouchbaseBucket<IConduitBucketProvider>(configManager["Couchbase:BucketName"], b =>
            {
                b
                    .AddScope(configManager["Couchbase:ScopeName"])
                    .AddCollection<IConduitUsersCollectionProvider>(configManager["Couchbase:UsersCollectionName"]);
                b
                    .AddScope(configManager["Couchbase:ScopeName"])
                    .AddCollection<IConduitFollowsCollectionProvider>(configManager["Couchbase:FollowsCollectionName"]);
                b
                    .AddScope(configManager["Couchbase:ScopeName"])
                    .AddCollection<IConduitTagsCollectionProvider>(configManager["Couchbase:TagsCollectionName"]);
                b
                    .AddScope(configManager["Couchbase:ScopeName"])
                    .AddCollection<IConduitArticlesCollectionProvider>(configManager["Couchbase:ArticlesCollectionName"]);
                b
                    .AddScope(configManager["Couchbase:ScopeName"])
                    .AddCollection<IConduitFavoritesCollectionProvider>("Favorites");
            });
        }
    }

    public static class AuthenticationSetupExtensions
    {
        /// <summary>
        /// Add JWT authentication
        /// </summary>
        public static void AddConduitAuthenticationSetup(this IServiceCollection @this,
            ConfigurationManager config)
        {
            @this.Configure<JwtSecrets>(config.GetSection("JwtSecrets"));

            @this.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = config["JwtSecrets:Issuer"], 
                        ValidAudience = config["JwtSecrets:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes( config["JwtSecrets:SecurityKey"]))
                    };
                    options.Events = new JwtBearerEvents()
                    {
                        // this code strips out "Token" from the header
                        // due to Conduit spec saying "Token" instead of "Bearer"
                        OnMessageReceived = ctx =>
                        {
                            if (ctx.Request.Headers.ContainsKey("Authorization"))
                            {
                                var bearerToken = ctx.Request.Headers["Authorization"].ElementAt(0);
                                var token = bearerToken.StartsWith("Token ") ? bearerToken.Substring(6) : bearerToken;
                                ctx.Token = token;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
        }
    }

    public static class SwaggerSetupExtensions
    {
        /// <summary>
        /// Add OpenAI generation, setup swagger/swashbuckle
        /// </summary>
        public static void AddConduitSwaggerSetup(this IServiceCollection @this)
        {
            @this.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Conduit API", Version = "v1" });

                // use the XMLDoc generated by dotnet to document the endpoints in OpenAPI
                var xmlFile = Directory.GetFiles(AppContext.BaseDirectory, "*.xml").FirstOrDefault();
                c.IncludeXmlComments(xmlFile);

                // Configure JWT authentication
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,   // using this instead of JWT because of Swashbuckle issue, see: https://stackoverflow.com/questions/76448339/changing-bearer-to-something-else-in-the-header-like-token-for-jwt
                    Scheme = "Token",                   // This would normally be "Bearer"
                    BearerFormat = "JWT"
                };

                // These next two statements enable bearer token
                // auth when using the SwaggerUI
                c.AddSecurityDefinition("Token", securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Token"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }
    }
}
