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

    public static void ShouldHaveClass(this IElement element, string cssClass) => element.ClassList.Should().Contain(cssClass);

    public static void ShouldHaveVariant(this IRenderedComponent<IComponent> component, string variantName)
    {
        string variantClass = $"variant-{variantName.ToLower()}";
        component.Find(".btn, .input").ClassList.Should().Contain(variantClass);
    }
}
