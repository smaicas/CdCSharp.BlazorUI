using CdCSharp.BlazorUI.Components.Generic.Button;

namespace CdCSharp.BlazorUI.Tests.Integration.Variants;

public class MyCustomButtonVariants : UIButtonVariant
{
    public MyCustomButtonVariants(string name) : base(name)
    {
    }

    public static readonly UIButtonVariant MyCustom1 = Custom("MyCustom1");
    public static readonly UIButtonVariant MyCustom2 = Custom("MyCustom2");
}
