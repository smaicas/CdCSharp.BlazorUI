namespace GeneratePublicApi;

/// <summary>
/// Opciones de línea de comandos para la herramienta.
/// </summary>
internal sealed class CliOptions
{
    public string SolutionPath    { get; private init; } = "";
    public bool   Overwrite       { get; private init; }
    public bool   NullableEnable  { get; private init; } = true;

    private CliOptions() { }

    public static CliOptions? Parse(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
        {
            PrintHelp();
            return null;
        }

        bool overwrite      = args.Contains("--overwrite") || args.Contains("-o");
        bool noNullable     = args.Contains("--no-nullable");
        string? solutionArg    = args.FirstOrDefault(a => !a.StartsWith('-'));

        string solutionPath;

        if (solutionArg is not null)
        {
            solutionPath = Path.GetFullPath(solutionArg);
        }
        else
        {
            // Buscar automáticamente un .sln en el directorio actual
            string[] slnFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sln");
            if (slnFiles.Length == 0)
            {
                Console.Error.WriteLine("[ERROR] No se encontró ningún archivo .sln. Especifícalo como argumento.");
                PrintHelp();
                return null;
            }
            if (slnFiles.Length > 1)
            {
                Console.Error.WriteLine("[ERROR] Hay más de un .sln. Especifícalo como argumento:");
                foreach (string s in slnFiles)
                    Console.Error.WriteLine($"  {s}");
                return null;
            }
            solutionPath = slnFiles[0];
        }

        if (!File.Exists(solutionPath))
        {
            Console.Error.WriteLine($"[ERROR] No existe el fichero: {solutionPath}");
            return null;
        }

        return new CliOptions
        {
            SolutionPath   = solutionPath,
            Overwrite      = overwrite,
            NullableEnable = !noNullable,
        };
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
            generate-public-api — Genera PublicAPI.Unshipped.txt para proyectos
                                  que usan Microsoft.CodeAnalysis.PublicApiAnalyzers

            USO:
              generate-public-api [<solucion.sln>] [opciones]

            ARGUMENTOS:
              <solucion.sln>    Ruta al archivo .sln. Si se omite, se busca en el
                                directorio actual.

            OPCIONES:
              --overwrite, -o   Sobrescribe PublicAPI.Unshipped.txt si ya existe.
              --no-nullable     Omite la cabecera '#nullable enable'.
              --help, -h        Muestra esta ayuda.

            EJEMPLOS:
              generate-public-api MySolution.sln --overwrite
              generate-public-api --overwrite
            """);
    }
}
