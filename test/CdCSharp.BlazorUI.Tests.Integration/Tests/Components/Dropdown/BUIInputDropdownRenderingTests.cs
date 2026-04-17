using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using System.Linq.Expressions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Dropdown;

[Trait("Component Rendering", "BUIInputDropdown")]
public class BUIInputDropdownRenderingTests
{
    
    private class DummyModel { public string? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<string?>> _expr = () => _dm.Value;

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_DropdownContainer_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("dropdown-container");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Input_Family_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-input-base").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Trigger_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Assert
        cut.Find("button.bui-dropdown__trigger").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Label, "Select option"));

        // Assert
        cut.Markup.Should().Contain("Select option");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Placeholder_When_No_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.Placeholder, "Choose..."));

        // Assert
        cut.Markup.Should().Contain("Choose...");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Menu_When_Closed(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr));

        // Assert
        cut.FindAll(".bui-dropdown__menu").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Menu_With_Listbox_Role_When_Open(BlazorScenario scenario)
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

        // Act
        cut.Find("button.bui-dropdown__trigger").Click();

        // Assert
        IElement menu = cut.Find(".bui-dropdown__menu");
        menu.GetAttribute("role").Should().Be("listbox");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Helper_Text_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdown<string>> cut = ctx.Render<BUIInputDropdown<string>>(p => p
            .Add(c => c.ValueExpression, _expr)
            .Add(c => c.HelperText, "Pick one item"));

        // Assert
        cut.Markup.Should().Contain("Pick one item");
    }
}
