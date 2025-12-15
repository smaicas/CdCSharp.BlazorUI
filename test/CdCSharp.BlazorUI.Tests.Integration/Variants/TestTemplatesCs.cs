using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Core.Variants;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Variants;

public static class TestTemplatesCs
{
    [VariantTemplate(typeof(UIButton), typeof(UIButtonVariant), "Custom")]
    public static RenderFragment BasicCustomTemplate(UIButton component) => __builder =>
    {
        __builder.OpenElement(0, "button");
        __builder.AddMultipleAttributes(1, component.AdditionalAttributes);
        __builder.AddAttribute(2, "onclick", component.OnClick);
        __builder.AddAttribute(3, "disabled", component.Disabled);
        __builder.AddContent(4, component.Text);
        __builder.CloseElement();
    };

    [VariantTemplate(typeof(UIButton), typeof(UIButtonVariant), "Glass")]
    public static RenderFragment AddsOneClassTemplate(UIButton component) => __builder =>
    {
        __builder.OpenElement(0, "button");
        __builder.AddMultipleAttributes(1, component.AdditionalAttributes);
        __builder.AddAttribute(2, "class", $"{component.ComputedCssClasses} btn-glass");
        __builder.AddAttribute(3, "onclick", component.OnClick);
        __builder.AddAttribute(4, "disabled", component.Disabled);
        __builder.AddContent(5, component.Text);
        __builder.CloseElement();
    };

    [VariantTemplate(typeof(UIButton), typeof(UIButtonVariant), "Override")]
    public static RenderFragment OverrideClassTemplate(UIButton component) => __builder =>
    {
        __builder.OpenElement(0, "button");
        __builder.AddAttribute(1, "class", "btn-override-only");
        __builder.AddContent(2, $"Override: {component.Text}");
        __builder.CloseElement();
    };
}
