using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DiscordMusicBot.WebUI;
using DiscordMusicBot.WebUI.Services;
using Microsoft.JSInterop;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://0.0.0.0:5257") });

builder.Services.AddMudServices();

builder.Services.AddScoped<LocalStorageService>(sp=> new LocalStorageService(sp.GetRequiredService<IJSRuntime>()));
builder.Services.AddScoped<ApiController>(sp=> new ApiController(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<LocalStorageService>()));

await builder.Build().RunAsync();