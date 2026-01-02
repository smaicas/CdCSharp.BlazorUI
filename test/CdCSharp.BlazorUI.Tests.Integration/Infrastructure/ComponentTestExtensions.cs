using Bunit;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

public static class ComponentTestExtensions
{
    public static string GetNormalizedMarkup<TComponent>(this IRenderedComponent<TComponent> fragment) where TComponent : IComponent
    {
        return fragment.Markup
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Trim();
    }

    public static void WaitForAssertion<TComponent>(
        this IRenderedComponent<TComponent> fragment,
        Action assertion,
        TimeSpan? timeout = null) where TComponent : IComponent
    {
        fragment.WaitForAssertion(assertion, timeout);
    }
}
