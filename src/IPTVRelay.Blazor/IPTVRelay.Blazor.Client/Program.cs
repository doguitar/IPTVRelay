using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services
    .AddBlazorBootstrap();

var app = builder.Build();    

await app.RunAsync();
