using DiscordMusicBot.WebAPI.Endpoints;
using DiscordMusicBot.Worker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<Worker>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder => policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

var worker = app.Services.GetRequiredService<Worker>();

app.MapGet("/Check", () => Results.Ok());

BotEndpoints.Map(app, worker);

MusicEndpoints.Map(app, worker);

app.Run();