using CdCSharp.BuildTools.Attributes;
using System.Reflection;

namespace CdCSharp.BuildTools;

public class TemplateManager
{
    private readonly BuildContext _context;

    public TemplateManager(BuildContext context)
    {
        _context = context;
    }

    public async Task EnsureTemplatesAsync()
    {
        IEnumerable<(MethodInfo Method, BuildTemplateAttribute? Attr)> methods = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetMethods(
                BindingFlags.Public | BindingFlags.Static))
            .Select(m => (Method: m, Attr: m.GetCustomAttribute<BuildTemplateAttribute>()))
            .Where(x => x.Attr != null);

        foreach ((MethodInfo? method, BuildTemplateAttribute? attr) in methods)
        {
            string path = _context.GetFullPath(attr!.RelativePath);
            if (File.Exists(path) && !attr.Overwrite)
            {
                continue;
            }

            string content = (string)method.Invoke(null, null)!;
            await File.WriteAllTextAsync(path, content);
        }
    }
}