using IPTVRelay.Blazor.Client.Pages;
using IPTVRelay.Blazor.Components;
using IPTVRelay.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace IPTVRelay.Blazor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables("IPTV_");


            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services
                .AddDbContext<IPTVRelayContext>(options => options.UseSqlite(new SqliteConnectionStringBuilder { DataSource = "IPTVRelay.db" }.ToString()))
                .AddSingleton((IConfiguration)configBuilder.Build())
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

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            DirectoryInfo dataDirectory = null;
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IPTVRelayContext>();
                db.Database.Migrate();

                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                dataDirectory = new DirectoryInfo(config.GetValue<string>("DATA_FOLDER"));

                if (int.TryParse(config.GetValue<string>("PORT"), out var port))
                {
                    app.Urls.Clear();
                    app.Urls.Add($"http://*:{port}");
                }

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
