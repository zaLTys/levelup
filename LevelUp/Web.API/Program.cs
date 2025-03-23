using DataAccessLayer;
using DataAccessLayer.ManualMigrations.VersionMetadata;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Web.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.AddConsole();

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("Postgres");
            builder.Services.AddDbContext<MyDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                //.EnableSensitiveDataLogging() // Optional, logs parameter values
                //.LogTo(Console.WriteLine, LogLevel.Information);
            });
            
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://localhost:5001";
                    options.ClientId = "efcorewebapiclient";
                    options.ClientSecret = "efcorewebapiclient-secret";
                    options.ResponseType = "code";
                    options.SaveTokens = true;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://localhost:5001";
                    options.Audience = "efcorewebapiclient";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidateIssuer = true
                    };
                });
            
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler =
                        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("https://localhost:5001/connect/authorize"),
                            TokenUrl = new Uri("https://localhost:5001/connect/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "OpenID scope" },
                                { "profile", "User profile scope" },
                                { "api.read", "Read access to API" },
                                { "api.write", "Write access to API" }
                            }
                        }
                    }
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        },
                        new[] { "api.read", "api.write" }
                    }
                });
            });
            
            //Fluent migrator
            builder.Services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddPostgres()
                    .WithGlobalConnectionString(connectionString)
                    .WithGlobalCommandTimeout(TimeSpan.FromMinutes(15))
                    .ScanIn(typeof(MyDbContext).Assembly).For.Migrations())
                .AddScoped<IMigrationRunner, MigrationRunner>()
                .AddScoped<IVersionTableMetaData, CustomVersionTableMetaData>()
                .Configure<RunnerOptions>(opt =>
                {
                    opt.Tags = new[] { "Postgres" };
                });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
            
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var dbContext = services.GetRequiredService<MyDbContext>();
                    dbContext.Database.Migrate();

                    ListMigrations(dbContext);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
                }
            }
            
            //Migration control
            using (var scope = app.Services.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                var configuration = app.Services.GetRequiredService<IConfiguration>();
                var migrateTo = configuration["ManualMigrations:MigrateTo"] ?? string.Empty;

                if (string.IsNullOrWhiteSpace(migrateTo))
                {
                    Console.WriteLine("No migration action taken.");
                    return;
                }

                if (migrateTo.Equals("latest", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Applying latest migrations...");
                    runner.MigrateUp();
                }
                else if (long.TryParse(migrateTo, out long targetVersion))
                {
                    Console.WriteLine($"Rolling back to migration version {targetVersion}...");
                    runner.RollbackToVersion(targetVersion);
                }
                else
                {
                    Console.WriteLine($"Invalid migration target: {migrateTo}. No action taken.");
                }
            }
            
            app.UseCors("AllowAllOrigins");

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API v1");

                c.OAuthClientId("efcorewebapiclient");
                c.OAuthClientSecret("efcorewebapiclient-secret");
                c.OAuthUsePkce(); // Enables PKCE flow
                c.OAuth2RedirectUrl("https://localhost:7285/swagger/oauth2-redirect.html");
            });

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();

            
        }
        
        private static void ListMigrations(MyDbContext myDbContext)
        {
            var migrations = myDbContext.Database.GetAppliedMigrations();
            foreach (var migration in migrations)
            {
                Console.WriteLine($"Applied Migration: {migration}");
            }
        }
    }
}