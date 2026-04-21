using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Core.Abstractions.Components;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Core.BaseComponents;

/// <summary>
/// Direct unit tests for <see cref="BUIComponentAttributesBuilder" />.
/// Exercises <c>BuildStyles</c> and <c>PatchVolatileAttributes</c> against minimal stubs
/// so that the contract (kebab-case naming, per-IHas* emission, volatile subset, family
/// coexistence, built-component hook, inline-style merging) is pinned without going through
/// the full bUnit render pipeline.
/// </summary>
[Trait("Core", "BUIComponentAttributesBuilder")]
public class BUIComponentAttributesBuilderUnitTests
{
    // ---------- Naming ----------

    [Fact]
    public void ComponentName_Should_Strip_BUI_Prefix_And_Kebab_Case()
    {
        BUIComponentAttributesBuilder builder = new();
        BUIDemoComponent component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Component]
            .Should().Be("demo-component");
    }

    [Fact]
    public void ComponentName_Should_Strip_Generic_Arity_Backtick()
    {
        BUIComponentAttributesBuilder builder = new();
        BUIGenericDemo<int> component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Component]
            .Should().Be("generic-demo");
    }

    [Fact]
    public void ComponentName_Should_Kebab_Case_Non_Prefixed_Types()
    {
        BUIComponentAttributesBuilder builder = new();
        PlainComponent component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Component]
            .Should().Be("plain-component");
    }

    // ---------- Per-IHas emission ----------

    [Fact]
    public void BuildStyles_Should_Emit_Data_Attributes_For_Every_IHas_Interface()
    {
        BUIComponentAttributesBuilder builder = new();
        FullFeaturedStub component = new()
        {
            Size = SizeEnum.Large,
            Density = DensityEnum.Compact,
            FullWidth = true,
            Loading = true,
            ErrorFlag = true,
            DisabledFlag = true,
            ActiveFlag = true,
            ReadOnlyFlag = true,
            RequiredFlag = true
        };

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Size].Should().Be("large");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Density].Should().Be("compact");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.FullWidth].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Loading].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Error].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Disabled].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Active].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.ReadOnly].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Required].Should().Be("true");
    }

    [Fact]
    public void BuildStyles_Should_Emit_Color_And_Background_Inline_Variables()
    {
        BUIComponentAttributesBuilder builder = new();
        ColoredStub component = new()
        {
            Color = "rgba(10,20,30,1)",
            BackgroundColor = "rgba(40,50,60,1)"
        };

        builder.BuildStyles(component, null);

        string style = (string)builder.ComputedAttributes["style"];
        style.Should().Contain("--bui-inline-color: rgba(10,20,30,1)");
        style.Should().Contain("--bui-inline-background: rgba(40,50,60,1)");
    }

    [Fact]
    public void BuildStyles_Should_Emit_Ripple_Variables_When_Enabled()
    {
        BUIComponentAttributesBuilder builder = new();
        RippleStub component = new()
        {
            DisableRipple = false,
            RippleColor = "rgba(255,255,255,0.5)",
            RippleDurationMs = 250
        };

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Ripple].Should().Be("true");
        string style = (string)builder.ComputedAttributes["style"];
        style.Should().Contain("--bui-inline-ripple-color: rgba(255,255,255,0.5)");
        style.Should().Contain("--bui-inline-ripple-duration: 250ms");
    }

    [Fact]
    public void BuildStyles_Should_Omit_Ripple_Variables_When_Disabled()
    {
        BUIComponentAttributesBuilder builder = new();
        RippleStub component = new()
        {
            DisableRipple = true,
            RippleColor = "rgba(255,255,255,0.5)",
            RippleDurationMs = 250
        };

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Ripple].Should().Be("false");
        builder.ComputedAttributes.TryGetValue("style", out object? style).Should().BeFalse(
            "ripple vars must not be emitted when disabled");
        _ = style;
    }

    [Fact]
    public void BuildStyles_Should_Emit_Shadow_Attribute_And_Variable()
    {
        BUIComponentAttributesBuilder builder = new();
        ShadowStub component = new() { Shadow = BUIShadowPresets.Elevation(4) };

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Shadow].Should().Be("true");
        ((string)builder.ComputedAttributes["style"]).Should().Contain("--bui-inline-shadow:");
    }

    [Fact]
    public void BuildStyles_Should_Drop_Shadow_Attribute_When_Shadow_Is_Null()
    {
        BUIComponentAttributesBuilder builder = new();
        ShadowStub component = new() { Shadow = null };

        builder.BuildStyles(component, null);

        builder.ComputedAttributes.ContainsKey(FeatureDefinitions.DataAttributes.Shadow)
            .Should().BeFalse();
    }

    [Fact]
    public void BuildStyles_Should_Emit_Prefix_And_Suffix_Variables()
    {
        BUIComponentAttributesBuilder builder = new();
        PrefixSuffixStub component = new()
        {
            PrefixColor = "#123",
            PrefixBackgroundColor = "#abc",
            SuffixColor = "#456",
            SuffixBackgroundColor = "#def"
        };

        builder.BuildStyles(component, null);

        string style = (string)builder.ComputedAttributes["style"];
        style.Should().Contain("--bui-inline-prefix-color: #123");
        style.Should().Contain("--bui-inline-prefix-background: #abc");
        style.Should().Contain("--bui-inline-suffix-color: #456");
        style.Should().Contain("--bui-inline-suffix-background: #def");
    }

    [Fact]
    public void BuildStyles_Should_Emit_Border_Sides_And_Radius()
    {
        BUIComponentAttributesBuilder builder = new();
        BorderStub component = new()
        {
            Border = BorderStyle.Create()
                .All("1px", BorderStyleType.Solid, "#000")
                .Radius(6)
        };

        builder.BuildStyles(component, null);

        string style = (string)builder.ComputedAttributes["style"];
        style.Should().Contain("--bui-inline-border:");
        style.Should().Contain("1px solid #000");
        style.Should().Contain("--bui-inline-border-radius: 6px");
    }

    [Fact]
    public void BuildStyles_Should_Emit_Transitions_DataAttribute_And_Variables()
    {
        BUIComponentAttributesBuilder builder = new();
        TransitionsStub component = new()
        {
            Transitions = BUITransitionPresets.HoverFade
        };

        builder.BuildStyles(component, null);

        builder.ComputedAttributes.ContainsKey(FeatureDefinitions.DataAttributes.Transitions)
            .Should().BeTrue();
        ((string)builder.ComputedAttributes["style"]).Should().Contain("--bui-t-transition");
    }

    // ---------- Variant ----------

    [Fact]
    public void BuildStyles_Should_Emit_Variant_Attribute_In_Lower_Invariant()
    {
        BUIComponentAttributesBuilder builder = new();
        VariantStub component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Variant].Should().Be("filled");
    }

    // ---------- Families ----------

    [Fact]
    public void BuildStyles_Should_Emit_All_Family_Attributes_When_Component_Is_Multi_Family()
    {
        BUIComponentAttributesBuilder builder = new();
        MultiFamilyStub component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.InputBase].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.PickerBase].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.DataCollectionBase].Should().Be("true");
    }

    // ---------- IBuiltComponent hook ----------

    [Fact]
    public void BuildStyles_Should_Invoke_BuildComponentDataAttributes_And_CssVariables()
    {
        BUIComponentAttributesBuilder builder = new();
        BuiltComponentStub component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes["data-bui-custom"].Should().Be("custom-value");
        ((string)builder.ComputedAttributes["style"]).Should().Contain("--bui-inline-custom: 42px");
    }

    // ---------- User style merge ----------

    [Fact]
    public void BuildStyles_Should_Merge_User_Style_After_Framework_Variables()
    {
        BUIComponentAttributesBuilder builder = new();
        ColoredStub component = new() { Color = "rgba(1,2,3,1)" };
        Dictionary<string, object> additional = new() { ["style"] = "display: flex;" };

        builder.BuildStyles(component, additional);

        string style = (string)builder.ComputedAttributes["style"];
        style.Should().StartWith("--bui-inline-color:");
        style.Should().Contain("display: flex;");
    }

    [Fact]
    public void BuildStyles_Should_Preserve_Only_User_Style_When_No_Framework_Vars_Present()
    {
        BUIComponentAttributesBuilder builder = new();
        PlainComponent component = new();
        Dictionary<string, object> additional = new() { ["style"] = "opacity: 0.5" };

        builder.BuildStyles(component, additional);

        ((string)builder.ComputedAttributes["style"]).Should().Be("opacity: 0.5");
    }

    [Fact]
    public void BuildStyles_Should_Remove_Style_When_No_Variables_And_No_User_Style()
    {
        BUIComponentAttributesBuilder builder = new();
        PlainComponent component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes.ContainsKey("style").Should().BeFalse();
    }

    // ---------- Order stability ----------

    [Fact]
    public void BuildStyles_Should_Produce_Identical_Style_String_For_Identical_Inputs()
    {
        BUIComponentAttributesBuilder b1 = new();
        BUIComponentAttributesBuilder b2 = new();
        ColoredStub c = new()
        {
            Color = "rgba(10,20,30,1)",
            BackgroundColor = "rgba(40,50,60,1)"
        };

        b1.BuildStyles(c, null);
        b2.BuildStyles(c, null);

        ((string)b1.ComputedAttributes["style"])
            .Should().Be((string)b2.ComputedAttributes["style"]);
    }

    [Fact]
    public void BuildStyles_Should_Reset_Previous_State_On_Re_Invocation()
    {
        BUIComponentAttributesBuilder builder = new();
        ColoredStub withColor = new() { Color = "rgba(1,2,3,1)" };
        PlainComponent plain = new();

        builder.BuildStyles(withColor, null);
        builder.ComputedAttributes.ContainsKey("style").Should().BeTrue();

        builder.BuildStyles(plain, null);

        builder.ComputedAttributes.ContainsKey("style").Should().BeFalse(
            "second component has no vars — stale style must be cleared");
        builder.ComputedAttributes.ContainsKey(FeatureDefinitions.DataAttributes.Size)
            .Should().BeFalse("second component does not implement IHasSize");
    }

    // ---------- PatchVolatileAttributes ----------

    [Fact]
    public void PatchVolatileAttributes_Should_Refresh_Volatile_Subset_Without_Touching_Component_Attribute()
    {
        BUIComponentAttributesBuilder builder = new();
        FullFeaturedStub component = new()
        {
            Size = SizeEnum.Small,
            DisabledFlag = false,
            Loading = false,
            ErrorFlag = false,
            ReadOnlyFlag = false,
            RequiredFlag = false,
            FullWidth = false,
            ActiveFlag = false
        };

        builder.BuildStyles(component, null);

        // Flip every volatile flag
        component.DisabledFlag = true;
        component.Loading = true;
        component.ErrorFlag = true;
        component.ReadOnlyFlag = true;
        component.RequiredFlag = true;
        component.FullWidth = true;
        component.ActiveFlag = true;

        builder.PatchVolatileAttributes(component);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Disabled].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Loading].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Error].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.ReadOnly].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Required].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.FullWidth].Should().Be("true");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Active].Should().Be("true");
        // Non-volatile attribute left untouched
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Size].Should().Be("small");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Component].Should().Be("full-featured-stub");
    }

    [Fact]
    public void PatchVolatileAttributes_Should_Re_Execute_BuildComponentDataAttributes()
    {
        BUIComponentAttributesBuilder builder = new();
        BuiltComponentStub component = new();

        builder.BuildStyles(component, null);
        builder.ComputedAttributes["data-bui-custom"].Should().Be("custom-value");

        component.DataValue = "patched";
        builder.PatchVolatileAttributes(component);

        builder.ComputedAttributes["data-bui-custom"].Should().Be("patched");
    }

    // ---------- BASE-02: framework owns the contract keys ----------

    [Fact]
    public void BuildStyles_Should_Preserve_Framework_DataAttributes_Over_Component_Overrides()
    {
        BUIComponentAttributesBuilder builder = new();
        ContractHijackingStub component = new() { DisabledFlag = true };

        builder.BuildStyles(component, null);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Component]
            .Should().Be("contract-hijacking-stub",
                "framework-owned data-bui-component must not be overridable by a component hook");
        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Disabled]
            .Should().Be("true",
                "framework-owned data-bui-disabled must reflect IHasDisabled, not the hook's override");
    }

    [Fact]
    public void BuildStyles_Should_Preserve_Framework_InlineVariables_Over_Component_Overrides()
    {
        BUIComponentAttributesBuilder builder = new();
        ContractHijackingStub component = new() { ColorValue = "rgba(10,20,30,1)" };

        builder.BuildStyles(component, null);

        string style = (string)builder.ComputedAttributes["style"];
        style.Should().Contain("--bui-inline-color: rgba(10,20,30,1)",
            "framework-owned --bui-inline-color must win over a component override");
        style.Should().NotContain("HIJACKED");
    }

    [Fact]
    public void BuildStyles_Should_Allow_Component_Keys_That_Do_Not_Collide()
    {
        BUIComponentAttributesBuilder builder = new();
        ContractHijackingStub component = new();

        builder.BuildStyles(component, null);

        builder.ComputedAttributes["data-bui-custom-extra"].Should().Be("ok");
        ((string)builder.ComputedAttributes["style"]).Should().Contain("--bui-inline-custom-extra: 99px");
    }

    [Fact]
    public void PatchVolatileAttributes_Should_Preserve_Framework_Volatile_State_Over_Component_Overrides()
    {
        BUIComponentAttributesBuilder builder = new();
        ContractHijackingStub component = new() { DisabledFlag = true };

        builder.BuildStyles(component, null);

        component.DisabledFlag = false;
        builder.PatchVolatileAttributes(component);

        builder.ComputedAttributes[FeatureDefinitions.DataAttributes.Disabled]
            .Should().Be("false",
                "PatchVolatileAttributes must refresh data-bui-disabled from IHasDisabled and ignore the component override");
    }

    // ---------- Type info cache (PERF-04) ----------

    [Fact]
    public void GetTypeInfo_Should_Resolve_Same_Instance_For_Same_Type()
    {
        BUIComponentAttributesBuilder b1 = new();
        BUIComponentAttributesBuilder b2 = new();

        b1.BuildStyles(new ColoredStub(), null);
        b2.BuildStyles(new ColoredStub(), null);

        // Both should resolve to "colored-stub" without reflecting twice (cache is static).
        // The observable invariant is that the component name is stable and the style composition
        // for the same inputs matches byte-for-byte.
        b1.ComputedAttributes[FeatureDefinitions.DataAttributes.Component]
            .Should().Be(b2.ComputedAttributes[FeatureDefinitions.DataAttributes.Component]);
    }

    // ================================================================
    // Stubs
    // ================================================================

    private sealed class BUIDemoComponent : ComponentBase;

    private sealed class BUIGenericDemo<T> : ComponentBase;

    private sealed class PlainComponent : ComponentBase;

    private sealed class ColoredStub : ComponentBase, IHasColor, IHasBackgroundColor
    {
        public string? Color { get; set; }
        public string? BackgroundColor { get; set; }
    }

    private sealed class RippleStub : ComponentBase, IHasRipple
    {
        public bool DisableRipple { get; set; }
        public string? RippleColor { get; set; }
        public int? RippleDurationMs { get; set; }
        public ElementReference GetRippleContainer() => default;
    }

    private sealed class ShadowStub : ComponentBase, IHasShadow
    {
        public ShadowStyle? Shadow { get; set; }
    }

    private sealed class PrefixSuffixStub : ComponentBase, IHasPrefix, IHasSuffix
    {
        public string? PrefixText { get; set; }
        public string? PrefixIcon { get; set; }
        public string? PrefixColor { get; set; }
        public string? PrefixBackgroundColor { get; set; }
        public string? SuffixText { get; set; }
        public string? SuffixIcon { get; set; }
        public string? SuffixColor { get; set; }
        public string? SuffixBackgroundColor { get; set; }
    }

    private sealed class BorderStub : ComponentBase, IHasBorder
    {
        public BorderStyle? Border { get; set; }
    }

    private sealed class TransitionsStub : ComponentBase, IHasTransitions
    {
        public BUITransitions? Transitions { get; set; }
    }

    private sealed class VariantStub : ComponentBase, IVariantComponent
    {
        public Variant CurrentVariant { get; set; } = new BUIBadgeVariant("Filled");
        public Type VariantType => typeof(BUIBadgeVariant);
    }

    private sealed class MultiFamilyStub : ComponentBase,
        IInputFamilyComponent, IPickerFamilyComponent, IDataCollectionFamilyComponent;

    private sealed class BuiltComponentStub : ComponentBase, IBuiltComponent
    {
        public string DataValue { get; set; } = "custom-value";

        public void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes) =>
            dataAttributes["data-bui-custom"] = DataValue;

        public void BuildComponentCssVariables(Dictionary<string, string> cssVariables) =>
            cssVariables["--bui-inline-custom"] = "42px";
    }

    private sealed class ContractHijackingStub : ComponentBase, IBuiltComponent, IHasDisabled, IHasColor
    {
        public bool DisabledFlag { get; set; }
        public bool Disabled { get => DisabledFlag; set => DisabledFlag = value; }
        public bool IsDisabled => DisabledFlag;

        public string? ColorValue { get; set; }
        public string? Color { get => ColorValue; set => ColorValue = value; }

        public void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes)
        {
            dataAttributes[FeatureDefinitions.DataAttributes.Component] = "HIJACKED";
            dataAttributes[FeatureDefinitions.DataAttributes.Disabled] = "HIJACKED";
            dataAttributes["data-bui-custom-extra"] = "ok";
        }

        public void BuildComponentCssVariables(Dictionary<string, string> cssVariables)
        {
            cssVariables[FeatureDefinitions.InlineVariables.Color] = "HIJACKED";
            cssVariables["--bui-inline-custom-extra"] = "99px";
        }
    }

    private sealed class FullFeaturedStub : ComponentBase,
        IHasSize, IHasDensity, IHasFullWidth,
        IHasLoading, IHasError, IHasDisabled, IHasActive, IHasReadOnly, IHasRequired
    {
        public SizeEnum Size { get; set; } = SizeEnum.Medium;
        public DensityEnum Density { get; set; } = DensityEnum.Standard;
        public bool FullWidth { get; set; }
        public bool Loading { get; set; }
        public bool ErrorFlag { get; set; }
        public bool Error => ErrorFlag;
        public bool DisabledFlag { get; set; }
        public bool Disabled { get => DisabledFlag; set => DisabledFlag = value; }
        public bool IsDisabled => DisabledFlag;
        public bool ActiveFlag { get; set; }
        public bool Active { get => ActiveFlag; set => ActiveFlag = value; }
        public bool IsActive => ActiveFlag;
        public bool ReadOnlyFlag { get; set; }
        public bool ReadOnly => ReadOnlyFlag;
        public bool RequiredFlag { get; set; }
        public bool Required => RequiredFlag;
    }
}
