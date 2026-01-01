using CdCSharp.BlazorUI.Localization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace CdCSharp.BlazorUI.Localization.Server;

internal class ServerLocalizationPersistence : ILocalizationPersistence
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LocalizationSettings _options;

    public ServerLocalizationPersistence(
        IHttpContextAccessor httpContextAccessor,
        LocalizationSettings options)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options;
    }

    public Task<string?> GetStoredCultureAsync()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return Task.FromResult<string?>(null);

        IRequestCultureFeature? feature = httpContext.Features.Get<IRequestCultureFeature>();
        return Task.FromResult(feature?.RequestCulture.Culture.Name);
    }

    public Task SetStoredCultureAsync(string culture)
    {
        // En Server, NO intentamos establecer la cookie aquí
        // Solo retornamos - la cookie se establecerá a través del endpoint
        return Task.CompletedTask;
    }
}