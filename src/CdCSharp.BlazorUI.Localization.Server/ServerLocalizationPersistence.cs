using CdCSharp.BlazorUI.Localization.Abstractions;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Localization.Server;

//internal class ServerLocalizationPersistence : ILocalizationPersistence
//{
//    private readonly IHttpContextAccessor _httpContextAccessor;
//    private readonly LocalizationSettings _options;

//    public ServerLocalizationPersistence(
//        IHttpContextAccessor httpContextAccessor,
//        LocalizationSettings options)
//    {
//        _httpContextAccessor = httpContextAccessor;
//        _options = options;
//    }

//    public Task<string?> GetStoredCultureAsync()
//    {
//        HttpContext? httpContext = _httpContextAccessor.HttpContext;
//        if (httpContext == null) return Task.FromResult<string?>(null);

//        IRequestCultureFeature? feature = httpContext.Features.Get<IRequestCultureFeature>();
//        return Task.FromResult(feature?.RequestCulture.Culture.Name);
//    }

//    public Task SetStoredCultureAsync(string culture)
//    {
//        // En Server, NO intentamos establecer la cookie aquí
//        // Solo retornamos - la cookie se establecerá a través del endpoint
//        return Task.CompletedTask;
//    }
//}

internal class ServerLocalizationPersistence : ILocalizationPersistence
{
    public const string CULTURE_KEY = "BlazorUI.Culture";

    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public ServerLocalizationPersistence(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/CdCSharp.BlazorUI/js/Types/Storage/LocalStorageInterop.min.js");
        return _module;
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
}