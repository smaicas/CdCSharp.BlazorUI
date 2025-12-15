using CdCSharp.BlazorUI.Core.Components.Abstractions;

namespace CdCSharp.BlazorUI.Tests.Integration.Templates.Components;

public class TestVariant : Variant
{
    public TestVariant(string name) : base(name)
    {
    }

    public static readonly TestVariant Default = new("Default");
    public static readonly TestVariant Alternative = new("Alternative");
    public static TestVariant Custom(string name) => new(name);
}
