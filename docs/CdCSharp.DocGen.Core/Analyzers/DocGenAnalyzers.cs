using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;
using ProjectInfo = CdCSharp.DocGen.Core.Models.ProjectInfo;
using TypeInfo = CdCSharp.DocGen.Core.Models.TypeInfo;
using TypeKind = CdCSharp.DocGen.Core.Models.TypeKind;

namespace CdCSharp.DocGen.Core.Analysis;

public class PublicApiAnalyzer
{
    private readonly ILogger _logger;

    public PublicApiAnalyzer(ILogger? logger = null)
    {
        _logger = logger ?? new NullLogger();
    }

    public async Task<List<TypeInfo>> AnalyzeAsync(string projectRoot, List<Models.FileInfo> csFiles)
    {
        _logger.Verbose($"Analyzing public API from {csFiles.Count} C# files...");

        List<TypeInfo> types = [];
        int analyzed = 0;

        foreach (Models.FileInfo file in csFiles.Where(f => f.Type == FileType.CSharp))
        {
            string fullPath = Path.Combine(projectRoot, file.RelativePath);
            if (!File.Exists(fullPath))
            {
                _logger.Warning($"File not found: {file.RelativePath}");
                continue;
            }

            try
            {
                string code = await File.ReadAllTextAsync(fullPath);
                CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(code).GetCompilationUnitRoot();

                TypeVisitor visitor = new(file.RelativePath, file.Importance);
                visitor.Visit(root);

                types.AddRange(visitor.Types);
                analyzed++;

                if (visitor.Types.Count > 0)
                    _logger.Verbose($"  {file.RelativePath}: {visitor.Types.Count} public types");
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to analyze {file.RelativePath}: {ex.Message}");
            }
        }

        _logger.Verbose($"Analyzed {analyzed} files, found {types.Count} public types");
        return types;
    }

    private class TypeVisitor(string filePath, ImportanceLevel fileImportance) : CSharpSyntaxWalker
    {
        private string _namespace = string.Empty;
        public List<TypeInfo> Types { get; } = [];

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            _namespace = node.Name.ToString();
            base.VisitNamespaceDeclaration(node);
        }

        public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        {
            _namespace = node.Name.ToString();
            base.VisitFileScopedNamespaceDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (IsPublic(node.Modifiers))
                Types.Add(CreateTypeInfo(node, TypeKind.Class));
            base.VisitClassDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            if (IsPublic(node.Modifiers))
                Types.Add(CreateTypeInfo(node, TypeKind.Interface));
            base.VisitInterfaceDeclaration(node);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            if (IsPublic(node.Modifiers))
                Types.Add(CreateTypeInfo(node, TypeKind.Struct));
            base.VisitStructDeclaration(node);
        }

        public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
        {
            if (IsPublic(node.Modifiers))
                Types.Add(CreateTypeInfo(node, TypeKind.Record));
            base.VisitRecordDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            if (IsPublic(node.Modifiers))
                Types.Add(CreateTypeInfo(node, TypeKind.Enum));
            base.VisitEnumDeclaration(node);
        }

        private TypeInfo CreateTypeInfo(BaseTypeDeclarationSyntax node, TypeKind kind)
        {
            List<MemberInfo> members = ExtractMembers(node);
            List<AttributeInfo> attrs = ExtractAttributes(node.AttributeLists);

            return new TypeInfo
            {
                Name = node.Identifier.Text,
                Namespace = _namespace,
                FilePath = filePath,
                Kind = kind,
                PublicMembers = members,
                BaseTypes = node.BaseList?.Types.Select(t => t.Type.ToString()).ToList() ?? [],
                Attributes = attrs,
                Importance = DetermineImportance(attrs, members.Count)
            };
        }

