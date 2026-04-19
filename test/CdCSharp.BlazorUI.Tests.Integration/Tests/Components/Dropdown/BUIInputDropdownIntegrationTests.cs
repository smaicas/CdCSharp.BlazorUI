using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using System.Linq.Expressions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Dropdown;

[Trait("Component Integration", "BUIInputDropdown")]
public class BUIInputDropdownIntegrationTests
{

    private class DummyModel { public string? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<string?>> _expr = () => _dm.Value;

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Select_And_Display_Option(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string? selected = null;
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Label, "Choose")
            .Add(c => c.ValueChanged, v => selected = v)
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "apple");
                builder.AddAttribute(2, "Text", "Apple");
                builder.CloseComponent();
                builder.OpenComponent<DropdownOption<string>>(3);
                builder.AddAttribute(4, "Value", "banana");
                builder.AddAttribute(5, "Text", "Banana");
                builder.CloseComponent();
            }));

        // Act
        cut.Find("button.bui-dropdown__trigger").Click();
        IElement appleOption = cut.FindAll(".bui-dropdown__option")
            .First(o => o.TextContent.Contains("Apple"));
        appleOption.Click();

        // Assert
        selected.Should().Be("apple");
        cut.FindAll(".bui-dropdown__menu").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Selected_Value_As_Display_Text(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Value, "banana")
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "apple");
                builder.AddAttribute(2, "Text", "Apple");
                builder.CloseComponent();
                builder.OpenComponent<DropdownOption<string>>(3);
                builder.AddAttribute(4, "Value", "banana");
                builder.AddAttribute(5, "Text", "Banana");
                builder.CloseComponent();
            }));

        // Assert — display value shows selected option label
        cut.Find(".bui-dropdown__value").TextContent.Trim().Should().Be("Banana");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Keep_Menu_Open_When_CloseOnSelect_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.CloseOnSelect, false)
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Option 1");
                builder.CloseComponent();
            }));

        // Act
        cut.Find("button.bui-dropdown__trigger").Click();
        cut.Find(".bui-dropdown__option").Click();

        // Assert — menu stays open when CloseOnSelect=false
        cut.Find(".bui-dropdown__menu").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Select_Disabled_Option(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string? selected = null;
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.ValueChanged, v => selected = v)
            .Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Disabled Opt");
                builder.AddAttribute(3, "Disabled", true);
                builder.CloseComponent();
            }));

        // Act
        cut.Find("button.bui-dropdown__trigger").Click();
        cut.Find(".bui-dropdown__option--disabled").Click();

        // Assert — value stays null, not changed
        selected.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_No_Options_Message_When_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Act
        cut.Find("button.bui-dropdown__trigger").Click();

        // Assert
        cut.Find(".bui-dropdown__no-options").TextContent.Should().Contain("No options available");
    }
}
