using CdCSharp.BlazorUI.Components.Abstractions;
using CdCSharp.BlazorUI.Types;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Core.Theming.Interop;

public interface IThemeJsInterop
{
    ValueTask InitializeAsync(string? defaultTheme = null);
    ValueTask<string> GetThemeAsync();
    ValueTask SetThemeAsync(string theme);
    ValueTask<string> ToggleThemeAsync(string[] themes);
}

public sealed class ThemeJsInterop(IJSRuntime jsRuntime)
    : ModuleJsInteropBase(jsRuntime, JSModulesReference.ThemeJs),
      IThemeJsInterop
{
    public async ValueTask InitializeAsync(string? defaultTheme = null)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("initialize", defaultTheme);
    }

    public async ValueTask<string> GetThemeAsync()
    {
        IJSObjectReference module = await ModuleTask.Value;
        return await module.InvokeAsync<string>("getTheme");
    }

    public async ValueTask SetThemeAsync(string theme)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("setTheme", theme);
    }

    public async ValueTask<string> ToggleThemeAsync(params string[] themes)
    {
        IJSObjectReference module = await ModuleTask.Value;
        return await module.InvokeAsync<string>("toggleTheme", new object[] { themes });
    }
}