        private ImportanceLevel DetermineImportance(List<AttributeInfo> attrs, int memberCount)
        {
            if (attrs.Any(a => a.Name.Contains("Generator"))) return ImportanceLevel.Critical;
            if (fileImportance >= ImportanceLevel.High) return fileImportance;
            if (memberCount > 10) return ImportanceLevel.High;
            return ImportanceLevel.Normal;
        }

        private static List<MemberInfo> ExtractMembers(BaseTypeDeclarationSyntax node)
        {
            if (node is not TypeDeclarationSyntax typeDecl) return [];

            List<MemberInfo> members = [];
            foreach (MemberDeclarationSyntax member in typeDecl.Members)
            {
                if (member is MethodDeclarationSyntax method && IsPublic(method.Modifiers))
                {
                    string pars = string.Join(", ", method.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"));
                    members.Add(new MemberInfo
                    {
                        Name = method.Identifier.Text,
                        Signature = $"{method.ReturnType} {method.Identifier}({pars})",
                        Kind = MemberKind.Method
                    });
                }
                else if (member is PropertyDeclarationSyntax prop && IsPublic(prop.Modifiers))
                {
                    members.Add(new MemberInfo
                    {
                        Name = prop.Identifier.Text,
                        Signature = $"{prop.Type} {prop.Identifier}",
                        Kind = MemberKind.Property
                    });
                }
                else if (member is FieldDeclarationSyntax field && IsPublic(field.Modifiers))
                {
                    foreach (VariableDeclaratorSyntax v in field.Declaration.Variables)
                    {
                        members.Add(new MemberInfo
                        {
                            Name = v.Identifier.Text,
                            Signature = $"{field.Declaration.Type} {v.Identifier}",
                            Kind = MemberKind.Field
                        });
                    }
                }
            }
            return members;
        }

        private static List<AttributeInfo> ExtractAttributes(SyntaxList<AttributeListSyntax> attrLists)
        {
            List<AttributeInfo> attrs = [];
            foreach (AttributeListSyntax list in attrLists)
            {
                foreach (AttributeSyntax attr in list.Attributes)
                {
                    attrs.Add(new AttributeInfo
                    {
                        Name = attr.Name.ToString(),
                        Arguments = attr.ArgumentList?.Arguments.Select(a => a.ToString()).ToList() ?? []
                    });
                }
            }
            return attrs;
        }

        private static bool IsPublic(SyntaxTokenList modifiers) =>
            modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
    }
}

public class ComponentAnalyzer
{
    private readonly ILogger _logger;

    private static readonly Regex ParameterRegex = new(
    @"\[Parameter\]\s*(?:\[[^\]]+\]\s*)*public\s+(?:required\s+)?([\w\.<>\?,]+)\s+(\w+)",
    RegexOptions.Compiled | RegexOptions.Multiline);

    public ComponentAnalyzer(ILogger? logger = null)
    {
        _logger = logger ?? new NullLogger();
    }

    public async Task<List<ComponentInfo>> AnalyzeAsync(string projectRoot, List<Models.FileInfo> razorFiles)
    {
        _logger.Verbose($"Analyzing {razorFiles.Count} Razor components...");

        List<ComponentInfo> components = [];

        foreach (Models.FileInfo file in razorFiles.Where(f => f.Type == FileType.Razor))
        {
            string fullPath = Path.Combine(projectRoot, file.RelativePath);
            if (!File.Exists(fullPath))
            {
                _logger.Warning($"File not found: {file.RelativePath}");
                continue;
            }

            try
            {
                string code = await File.ReadAllTextAsync(fullPath);
                string name = Path.GetFileNameWithoutExtension(file.RelativePath);

                List<ParameterInfo> parameters = [];
                foreach (System.Text.RegularExpressions.Match match in ParameterRegex.Matches(code))
                {
                    if (match.Groups.Count > 2)
                    {
                        parameters.Add(new ParameterInfo
                        {
                            Name = match.Groups[2].Value,
                            Type = match.Groups[1].Value,
                            IsRequired = code.Contains($"required {match.Groups[1].Value} {match.Groups[2].Value}")
                        });
                    }
                }

                components.Add(new ComponentInfo
                {
                    Name = name,
                    FilePath = file.RelativePath,
                    Parameters = parameters,
                    HasCodeBlock = code.Contains("@code")
                });

                if (parameters.Count > 0)
                    _logger.Verbose($"  {name}: {parameters.Count} parameters");
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to analyze component {file.RelativePath}: {ex.Message}");
            }
        }

        _logger.Verbose($"Found {components.Count} components");
        return components;
    }
}

