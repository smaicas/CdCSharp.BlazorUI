# generate-public-api

Herramienta .NET que genera automáticamente `PublicAPI.Unshipped.txt` para todos los
proyectos de una solución que usan `Microsoft.CodeAnalysis.PublicApiAnalyzers`.

## Características

| Característica | Detalle |
|---|---|
| **Detección automática** | Parsea los `.csproj` y busca `PackageReference` a `PublicApiAnalyzers` |
| **Soporte Razor** | Usa `GetSourceGeneratedDocumentsAsync()` para incluir el código C# generado por el Razor Source Generator |
| **Aislamiento por proyecto** | Solo incluye símbolos declarados en los ficheros del propio proyecto, no los de proyectos referenciados |
| **Formato exacto** | Produce el formato que espera el analizador (`-> ReturnType`, `get ->`, `set ->`, etc.) |
| **Soporte nullable** | Cabecera `#nullable enable` e indicadores `!` en tipos de referencia no nulos |

## Requisitos

- .NET 10 SDK
- La solución debe compilar (el workspace de Roslyn necesita resolver dependencias)

## Instalación

### Como herramienta dotnet global

```bash
# Desde el directorio del proyecto de la herramienta
dotnet pack -c Release
dotnet tool install --global --add-source ./nupkg GeneratePublicApi
```

### Ejecución directa

```bash
cd GeneratePublicApi
dotnet run -- <ruta/a/tu/solucion.sln> --overwrite
```

## Uso

```
generate-public-api [<solucion.sln>] [opciones]

ARGUMENTOS:
  <solucion.sln>    Ruta al .sln. Si se omite, busca uno en el directorio actual.

OPCIONES:
  --overwrite, -o   Sobrescribe PublicAPI.Unshipped.txt si ya existe.
  --no-nullable     Omite la cabecera '#nullable enable'.
  --help, -h        Muestra esta ayuda.
```

## Ejemplo de salida

Para un proyecto con esta API pública:

```csharp
// MiLibreria/Componentes/MiComponente.razor
[Parameter] public string Titulo { get; set; } = "";
[Parameter] public EventCallback OnClick { get; set; }

// MiLibreria/Servicios/MiServicio.cs
public class MiServicio
{
    public string ObtenerDatos(int id, bool incluirExtra = false) => "";
    public event EventHandler? DatosActualizados;
}
```

Genera `PublicAPI.Unshipped.txt`:

```
#nullable enable
MiLibreria.Componentes.MiComponente
MiLibreria.Componentes.MiComponente.OnClick.add -> void
MiLibreria.Componentes.MiComponente.OnClick.remove -> void
MiLibreria.Componentes.MiComponente.Titulo.get -> string!
MiLibreria.Componentes.MiComponente.Titulo.set -> void
MiLibreria.Servicios.MiServicio
MiLibreria.Servicios.MiServicio.DatosActualizados.add -> void
MiLibreria.Servicios.MiServicio.DatosActualizados.remove -> void
MiLibreria.Servicios.MiServicio.MiServicio() -> void
MiLibreria.Servicios.MiServicio.ObtenerDatos(int id, bool incluirExtra = false) -> string!
```

## Arquitectura

```
GeneratePublicApi/
├── Program.cs              # Punto de entrada, orquestación
├── CliOptions.cs           # Parsing de argumentos CLI
├── ProjectDetector.cs      # Detecta si un .csproj usa PublicApiAnalyzers
├── OwnFileCollector.cs     # Recopila rutas de ficheros del proyecto (incl. Razor)
├── PublicApiExtractor.cs   # Recorre los símbolos de la compilación
└── SymbolFormatter.cs      # Formatea símbolos en el formato de PublicApiAnalyzers
```

## Cómo funciona el soporte de Razor

Los ficheros `.razor` se compilan mediante el **Razor Source Generator**
(`Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator`), que produce
ficheros C# en memoria con nombres como `MiComponente.razor.g.cs`.

Roslyn expone estos documentos mediante `Project.GetSourceGeneratedDocumentsAsync()`.
La herramienta los incluye en el conjunto de "ficheros propios" del proyecto, de modo
que los `[Parameter]`, métodos y propiedades de los componentes Razor quedan reflejados
en el `PublicAPI.Unshipped.txt`.

> **Nota**: Para que el Source Generator de Razor funcione en el workspace, la solución
> debe poder restaurar sus paquetes NuGet correctamente. Ejecuta `dotnet restore` antes
> de usar la herramienta si tienes errores de carga.

## Limitaciones conocidas y próximos pasos

- **Blazor WASM**: Los proyectos WASM tipicamente no forman parte de la API pública
  en el sentido tradicional, pero si tienen `PublicApiAnalyzers` la herramienta los
  procesará igualmente.
- **Partial classes**: Si un tipo `partial` tiene partes en varios ficheros y alguna
  está en un fichero de una referencia (poco común), solo se emitirán las partes
  propias del proyecto.
- **Compilación incremental**: Esta versión regenera siempre desde cero. Una versión
  futura podría mergear con un `PublicAPI.Shipped.txt` existente.
- **Formato exacto del analizador**: El formato está basado en ingeniería inversa del
  analizador. Si hay discrepancias, compara la salida con la que genera el code fix
  `RS0016` y ajusta `SymbolFormatter.cs`.
