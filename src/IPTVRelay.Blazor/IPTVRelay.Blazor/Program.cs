using Hangfire;
using Hangfire.Storage.SQLite;
using IPTVRelay.Blazor.Client.Pages;
using IPTVRelay.Blazor.Components;
using IPTVRelay.Database;
using IPTVRelay.Database.Enums;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace IPTVRelay.Blazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables("IPTV_");
            var config = (IConfiguration)configBuilder.Build();
            var dataDirectory = new DirectoryInfo(config.GetValue<string>("DATA_FOLDER"));
            if (!dataDirectory.Exists) dataDirectory.Create();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddHangfire(c => c.UseSQLiteStorage(Path.Combine(dataDirectory.FullName, "scheduler.db")))
                .AddDbContext<IPTVRelayContext>(options => options.UseSqlite(new SqliteConnectionStringBuilder { DataSource = Path.Combine(dataDirectory.FullName, "IPTVRelay.db") }.ToString()))
                .AddSingleton(config)
                .AddBlazorBootstrap()
                .AddHangfireServer()
                .AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();


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


            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IPTVRelayContext>();
                db.Database.Migrate();

                if (int.TryParse(config.GetValue<string>("PORT"), out var port))
                {
                    app.Urls.Clear();
                    app.Urls.Add($"http://*:{port}");
                }

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
                        ContentTypeProvider = new FileExtensionContentTypeProvider
                        {
                            Mappings = { [".m3u"] = "application/x-mpegURL", [".m3u8"] = "application/x-mpegURL", [".ts"] = "video/MP2T", [".xml"] = "application/xml" }
                        },
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
                {
                    var directory = "preview";
                    var staticDirectory = new DirectoryInfo(Path.Combine(dataDirectory.FullName, directory));
                    if (!staticDirectory.Exists) staticDirectory.Create();

                    app.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(Path.Combine(dataDirectory.FullName, directory)),
                        RequestPath = $"/{directory}",
                        ContentTypeProvider = new FileExtensionContentTypeProvider
                        {
                            Mappings = { [".m3u"] = "application/x-mpegURL", [".m3u8"] = "application/x-mpegURL", [".ts"] = "video/MP2T" }
                        },
                        OnPrepareResponseAsync = async ctx =>
                        {

                        }
                    });
                }
            }
            app.Run();

        }
    }
}

