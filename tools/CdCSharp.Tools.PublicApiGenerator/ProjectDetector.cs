namespace GeneratePublicApi;

using System.Xml.Linq;

/// <summary>
/// Detecta si un proyecto .csproj referencia Microsoft.CodeAnalysis.PublicApiAnalyzers.
/// Parsea el XML directamente para no depender de que el workspace esté cargado.
/// </summary>
internal static class ProjectDetector
{
    private const string PackageName = "Microsoft.CodeAnalysis.PublicApiAnalyzers";

    /// <summary>
    /// Devuelve <c>true</c> si el .csproj contiene una referencia a PublicApiAnalyzers,
    /// ya sea como PackageReference o como ProjectReference indirecta a un analyzer pack.
    /// </summary>
    public static bool UsesPublicApiAnalyzers(string csprojPath)
    {
        if (!File.Exists(csprojPath)) return false;

        try
        {
            var doc = XDocument.Load(csprojPath);

            // Buscamos en todo el XML (ignoramos namespaces de MSBuild si los hay)
            return doc.Descendants()
                .Any(e => IsPublicApiAnalyzersReference(e));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"  [WARN] No se pudo leer {csprojPath}: {ex.Message}");
            return false;
        }
    }

    private static bool IsPublicApiAnalyzersReference(XElement element)
    {
        // <PackageReference Include="Microsoft.CodeAnalysis.PublicApiAnalyzers" ... />
        if (element.Name.LocalName is "PackageReference")
        {
            string include = element.Attribute("Include")?.Value ?? "";
            return include.Equals(PackageName, StringComparison.OrdinalIgnoreCase);
        }

        // Soporte para props/targets que importan el paquete mediante una propiedad
        // <Import Project="..." /> que contiene el nombre (heurística)
        if (element.Name.LocalName is "Import")
        {
            string project = element.Attribute("Project")?.Value ?? "";
            return project.Contains(PackageName, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
