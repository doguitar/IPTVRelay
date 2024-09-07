using CommandLine;
using Hangfire;
using Hangfire.Storage.SQLite;
using IPTVRelay.Blazor.Components;
using IPTVRelay.Blazor.Configuration;
using IPTVRelay.Database;
using IPTVRelay.Database.Enums;
using IPTVRelay.Database.Psql;
using IPTVRelay.Database.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Xml.Linq;

namespace IPTVRelay.Blazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables("IPTV_");

            var config = new IPTVConfig();
            Parser.Default.ParseArguments(() => config, args);


            configBuilder.Build().Bind(config, o =>
            {
                o.ErrorOnUnknownConfiguration = false;
                o.BindNonPublicProperties = true;
            });

            var configDirectory = new DirectoryInfo(config.ConfigFolder);
            if (configDirectory.Exists)
            {
                foreach (var file in configDirectory.GetFiles("*.json").OrderByDescending(f => f.FullName.Length))
                {
                    configBuilder.AddJsonFile(file.FullName);
                }
            }
            Parser.Default.ParseArguments(() => config, args);

            configBuilder.Build().Bind(config, o =>
            {
                o.ErrorOnUnknownConfiguration = false;
                o.BindNonPublicProperties = true;
            });

            var dataDirectory = new DirectoryInfo(config.DataFolder);
            if (!dataDirectory.Exists) dataDirectory.Create();


            var builder = WebApplication.CreateBuilder(args);

            var hangireDb = Path.Combine(dataDirectory.FullName, "hangfire.db");

            builder.Services
                .AddHangfire(config => config.UseSQLiteStorage(hangireDb,
                new SQLiteStorageOptions
                {
                    AutoVacuumSelected = SQLiteStorageOptions.AutoVacuum.INCREMENTAL,
                    Prefix = "hangfire:"
                }))
                .AddHangfireServer()
                .AddSingleton(config)
                .AddBlazorBootstrap()
                .AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            switch (config.DbConfig.Provider)
            {
                case DbProvider.Psql:
                    builder.Services.AddDbContext<IPTVRelayContext, IPTVRelayNpgsqlContext>(options =>
                    {
                        options.UseNpgsql(
                            new NpgsqlConnectionStringBuilder
                            {
                                Host = config.DbConfig.PsqlConfig.Host,
                                Port = config.DbConfig.PsqlConfig.Port,
                                Username = config.DbConfig.PsqlConfig.Username,
                                Password = config.DbConfig.PsqlConfig.Password,
                                Database = config.DbConfig.PsqlConfig.Database
                            }.ConnectionString,
                            x => x.MigrationsAssembly(Constants.PSQL_MIGRATIONS));
                    });
                    break;
                case DbProvider.Sqlite:
                    builder.Services.AddDbContext<IPTVRelayContext, IPTVRelaySqliteContext>(options =>
                    {
                        options.UseSqlite(
                            new SqliteConnectionStringBuilder
                            {
                                DataSource = Path.Combine(dataDirectory.FullName, config.DbConfig.SqliteConfig.Path)
                            }.ConnectionString,
                            x => x.MigrationsAssembly(Constants.SQLITE_MIGRATIONS));
                    });
                    break;

                default:
                    break;
            }


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            // app.UseHangfireServer(); // Remove this line

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                //.AddAdditionalAssemblies(typeof(Client._Imports).Assembly)
                ;

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IPTVRelayContext>();
                db.Database.Migrate();

                app.Urls.Clear();
                app.Urls.Add($"http://{config.Host}:{config.Port}");

                var settings = await db.Setting.ToDictionaryAsync(s => s.Name, s => s.Value);
                settings.TryGetValue(SettingType.UPDATE_CRON.ToString(), out var updateCron);

                var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
                recurringJobManager.AddOrUpdate<Utility.Jobs.UpdateJob>(
                    "UPDATE",
                    (j) => j.Update(),
                    !string.IsNullOrWhiteSpace(updateCron) ? updateCron : Cron.Daily(DateTime.Now.Hour, DateTime.Now.Minute));
            }

            if (dataDirectory != null)
            {

                if (!dataDirectory.Exists) dataDirectory.Create();

                var staticFileDirectories = new[] { "images", "output", "m3u", "xmltv" };

                foreach (var directory in staticFileDirectories)
                {
                    var staticDirectory = new DirectoryInfo(Path.Combine(dataDirectory.FullName, directory));
                    if (!staticDirectory.Exists) staticDirectory.Create();

                    app.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(Path.Combine(dataDirectory.FullName, directory)),
                        RequestPath = $"/{directory}",
                        OnPrepareResponseAsync = async ctx =>
                        {
                            using var scope = app.Services.CreateScope();

                            var db = scope.ServiceProvider.GetRequiredService<IPTVRelayContext>();
                            var settings = await db.Setting.ToDictionaryAsync(s => s.Name, s => s.Value);

                            settings.TryGetValue(SettingType.API_KEY.ToString(), out var apiKey);
                            if (string.IsNullOrWhiteSpace(apiKey) ||
                                !(ctx.Context.Request.Query.TryGetValue("apikey", out var requestKey) || ctx.Context.Request.Headers.TryGetValue("API-Key", out requestKey)) ||
                                !apiKey.Equals(requestKey))
                            {
                                ctx.Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                await ctx.Context.Response.WriteAsync("Unauthorized");
                            }

                        }
                    });
                }
            }
            app.Run();

        }
    }
}

