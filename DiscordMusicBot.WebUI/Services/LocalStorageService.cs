using Microsoft.JSInterop;

namespace DiscordMusicBot.WebUI.Services;

public class LocalStorageService(IJSRuntime _jsRuntime)
{

    public async Task<string> GetItem(string key)
    {
        var item = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);

        return item;
    }

    public async Task SetItem(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    public async Task RemoveItem(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }

    public async Task Clear()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.clear");
    }
}