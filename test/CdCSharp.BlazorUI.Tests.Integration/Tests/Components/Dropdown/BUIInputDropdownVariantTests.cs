using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using System.Linq.Expressions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Dropdown;

[Trait("Component Variants", "BUIInputDropdown")]
public class BUIInputDropdownVariantTests
{

    private class DummyModel { public string? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<string?>> _expr = () => _dm.Value;

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Outlined_Variant_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("outlined");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Filled_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Variant, BUIInputVariant.Filled));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("filled");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Standard_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Variant, BUIInputVariant.Standard));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("standard");
    }
}
