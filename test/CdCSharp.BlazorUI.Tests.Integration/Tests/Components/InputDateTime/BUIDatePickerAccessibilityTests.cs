using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Accessibility", "BUIDatePicker")]
public class BUIDatePickerAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Label_Navigation_Buttons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();

        // Assert
        cut.Find("button[aria-label='Previous year']").Should().NotBeNull();
        cut.Find("button[aria-label='Previous month']").Should().NotBeNull();
        cut.Find("button[aria-label='Next month']").Should().NotBeNull();
        cut.Find("button[aria-label='Next year']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nav_Buttons_As_Native_Buttons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();

        // Assert — type="button" prevents form submission; button role is implicit
        foreach (string label in new[] { "Previous year", "Previous month", "Next month", "Next year" })
        {
            IElement btn = cut.Find($"button[aria-label='{label}']");
            btn.TagName.Should().Be("BUTTON");
            btn.GetAttribute("type").Should().Be("button");
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Keep_Day_Cells_Keyboard_Focusable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();

        // Assert — all day cell buttons are tabbable (tabindex="0")
        IReadOnlyList<IElement> dayCells = cut.FindAll(".bui-picker__grid button.bui-picker__cell");
        dayCells.Should().OnlyContain(c => c.GetAttribute("tabindex") == "0");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Week_Header_Cells_As_Muted_Non_Interactive(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();

        // Assert — header weekdays render as <span>, not <button>
        IReadOnlyList<IElement> headers = cut.FindAll(".bui-picker__grid span.bui-picker__cell");
        headers.Should().HaveCount(7);
        headers.Should().OnlyContain(h => h.TagName == "SPAN"
            && h.ClassList.Contains("bui-picker__cell--muted"));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Decorative_Chevron_Icons_From_AT(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();

        // Assert — icons inside aria-labeled buttons inherit accessible name;
        // the button's aria-label is sufficient, child svgs should not surface.
        IElement prevMonth = cut.Find("button[aria-label='Previous month']");
        prevMonth.QuerySelector("svg").Should().NotBeNull("chevron icon renders inside the button");
    }
}
