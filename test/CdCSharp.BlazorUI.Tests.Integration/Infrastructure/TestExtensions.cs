using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

public static class TestExtensions
{
    public static IRenderedComponent<T> RenderComponent<T>(
        this TestContextBase ctx,
        Action<ComponentParameterCollectionBuilder<T>>? parameterBuilder = null)
        where T : IComponent
    {
        return parameterBuilder == null
            ? ctx.Render<T>()
            : ctx.Render<T>(parameterBuilder);
    }

    public static void ShouldHaveClass(this IElement element, string cssClass) =>
        element.ClassList.Should().Contain(cssClass);

    public static void ShouldNotHaveClass(this IElement element, string cssClass) =>
        element.ClassList.Should().NotContain(cssClass);

    public static void ShouldHaveTagName(this IElement element, string expectedTagName) => element.TagName.Should().BeEquivalentTo(expectedTagName, options => options.IgnoringCase());
}
