//using AngleSharp.Dom;
//using Bunit;
//using CdCSharp.BlazorUI.Components;
//using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
//using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
//using FluentAssertions;

//namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Button;

//[Trait("Component Accessibility", "BUIButton")]
//public class BUIButtonAccessibilityTests
//{
//    [Theory]
//    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
//    public async Task Should_Have_Proper_ARIA_Attributes_When_Disabled(BlazorScenario scenario)
//    {
//        await using BlazorTestContextBase ctx = scenario.CreateContext();

// // Arrange & Act IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p .Add(c =>
// c.Text, "Disabled Button") .Add(c => c.Disabled, true) .Add(c => c.AriaLabel, "Cannot click this button"));

// // Assert IElement button = cut.Find("button");
// button.GetAttribute("disabled").Should().NotBeNull();
// button.GetAttribute("aria-disabled").Should().Be("true");
// button.GetAttribute("aria-label").Should().Be("Cannot click this button"); }

// [Theory] [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))] public async
// Task Should_Have_Proper_ARIA_Attributes_When_Loading(BlazorScenario scenario) { await using
// BlazorTestContextBase ctx = scenario.CreateContext();

// // Arrange & Act IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p .Add(c =>
// c.Text, "Loading Button") .Add(c => c.IsLoading, true) .Add(c => c.LoadingIndicatorVariant, BUILoadingIndicatorVariant.Spinner));

// // Assert IElement button = cut.Find("button");
// button.GetAttribute("aria-busy").Should().Be("true");
// button.GetAttribute("disabled").Should().NotBeNull(); }

// [Theory] [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))] public async
// Task Should_Support_Keyboard_Navigation(BlazorScenario scenario) { await using
// BlazorTestContextBase ctx = scenario.CreateContext();

// // Arrange int clickCount = 0; IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p
// .Add(c => c.Text, "Keyboard Button") .Add(c => c.OnClick, _ => clickCount++));

// IElement button = cut.Find("button");

// // Act - Simulate Enter key press await button.KeyPressAsync("Enter");

// // Assert clickCount.Should().Be(1);

// // Act - Simulate Space key press await button.KeyPressAsync(" ");

// // Assert clickCount.Should().Be(2); }

// [Theory] [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))] public async
// Task Should_Have_Proper_Role_Attribute(BlazorScenario scenario) { await using
// BlazorTestContextBase ctx = scenario.CreateContext();

// // Arrange & Act IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p .Add(c =>
// c.Text, "Role Button") .Add(c => c.Role, "menuitem"));

// // Assert IElement button = cut.Find("button");
// button.GetAttribute("role").Should().Be("menuitem"); }

// [Theory] [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))] public async
// Task Should_Have_Proper_TabIndex(BlazorScenario scenario) { await using BlazorTestContextBase ctx
// = scenario.CreateContext();

// // Test enabled button IRenderedComponent<BUIButton> enabledCut = ctx.Render<BUIButton>(p => p
// .Add(c => c.Text, "Enabled Button") .Add(c => c.TabIndex, 5));

// IElement enabledButton = enabledCut.Find("button"); enabledButton.GetAttribute("tabindex").Should().Be("5");

// // Test disabled button - should not be focusable IRenderedComponent<BUIButton> disabledCut =
// ctx.Render<BUIButton>(p => p .Add(c => c.Text, "Disabled Button") .Add(c => c.Disabled, true));

// IElement disabledButton = disabledCut.Find("button");
// disabledButton.GetAttribute("tabindex").Should().Be("-1"); }

// [Theory] [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))] public async
// Task Should_Announce_State_Changes_To_Screen_Readers(BlazorScenario scenario) { await using
// BlazorTestContextBase ctx = scenario.CreateContext();

// // Arrange IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p .Add(c => c.Text,
// "State Button") .Add(c => c.AriaLive, "polite"));

// // Act - Change to loading state cut.SetParametersAndRender(p => p .Add(c => c.IsLoading, true)
// .Add(c => c.LoadingIndicatorVariant, BUILoadingIndicatorVariant.Spinner));

// // Assert IElement button = cut.Find("button");
// button.GetAttribute("aria-live").Should().Be("polite");
// button.GetAttribute("aria-busy").Should().Be("true"); }

// [Theory] [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))] public async
// Task Should_Support_ARIA_Describedby(BlazorScenario scenario) { await using BlazorTestContextBase
// ctx = scenario.CreateContext();

// // Arrange & Act IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p .Add(c =>
// c.Text, "Described Button") .Add(c => c.AriaDescribedBy, "button-description"));

// // Assert IElement button = cut.Find("button");
// button.GetAttribute("aria-describedby").Should().Be("button-description"); }

// [Theory] [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))] public async
// Task Should_Have_Sufficient_Color_Contrast(BlazorScenario scenario) { await using
// BlazorTestContextBase ctx = scenario.CreateContext();

// // This is a placeholder for color contrast testing // In a real implementation, you would
// calculate WCAG contrast ratios // between foreground and background colors

// // Arrange & Act IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p .Add(c =>
// c.Text, "Contrast Button") .Add(c => c.Variant, BUIButtonVariant.Primary));

//        // Assert
//        cut.Find("button").Should().NotBeNull();
//        // TODO: Add actual contrast ratio calculations when color values are available
//    }
//}