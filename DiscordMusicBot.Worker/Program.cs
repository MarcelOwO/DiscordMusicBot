using DiscordMusicBot.Core;
using DiscordMusicBot.Worker;
using DiscordMusicBot.Worker.Interface;
using DiscordMusicBot.Worker.Service;
using DiscordMusicBot.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<DSharpPlusWrapper>();
builder.Services.AddSingleton<IAudioProvider,YoutubeProvider>();
builder.Services.AddSingleton<StreamProcessor>();

builder.Services.AddSingleton<BotService>();
builder.Services.AddSingleton<AudioService>();
builder.Services.AddSingleton<QueueService>();

builder.Services.AddHostedService<Worker>();


var host = builder.Build();
host.Run();