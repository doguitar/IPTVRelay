using Hangfire;
using Hangfire.Storage.SQLite;
using IPTVRelay.Blazor.Client.Pages;
using IPTVRelay.Blazor.Components;
using IPTVRelay.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

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

            app.UseHangfireServer();

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

                var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
                recurringJobManager.AddOrUpdate<Utility.Jobs.UpdateJob>(
                    "UPDATE",
                    (j) => j.Update(),
                    Cron.Daily(DateTime.Now.Hour, DateTime.Now.Minute));



            }

            if (dataDirectory != null)
            {
                if (!dataDirectory.Exists) dataDirectory.Create();
                var imageDirectory = new DirectoryInfo(Path.Combine(dataDirectory.FullName, "images"));
                if (!imageDirectory.Exists) imageDirectory.Create();
                var outputDirectory = new DirectoryInfo(Path.Combine(dataDirectory.FullName, "output"));
                if (!outputDirectory.Exists) outputDirectory.Create();

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(dataDirectory.FullName, "images")),
                    RequestPath = "/images"
                });
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(dataDirectory.FullName, "output")),
                    RequestPath = "/output"
                });
            }
            app.Run();
        }
    }
}
