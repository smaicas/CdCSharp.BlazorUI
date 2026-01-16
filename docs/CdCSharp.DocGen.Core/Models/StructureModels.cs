using System.Text.Json.Serialization;

namespace CdCSharp.DocGen.Core.Models;

public record ProjectStructure
{
    [JsonPropertyName("solution")]
    public string Solution { get; init; } = string.Empty;

    [JsonPropertyName("rootPath")]
    public string RootPath { get; init; } = string.Empty;

    [JsonPropertyName("assemblies")]
    public List<AssemblyInfo> Assemblies { get; init; } = [];

    [JsonPropertyName("globalSummary")]
    public GlobalSummary GlobalSummary { get; init; } = new();
}

public record AssemblyInfo
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;

    [JsonPropertyName("isTestProject")]
    public bool IsTestProject { get; init; }

    [JsonPropertyName("references")]
    public List<string> References { get; init; } = [];

    [JsonPropertyName("files")]
    public AssemblyFiles Files { get; init; } = new();

    [JsonPropertyName("summary")]
    public AssemblySummary Summary { get; init; } = new();
}

public record AssemblyFiles
{
    [JsonPropertyName("csharp")]
    public List<string> CSharp { get; init; } = [];

    [JsonPropertyName("razor")]
    public List<string> Razor { get; init; } = [];

    [JsonPropertyName("typescript")]
    public List<string> TypeScript { get; init; } = [];

    [JsonPropertyName("css")]
    public List<string> Css { get; init; } = [];

    [JsonPropertyName("other")]
    public List<string> Other { get; init; } = [];
}

public record AssemblySummary
{
    [JsonPropertyName("classes")]
    public int Classes { get; set; }

    [JsonPropertyName("interfaces")]
    public int Interfaces { get; set; }

    [JsonPropertyName("records")]
    public int Records { get; set; }

    [JsonPropertyName("structs")]
    public int Structs { get; set; }

    [JsonPropertyName("enums")]
    public int Enums { get; set; }

    [JsonPropertyName("delegates")]
    public int Delegates { get; set; }

    [JsonPropertyName("components")]
    public int Components { get; set; }

    [JsonPropertyName("generators")]
    public int Generators { get; set; }

    [JsonPropertyName("tsModules")]
    public int TsModules { get; set; }

    [JsonPropertyName("cssFiles")]
    public int CssFiles { get; set; }

    [JsonPropertyName("cssVariables")]
    public int CssVariables { get; set; }
}

public record GlobalSummary
{
    [JsonPropertyName("totalAssemblies")]
    public int TotalAssemblies { get; set; }

    [JsonPropertyName("totalTestProjects")]
    public int TotalTestProjects { get; set; }

    [JsonPropertyName("totalClasses")]
    public int TotalClasses { get; set; }

    [JsonPropertyName("totalInterfaces")]
    public int TotalInterfaces { get; set; }

    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; set; }

    [JsonPropertyName("totalComponents")]
    public int TotalComponents { get; set; }

    [JsonPropertyName("totalGenerators")]
    public int TotalGenerators { get; set; }

    [JsonPropertyName("totalTsModules")]
    public int TotalTsModules { get; set; }

    [JsonPropertyName("totalCssFiles")]
    public int TotalCssFiles { get; set; }

    [JsonPropertyName("detectedPatterns")]
    public List<string> DetectedPatterns { get; init; } = [];

    [JsonPropertyName("projectType")]
    public string ProjectType { get; set; } = "Unknown";
}