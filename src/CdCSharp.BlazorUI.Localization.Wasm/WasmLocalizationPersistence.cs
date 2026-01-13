using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Localization.Wasm;

public interface ILocalizationPersistence
{
    Task<string?> GetStoredCultureAsync();

    Task SetStoredCultureAsync(string culture);
}

internal class WasmLocalizationPersistence : ILocalizationPersistence
{
    public const string CULTURE_KEY = "BlazorUI.Culture";

    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public WasmLocalizationPersistence(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> GetStoredCultureAsync()
    {
        try
        {
            IJSObjectReference module = await GetModuleAsync();
            return await module.InvokeAsync<string?>("get", CULTURE_KEY);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetStoredCultureAsync(string culture)
    {
        IJSObjectReference module = await GetModuleAsync();
        await module.InvokeVoidAsync("set", CULTURE_KEY, culture);
    }

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/CdCSharp.BlazorUI/js/Types/Storage/LocalStorageInterop.min.js");
        return _module;
    }
}