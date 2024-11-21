using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DiscordMusicBot.WebUI;
using DiscordMusicBot.WebUI.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://192.168.50.123:5257") });

builder.Services.AddMudServices();

builder.Services.AddScoped<ApiController>(HttpClient => new ApiController(HttpClient.GetRequiredService<HttpClient>()));

await builder.Build().RunAsync();