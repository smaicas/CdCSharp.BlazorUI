using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class TransitionsCssGenerator : IAssetGenerator
{
    public string FileName => "_transition-classes.css";
    public string Name => "Transitions CSS";

    private static readonly string[] SupportedProperties =
    [
        "scale", "rotate", "translate",
        "opacity",
        "filter", "backdrop-filter",
        "box-shadow", "text-shadow",
        "color", "background-color", "border-color", "outline-color",
        "background",
        "border-radius", "outline", "outline-offset",
        "padding", "gap"
    ];

    private static readonly (string Name, string ChildPseudo, string SelfPseudo)[] Triggers =
    [
        ("hover",  ":hover:not(:has(:disabled))",  ":hover:not(:disabled)"),
        ("focus",  ":focus-within",                 ":focus-visible"),
        ("active", ":active:not(:has(:disabled))",  ":active:not(:disabled)")
    ];

    public async Task<string> GetContent()
    {
        StringBuilder sb = new();

        sb.AppendLine("""
/* ========================================
   Transition Classes
   Auto-generated - Do not edit manually
   ======================================== */

/* === BASE: apply transition shorthand to target element === */

bui-component[data-bui-transitions] .transition-target,
bui-component[data-bui-transitions].transition-target {
    transition: var(--bui-t-transition);
}
""");

        foreach ((string triggerName, string childPseudo, string selfPseudo) in Triggers)
        {
            sb.AppendLine($"/* === {triggerName.ToUpperInvariant()} === */");
            sb.AppendLine();

            foreach (string prop in SupportedProperties)
            {
                string token = $"{triggerName}:{prop}";
                string variable = $"--bui-t-{triggerName}-{prop}";

                sb.AppendLine(
$$"""
bui-component[data-bui-transitions~="{{token}}"]{{childPseudo}} .transition-target,
bui-component[data-bui-transitions~="{{token}}"].transition-target{{selfPseudo}} {
    {{prop}}: var({{variable}});
}
""");
            }
        }

        return sb.ToString();
    }
}