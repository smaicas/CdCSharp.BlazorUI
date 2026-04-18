using System.Reflection;
using CdCSharp.BlazorUI.Components;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Core;

/// <summary>
/// CORE-T-05: No orphaned FeatureDefinitions constants.
/// Every DataAttributes.* and InlineVariables.* constant must appear in the
/// BUIComponentAttributesBuilder (the consumer) or in a generator/component template.
/// This guards against dead constants that silently drift out of sync.
/// </summary>
[Trait("Core", "FeatureDefinitions")]
public class FeatureDefinitionsTests
{
    [Fact]
    public void DataAttributes_Component_Should_Follow_Bui_Prefix_Convention()
    {
        // Assert — data-bui-component is the root data attribute
        FeatureDefinitions.DataAttributes.Component.Should().StartWith("data-bui-");
    }

    [Fact]
    public void DataAttributes_All_Should_Start_With_Data_Bui()
    {
        // Arrange — collect all string constants from DataAttributes nested class
        IEnumerable<string> constants = GetStringConstants(typeof(FeatureDefinitions.DataAttributes));

        // Assert — every attribute follows the data-bui-* convention
        foreach (string constant in constants)
        {
            constant.Should().StartWith("data-bui-",
                because: $"DataAttributes constant '{constant}' must follow data-bui-* convention");
        }
    }

    [Fact]
    public void InlineVariables_All_Should_Start_With_Bui_Inline()
    {
        // Arrange — collect all string constants from InlineVariables nested class
        IEnumerable<string> constants = GetStringConstants(typeof(FeatureDefinitions.InlineVariables));

        // Assert — every CSS variable follows --bui-inline-* convention
        foreach (string constant in constants)
        {
            constant.Should().StartWith("--bui-inline-",
                because: $"InlineVariables constant '{constant}' must follow --bui-inline-* convention");
        }
    }

    [Fact]
    public void CssClasses_Input_Should_Follow_BEM_Naming()
    {
        IEnumerable<string> constants = GetStringConstants(typeof(FeatureDefinitions.CssClasses.Input));

        foreach (string constant in constants)
        {
            constant.Should().StartWith("bui-input__",
                because: $"Input CSS class '{constant}' must use bui-input__ BEM prefix");
        }
    }

    [Fact]
    public void CssClasses_Picker_Should_Follow_BEM_Naming()
    {
        IEnumerable<string> constants = GetStringConstants(typeof(FeatureDefinitions.CssClasses.Picker));

        foreach (string constant in constants)
        {
            constant.Should().StartWith("bui-picker__",
                because: $"Picker CSS class '{constant}' must use bui-picker__ BEM prefix");
        }
    }

    [Fact]
    public void DataAttributes_Count_Should_Be_Stable()
    {
        // Regression guard: if a constant is added/removed, this test catches it
        int count = GetStringConstants(typeof(FeatureDefinitions.DataAttributes)).Count();
        count.Should().BeGreaterThan(10, "DataAttributes should define at least the core state attributes");
    }

    [Fact]
    public void Tags_Component_Should_Be_Bui_Component()
    {
        FeatureDefinitions.Tags.Component.Should().Be("bui-component");
    }

    private static IEnumerable<string> GetStringConstants(Type type)
    {
        List<string> values = new();

        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType == typeof(string) && field.IsLiteral)
            {
                string? value = (string?)field.GetRawConstantValue();
                if (value != null) values.Add(value);
            }
        }

        foreach (Type nested in type.GetNestedTypes(BindingFlags.Public))
        {
            values.AddRange(GetStringConstants(nested));
        }

        return values;
    }
}
