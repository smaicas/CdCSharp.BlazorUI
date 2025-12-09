using Bunit;
using CdCSharp.BlazorUI.Core.Components.Abstractions;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Components;

public abstract class ComponentVariantTestBase<TComponent, TVariant> : TestContextBase
    where TComponent : UIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    protected abstract TVariant[] GetAllVariants();
    protected abstract string GetExpectedCssClass(TVariant variant);

    [Fact]
    public void AllVariants_ShouldRenderCorrectly()
    {
        // Arrange & Act
        List<(TVariant variant, IRenderedComponent<TComponent> component)> results = GetAllVariants().Select(variant =>
        {
            IRenderedComponent<TComponent> component = Render<TComponent>(parameters =>
                parameters.Add(p => p.Variant, variant));
            return (variant, component);
        }).ToList();

        // Assert
        results.Should().AllSatisfy(result =>
        {
            string expectedClass = GetExpectedCssClass(result.variant);
            result.component.Find("*").ShouldHaveClass(expectedClass);
        });
    }
}
