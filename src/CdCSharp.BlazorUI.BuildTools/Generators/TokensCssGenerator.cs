using CdCSharp.BlazorUI.BuildTools.Pipeline;
using CdCSharp.BlazorUI.Core.Tokens;
using System.Text;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

public class TokensCssGenerator : IAssetGenerator
{
    private readonly BuildContext _context;

    public string Name => "Design Tokens CSS";

    public TokensCssGenerator(BuildContext context)
    {
        _context = context;
    }

    public async Task GenerateAsync()
    {
        CssTokenProvider provider = new();
        IDictionary<string, string> tokens = provider.GetAllTokens();

        StringBuilder sb = new();
        sb.AppendLine("/* ========================================");
        sb.AppendLine("   Design Tokens");
        sb.AppendLine("   Auto-generated from C# constants");
        sb.AppendLine("   ======================================== */");
        sb.AppendLine();
        sb.AppendLine(":root {");

        string currentCategory = string.Empty;

        foreach (KeyValuePair<string, string> kvp in tokens.OrderBy(t => t.Key))
        {
            string[] parts = kvp.Key.Split('-');
            string category = parts[0];

            if (category != currentCategory)
            {
                if (!string.IsNullOrEmpty(currentCategory))
                {
                    sb.AppendLine();
                }
                sb.AppendLine($"  /* {char.ToUpper(category[0]) + category.Substring(1)} */");
                currentCategory = category;
            }

            string cssVarName = $"--bui-{kvp.Key}";
            sb.AppendLine($"  {cssVarName}: {kvp.Value};");
        }

        sb.AppendLine("}");

        string outputPath = _context.GetFullPath("CssBundle/tokens.css");
        await File.WriteAllTextAsync(outputPath, sb.ToString());
    }
}
