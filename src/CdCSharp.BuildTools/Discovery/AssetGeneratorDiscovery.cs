using CdCSharp.BuildTools.Attributes;
using System.Reflection;

namespace CdCSharp.BuildTools.Discovery;

public static class AssetGeneratorDiscovery
{
    public static IReadOnlyList<IAssetGenerator> Discover()
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                !t.IsAbstract &&
                typeof(IAssetGenerator).IsAssignableFrom(t) &&
                t.GetCustomAttribute<AssetGeneratorAttribute>() != null)
            .Select(t =>
            {
                return (IAssetGenerator)Activator.CreateInstance(t)!;
            })
            .OrderBy(g =>
                g.GetType()
                 .GetCustomAttribute<AssetGeneratorAttribute>()!.Order)
            .ToList();
    }
}