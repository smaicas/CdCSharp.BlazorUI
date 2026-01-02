using System.Globalization;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Fakes;

public interface ITestCultureService
{
    CultureInfo CurrentCulture { get; }
    Task SetCultureAsync(string culture);
}
