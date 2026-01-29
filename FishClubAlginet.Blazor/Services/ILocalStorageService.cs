using FishClubAlginet.Blazor.Constants;

namespace FishClubAlginet.Blazor.Services;

public interface ILocalStorageService
{
    Task SetItemAsync<T>(string key, T value);
    Task<T?> GetItemAsync<T>(string key);
    Task RemoveItemAsync(string key);
}
public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        // Serializamos a JSON para poder guardar objetos complejos si hiciera falta
        var json = JsonSerializer.Serialize(value);
        await _jsRuntime.InvokeVoidAsync(LogicConstants.LocalStorageSetItem, key, json);
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        // Recuperamos el string crudo
        var json = await _jsRuntime.InvokeAsync<string?>(LogicConstants.LocalStorageGetItem, key);

        if (json == null)
            return default;

        // Lo convertimos de vuelta al tipo C# que necesitemos
        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task RemoveItemAsync(string key) => await _jsRuntime.InvokeVoidAsync(LogicConstants.LocalStorageRemoveItem, key);
}
