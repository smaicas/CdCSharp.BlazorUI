namespace CdCSharp.BlazorUI.Localization.Abstractions;

public interface ILocalizationPersistence
{
    Task<string?> GetStoredCultureAsync();
    Task SetStoredCultureAsync(string culture);
}
