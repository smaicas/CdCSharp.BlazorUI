using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Css;
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

    public static void ShouldHaveTagName(this IElement element, string expectedTagName) => element.TagName.Should().BeEquivalentTo(expectedTagName, options => options.IgnoringCase());

    public static void ShouldNotHaveClass(this IElement element, string cssClass) =>
            element.ClassList.Should().NotContain(cssClass);

    /// <summary>
    /// Asserts that an element's class attribute contains no duplicates
    /// </summary>
    public static void ShouldHaveNoDuplicateClasses(this IElement element)
    {
        List<string> classes = element.ClassList.ToList();
        List<string> duplicates = classes.GroupBy(c => c).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

        duplicates.Should().BeEmpty(
            because: $"element should not have duplicate CSS classes, but found: {string.Join(", ", duplicates)}");
    }

    /// <summary>
    /// Asserts that an element's style attribute contains the specified property
    /// </summary>
    public static void ShouldHaveStyle(this IElement element, string property, string? expectedValue = null)
    {
        string style = element.GetAttribute("style") ?? "";
        style.Should().Contain(property, because: $"element style should contain property '{property}'");

        if (expectedValue != null)
        {
            style.Should().Contain($"{property}: {expectedValue}",
                because: $"element style property '{property}' should have value '{expectedValue}'");
        }
    }

    /// <summary>
    /// Asserts that an element's style attribute does not contain the specified property
    /// </summary>
    public static void ShouldNotHaveStyle(this IElement element, string property)
    {
        string style = element.GetAttribute("style") ?? "";
        style.Should().NotContain(property, because: $"element style should not contain property '{property}'");
    }

    /// <summary>
    /// Gets the number of CSS classes that match a predicate
    /// </summary>
    public static int CountClasses(this IElement element, Func<string, bool> predicate)
    {
        return element.ClassList.Count(predicate);
    }

    /// <summary>
    /// Asserts that an element has exactly one class matching the prefix
    /// </summary>
    public static void ShouldHaveExactlyOneClassWithPrefix(this IElement element, string prefix)
    {
        int count = element.CountClasses(c => c.StartsWith(prefix));
        count.Should().Be(1,
            because: $"element should have exactly one CSS class starting with '{prefix}'");
    }

    /// <summary>
    /// Asserts that an element has exactly one class matching the prefix
    /// </summary>
    public static void ShouldNotHaveClassWithPrefix(this IElement element, string prefix)
    {
        int count = element.CountClasses(c => c.StartsWith(prefix));
        count.Should().Be(0,
            because: $"element should have exactly one CSS class starting with '{prefix}'");
    }

    /// <summary>
    /// Gets the single class that starts with the specified prefix
    /// </summary>
    public static string? GetClassWithPrefix(this IElement element, string prefix)
    {
        return element.ClassList.FirstOrDefault(c => c.StartsWith(prefix));
    }

    /// <summary>
    /// Asserts common feature classes are applied correctly
    /// </summary>
    public static void ShouldHaveFeatureClasses(this IElement element,
        SizeEnum? expectedSize = null,
        DensityEnum? expectedDensity = null,
        int? expectedElevation = null,
        bool? expectFullWidth = null,
        bool? expectLoading = null,
        bool? expectRipple = null)
    {
        if (expectedSize.HasValue)
        {
            element.ShouldHaveClass(CssClassesReference.Size(expectedSize.Value));
        }

        if (expectedDensity.HasValue)
        {
            element.ShouldHaveClass(CssClassesReference.Density(expectedDensity.Value));
        }

        if (expectedElevation.HasValue && expectedElevation > 0)
        {
            element.ShouldHaveClass(CssClassesReference.Elevation(expectedElevation.Value));
        }

        if (expectFullWidth == true)
        {
            element.ShouldHaveClass(CssClassesReference.FullWidth);
        }

        if (expectLoading == true)
        {
            element.ShouldHaveClass(CssClassesReference.Loading);
        }

        if (expectRipple == true)
        {
            element.ShouldHaveClass(CssClassesReference.HasRipple);
        }
    }

    public static void ShouldHaveDataAttribute(this IElement element, string attributeName, string expectedValue)
    {
        element.GetAttribute($"data-{attributeName}")
            .Should().Be(expectedValue, $"element should have data-{attributeName}='{expectedValue}'");
    }

    public static void ShouldHaveDataComponent(this IElement element, string componentName)
    {
        element.ShouldHaveDataAttribute("ui-component", componentName);
    }

    public static void ShouldHaveDataVariant(this IElement element, string variantName)
    {
        element.ShouldHaveDataAttribute("ui-variant", variantName);
    }

    public static IElement FindByDataComponent<TComponent>(this IRenderedComponent<TComponent> fragment, string componentName) where TComponent : IComponent
    {
        return fragment.Find<TComponent>($"[data-ui-component='{componentName}']");
    }

    public static IElement FindByDataVariant<TComponent>(this IRenderedComponent<TComponent> fragment, string variantName) where TComponent : IComponent
    {
        return fragment.Find<TComponent>($"[data-ui-variant='{variantName}']");
    }
    /// <summary>
    /// Renders a component multiple times with the same parameters to test stability
    /// </summary>
    public static void RenderMultipleTimes<TComponent>(this TestContextBase context,
        int times,
        Action<ComponentParameterCollectionBuilder<TComponent>> parameterBuilder,
        Action<IRenderedComponent<TComponent>, int> assertion) where TComponent : IComponent
    {
        IRenderedComponent<TComponent> cut = context.Render(parameterBuilder);

        for (int i = 0; i < times; i++)
        {
            cut.Render(parameterBuilder);
            assertion(cut, i);
        }
    }
}