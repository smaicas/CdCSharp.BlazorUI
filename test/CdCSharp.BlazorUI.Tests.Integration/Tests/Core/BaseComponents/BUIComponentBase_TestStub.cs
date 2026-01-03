using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Design;
using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Javascript;
using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.State;
using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Transitions;
using CdCSharp.BlazorUI.Core.Abstractions.Components;
using CdCSharp.BlazorUI.Core.Css;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Core.BaseComponents;

internal class BUIComponentBase_TestStub : BUIComponentBase,
    IHasSize, IHasDensity, IHasFullWidth, IHasElevation, IHasLoading,
    IHasError, IHasDisabled, IHasReadOnly, IHasRequired, IHasRipple,
    IHasColor, IHasBackgroundColor, IHasBorder, IHasTransitions, IJsBehavior
{
    [Parameter] public SizeEnum Size { get; set; }
    [Parameter] public DensityEnum Density { get; set; }
    [Parameter] public bool FullWidth { get; set; }
    [Parameter] public int? Elevation { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public bool IsError { get; set; }
    [Parameter] public bool IsDisabled { get; set; }
    [Parameter] public bool IsReadOnly { get; set; }
    [Parameter] public bool IsRequired { get; set; }
    [Parameter] public bool DisableRipple { get; set; }
    [Parameter] public CssColor? RippleColor { get; set; }
    [Parameter] public int? RippleDuration { get; set; }
    [Parameter] public CssColor? Color { get; set; }
    [Parameter] public CssColor? BackgroundColor { get; set; }
    [Parameter] public BorderStyle? Border { get; set; }
    [Parameter] public BorderStyle? BorderTop { get; set; }
    [Parameter] public BorderStyle? BorderRight { get; set; }
    [Parameter] public BorderStyle? BorderBottom { get; set; }
    [Parameter] public BorderStyle? BorderLeft { get; set; }
    [Parameter] public BUITransitions? Transitions { get; set; }

    public ElementReference RootRef;
    public ElementReference GetRootElement() => RootRef;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        // Attributes MUST be just after the opening element
        builder.AddMultipleAttributes(1, ComputedAttributes);
        builder.AddElementReferenceCapture(2, @ref => RootRef = @ref);
        builder.CloseElement();
    }
}
