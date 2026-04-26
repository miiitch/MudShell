using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazorExtended;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddMudBlazorExtended();

await builder.Build().RunAsync();
