using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component State", "BUIInputDateTime")]
public class BUIInputDateTimeStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Disabled_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDateTime<DateOnly?>> cut = ctx.Render<BUIInputDateTime<DateOnly?>>();

        // Act
        cut.Render(p => p.Add(c => c.Disabled, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-disabled").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Readonly_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDateTime<DateOnly?>> cut = ctx.Render<BUIInputDateTime<DateOnly?>>();

        // Act
        cut.Render(p => p.Add(c => c.ReadOnly, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-readonly").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Loading_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDateTime<DateOnly?>> cut = ctx.Render<BUIInputDateTime<DateOnly?>>();

        // Act
        cut.Render(p => p.Add(c => c.Loading, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-loading").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_When_DateOnly_Value_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDateTime<DateOnly?>> cut = ctx.Render<BUIInputDateTime<DateOnly?>>();
        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("false");

        // Act
        cut.Render(p => p.Add(c => c.Value, new DateOnly(2024, 1, 1)));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_When_TimeOnly_Value_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDateTime<TimeOnly?>> cut = ctx.Render<BUIInputDateTime<TimeOnly?>>();
        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("false");

        // Act
        cut.Render(p => p.Add(c => c.Value, new TimeOnly(12, 0)));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_When_DateTime_Value_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDateTime<DateTime?>> cut = ctx.Render<BUIInputDateTime<DateTime?>>();
        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("false");

        // Act
        cut.Render(p => p.Add(c => c.Value, new DateTime(2024, 6, 15, 14, 30, 0)));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Picker_Button_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDateTime<DateOnly?>> cut = ctx.Render<BUIInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Disabled, true));

        // Assert
        cut.Find("button[aria-label='Open picker']").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_Additional_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDateTime<DateOnly?>> cut = ctx.Render<BUIInputDateTime<DateOnly?>>(p => p
            .AddUnmatched("data-testid", "dt-picker"));

        // Assert
        cut.Find("bui-component").GetAttribute("data-testid").Should().Be("dt-picker");
    }
}
