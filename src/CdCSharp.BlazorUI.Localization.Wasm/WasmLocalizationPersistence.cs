using CdCSharp.BlazorUI.Localization.Abstractions;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Localization.Wasm;

internal class WasmLocalizationPersistence : ILocalizationPersistence
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public WasmLocalizationPersistence(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/CdCSharp.BlazorUI/js/Types/Localization/LocalizationInterop.min.js");
        return _module;
    }

    public async Task<string?> GetStoredCultureAsync()
    {
        try
        {
            IJSObjectReference module = await GetModuleAsync();
            return await module.InvokeAsync<string?>("getStoredCulture");
        }
        catch
        {
            return null;
        }
    }

    public async Task SetStoredCultureAsync(string culture)
    {
        IJSObjectReference module = await GetModuleAsync();
        await module.InvokeVoidAsync("setStoredCulture", culture);
    }
}