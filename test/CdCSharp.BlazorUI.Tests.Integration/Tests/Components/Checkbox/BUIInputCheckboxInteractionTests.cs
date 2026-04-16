using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Checkbox;

[Trait("Component Interaction", "BUIInputCheckbox")]
public class BUIInputCheckboxInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Bool_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool captured = false;
        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Value, false)
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find(".bui-checkbox").Click();

        // Assert
        captured.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Uncheck_On_Second_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool captured = true;
        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Value, true)
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find(".bui-checkbox").Click();

        // Assert
        captured.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Cycle_Nullable_Bool_False_Null_True_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // false → null (indeterminate)
        bool? captured = false;
        IRenderedComponent<BUIInputCheckbox<bool?>> cut = ctx.Render<BUIInputCheckbox<bool?>>(p => p
            .Add(c => c.Value, (bool?)false)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.Find(".bui-checkbox").Click();
        captured.Should().BeNull();

        // null → true
        cut.Render(p => p
            .Add(c => c.Value, (bool?)null)
            .Add(c => c.ValueChanged, v => captured = v));
        cut.Find(".bui-checkbox").Click();
        captured.Should().BeTrue();

        // true → false
        cut.Render(p => p
            .Add(c => c.Value, (bool?)true)
            .Add(c => c.ValueChanged, v => captured = v));
        cut.Find(".bui-checkbox").Click();
        captured.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Toggle_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool captured = false;
        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Value, false)
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find(".bui-checkbox").Click();

        // Assert
        captured.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_On_Space_Key(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool captured = false;
        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Value, false)
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find(".bui-checkbox").KeyDown(key: " ");

        // Assert
        captured.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_On_Enter_Key(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool captured = false;
        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Value, false)
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find(".bui-checkbox").KeyDown(key: "Enter");

        // Assert
        captured.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Toggle_On_Other_Keys(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool captured = false;
        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Value, false)
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find(".bui-checkbox").KeyDown(key: "Tab");

        // Assert
        captured.Should().BeFalse();
    }
}
