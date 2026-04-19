using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using System.Linq.Expressions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Dropdown;

[Trait("Component State", "BUIInputDropdown")]
public class BUIInputDropdownStateTests
{

    private class DummyModel { public string? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<string?>> _expr = () => _dm.Value;

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Disabled_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Act
        cut.Render(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Disabled, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-disabled").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Readonly_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Act
        cut.Render(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.ReadOnly, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-readonly").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Loading_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Act
        cut.Render(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.IsLoading, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-loading").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Dropdown_Closed_Initially(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-dropdown-open").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_Label_When_Value_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Label, "Select")
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Option 1");
                builder.CloseComponent();
            }));

        // Assert initial — not floated
        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("false");

        // Act — set value
        cut.Render(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Value, "opt1")
            .Add(c => c.Label, "Select")
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Option 1");
                builder.CloseComponent();
            }));

        // Assert floated
        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Selected_Option_Text_In_Trigger(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Value, "opt1")
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Option 1");
                builder.CloseComponent();
            }));

        // Assert
        cut.Find(".bui-dropdown__value").TextContent.Trim().Should().Contain("Option 1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Trigger_Button_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Disabled, true));

        // Assert
        cut.Find("button.bui-dropdown__trigger").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Open_Attribute_When_Opened(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Option 1");
                builder.CloseComponent();
            }));

        // Assert initial
        cut.Find("bui-component").GetAttribute("data-bui-dropdown-open").Should().Be("false");

        // Act
        cut.Find("button.bui-dropdown__trigger").Click();

        // Assert opened
        cut.Find("bui-component").GetAttribute("data-bui-dropdown-open").Should().Be("true");
    }
}
