using CdCSharp.BlazorUI.Components.Abstractions;

namespace CdCSharp.BlazorUI.Tests.Integration.Templates.Components;

public class TestVariant : Variant
{
    public static readonly TestVariant Alternative = new("Alternative");

    public static readonly TestVariant Default = new("Default");

    public TestVariant(string name) : base(name)
    {
    }

    public static TestVariant Custom(string name) => new(name);
}