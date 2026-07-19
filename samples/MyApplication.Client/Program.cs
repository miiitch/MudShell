using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyApplication.Client.Theme;
using MudShell;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddMudShell();
builder.Services.AddScoped<ThemeState>();

await builder.Build().RunAsync();
