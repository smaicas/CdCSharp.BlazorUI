using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using System.Linq.Expressions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Dropdown;

[Trait("Component Interaction", "BUIInputDropdown")]
public class BUIInputDropdownInteractionTests
{
    
    private class DummyModel { public string? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<string?>> _expr = () => _dm.Value;

    private static Action<ComponentParameterCollectionBuilder<BUIInputDropdown<string>>> WithOptions(string? value = null) =>
        p =>
        {
            p.Add(c => c.ValueExpression, _expr);
            if (value != null) p.Add(c => c.Value, value);
            p.Add(c => c.ChildContent, builder =>
            {
                builder.OpenComponent<DropdownOption<string>>(0);
                builder.AddAttribute(1, "Value", "opt1");
                builder.AddAttribute(2, "Text", "Option 1");
                builder.CloseComponent();
                builder.OpenComponent<DropdownOption<string>>(3);
                builder.AddAttribute(4, "Value", "opt2");
                builder.AddAttribute(5, "Text", "Option 2");
                builder.CloseComponent();
            });
        };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Open_Menu_On_Trigger_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(WithOptions());

        // Act
        cut.Find("button.bui-dropdown__trigger").Click();

        // Assert
        cut.Find(".bui-dropdown__menu").Should().NotBeNull();
        cut.Find("bui-component").GetAttribute("data-bui-dropdown-open").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_Menu_On_Second_Trigger_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(WithOptions());

        // Act
        cut.Find("button.bui-dropdown__trigger").Click(); // open
        cut.Find("button.bui-dropdown__trigger").Click(); // close

        // Assert
        cut.FindAll(".bui-dropdown__menu").Should().BeEmpty();
        cut.Find("bui-component").GetAttribute("data-bui-dropdown-open").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Options_When_Open(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(WithOptions());

        // Act
        cut.Find("button.bui-dropdown__trigger").Click();

        // Assert
        IReadOnlyList<IElement> options = cut.FindAll(".bui-dropdown__option");
        options.Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Option_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string? captured = null;
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p =>
        {
            WithOptions()(p);
            p.Add(c => c.ValueChanged, v => captured = v);
        });

        // Act
        cut.Find("button.bui-dropdown__trigger").Click();
        cut.Find(".bui-dropdown__option").Click();

        // Assert
        captured.Should().Be("opt1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_Menu_After_Selection_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(WithOptions());

        // Act
        cut.Find("button.bui-dropdown__trigger").Click();
        cut.Find(".bui-dropdown__option").Click();

        // Assert — CloseOnSelect=true by default, menu closes
        cut.FindAll(".bui-dropdown__menu").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Open_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p =>
        {
            WithOptions()(p);
            p.Add(c => c.Disabled, true);
        });

        // Act — trigger is disabled, click won't fire
        cut.FindAll(".bui-dropdown__menu").Should().BeEmpty();
        cut.Find("button.bui-dropdown__trigger").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Aria_Expanded_On_Open(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(WithOptions());

        // Assert initial
        cut.Find("button.bui-dropdown__trigger").GetAttribute("aria-expanded").Should().Be("false");

        // Act
        cut.Find("button.bui-dropdown__trigger").Click();

        // Assert after open
        cut.Find("button.bui-dropdown__trigger").GetAttribute("aria-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Selected_Option_As_Selected(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(WithOptions("opt1"));

        // Act
        cut.Find("button.bui-dropdown__trigger").Click();

        // Assert — selected option has aria-selected=true
        IElement selectedOption = cut.FindAll(".bui-dropdown__option")
            .First(o => o.TextContent.Contains("Option 1"));
        selectedOption.GetAttribute("aria-selected").Should().Be("true");
    }
}
