using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudShell;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddMudShell();

await builder.Build().RunAsync();