public class PatternDetector
{
    private readonly ILogger _logger;

    public PatternDetector(ILogger? logger = null)
    {
        _logger = logger ?? new NullLogger();
    }

    public List<PatternInfo> Detect(ProjectInfo project)
    {
        _logger.Verbose("Detecting architectural patterns...");

        List<PatternInfo> patterns = [];
        patterns.AddRange(DetectSourceGenerators(project));
        patterns.AddRange(DetectBlazor(project));
        patterns.AddRange(DetectDependencyInjection(project));
        patterns.AddRange(DetectRepository(project));

        if (patterns.Count > 0)
            _logger.Verbose($"Detected {patterns.Count} patterns");

        return patterns;
    }

    private List<PatternInfo> DetectSourceGenerators(ProjectInfo project)
    {
        List<TypeInfo> generators = project.PublicTypes
            .Where(t => t.Attributes.Any(a => a.Name.Contains("Generator")))
            .ToList();

        if (generators.Count == 0) return [];

        _logger.Verbose($"  Source Generators: {generators.Count} types");

        return [new PatternInfo
        {
            Name = "Source Generators",
            Description = "Roslyn source generators for compile-time code generation",
            Type = PatternType.SourceGenerator,
            AffectedFiles = generators.Select(t => t.FilePath).ToList()
        }];
    }

    private List<PatternInfo> DetectBlazor(ProjectInfo project)
    {
        List<Models.FileInfo> razorFiles = project.Files.Where(f => f.Type == FileType.Razor).ToList();
        if (razorFiles.Count == 0) return [];

        _logger.Verbose($"  Blazor Components: {razorFiles.Count} files");

        return [new PatternInfo
        {
            Name = "Blazor Components",
            Description = "Reusable Blazor UI components",
            Type = PatternType.Blazor,
            AffectedFiles = razorFiles.Select(f => f.RelativePath).ToList()
        }];
    }

    private List<PatternInfo> DetectDependencyInjection(ProjectInfo project)
    {
        List<TypeInfo> diTypes = project.PublicTypes
            .Where(t => t.Name.EndsWith("Extensions") &&
                       t.PublicMembers.Any(m => m.Name.StartsWith("Add")))
            .ToList();

        if (diTypes.Count == 0) return [];

        _logger.Verbose($"  Dependency Injection: {diTypes.Count} extension types");

        return [new PatternInfo
        {
            Name = "Dependency Injection",
            Description = "Service registration extensions",
            Type = PatternType.DependencyInjection,
            AffectedFiles = diTypes.Select(t => t.FilePath).ToList()
        }];
    }

    private List<PatternInfo> DetectRepository(ProjectInfo project)
    {
        List<TypeInfo> repos = project.PublicTypes
            .Where(t => t.Name.Contains("Repository") || t.Name.Contains("Service"))
            .Where(t => t.PublicMembers.Any(m =>
                m.Name.Contains("Get") || m.Name.Contains("Save") || m.Name.Contains("Delete")))
            .ToList();

        if (repos.Count < 3) return [];

        _logger.Verbose($"  Repository Pattern: {repos.Count} repository/service types");

        return [new PatternInfo
        {
            Name = "Repository Pattern",
            Description = "Data access abstraction",
            Type = PatternType.Repository,
            AffectedFiles = repos.Select(t => t.FilePath).ToList()
        }];
    }
}