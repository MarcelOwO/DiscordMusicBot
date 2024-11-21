using System.Text.Json.Serialization;

namespace DiscordMusicBot.Core.Models;

[method: JsonConstructor]
public class ServerData(string id, string name)
{
    [JsonPropertyName("id")] public string Id { get; set; } = id;
    [JsonPropertyName("name")] public string Name { get; set; } = name;
}