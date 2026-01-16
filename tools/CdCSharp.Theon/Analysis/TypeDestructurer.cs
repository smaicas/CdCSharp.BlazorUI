using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeInfo = CdCSharp.Theon.Models.TypeInfo;
using TypeKind = CdCSharp.Theon.Models.TypeKind;

namespace CdCSharp.Theon.Analysis;

public class TypeDestructurer
{
    private readonly TheonLogger _logger;

    public TypeDestructurer(TheonLogger logger)
    {
        _logger = logger;
    }

    public async Task<List<NamespaceInfo>> DestructureAsync(string rootPath, List<string> csFiles)
    {
        Dictionary<string, List<TypeInfo>> namespaces = [];

        foreach (string relativePath in csFiles)
        {
            string fullPath = Path.Combine(rootPath, relativePath);
            if (!File.Exists(fullPath)) continue;

            try
            {
                string code = await File.ReadAllTextAsync(fullPath);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

                foreach ((string ns, TypeInfo type) in ExtractTypes(root, relativePath))
                {
                    if (!namespaces.ContainsKey(ns))
                        namespaces[ns] = [];
                    namespaces[ns].Add(type);
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to parse {relativePath}: {ex.Message}");
            }
        }

        return namespaces
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => new NamespaceInfo
            {
                Name = kvp.Key,
                Types = kvp.Value.OrderBy(t => t.Name).ToList()
            })
            .ToList();
    }

    private static IEnumerable<(string Namespace, TypeInfo Type)> ExtractTypes(
        CompilationUnitSyntax root, string filePath)
    {
        string currentNamespace = "";

        foreach (MemberDeclarationSyntax member in root.Members)
        {
            if (member is BaseNamespaceDeclarationSyntax ns)
            {
                currentNamespace = ns.Name.ToString();
                foreach (MemberDeclarationSyntax typeMember in ns.Members)
                {
                    if (typeMember is TypeDeclarationSyntax typeDecl && IsPublicOrInternal(typeDecl.Modifiers))
                    {
                        yield return (currentNamespace, CreateTypeInfo(typeDecl, filePath));
                    }
                }
            }
        }
    }

    private static TypeInfo CreateTypeInfo(TypeDeclarationSyntax node, string filePath)
    {
        TypeKind kind = node switch
        {
            ClassDeclarationSyntax => TypeKind.Class,
            InterfaceDeclarationSyntax => TypeKind.Interface,
            RecordDeclarationSyntax => TypeKind.Record,
            StructDeclarationSyntax => TypeKind.Struct,
            _ => TypeKind.Class
        };

        string name = node.Identifier.Text;
        if (node.TypeParameterList != null)
            name += node.TypeParameterList.ToString();

        return new TypeInfo
        {
            Name = name,
            Kind = kind,
            File = filePath,
            Modifiers = node.Modifiers.Select(m => m.Text).ToList(),
            BaseTypes = node.BaseList?.Types.Select(t => t.Type.ToString()).ToList() ?? [],
            Members = ExtractMembers(node)
        };
    }

    private static List<MemberInfo> ExtractMembers(TypeDeclarationSyntax node)
    {
        List<MemberInfo> members = [];

        foreach (MemberDeclarationSyntax member in node.Members)
        {
            MemberInfo? info = member switch
            {
                MethodDeclarationSyntax m when IsPublicOrInternal(m.Modifiers) =>
                    new MemberInfo
                    {
                        Name = m.Identifier.Text,
                        Kind = MemberKind.Method,
                        Signature = $"{m.ReturnType} {m.Identifier}({FormatParams(m.ParameterList)})",
                        Modifiers = m.Modifiers.Select(x => x.Text).ToList()
                    },

                PropertyDeclarationSyntax p when IsPublicOrInternal(p.Modifiers) =>
                    new MemberInfo
                    {
                        Name = p.Identifier.Text,
                        Kind = MemberKind.Property,
                        Signature = $"{p.Type} {p.Identifier}",
                        Modifiers = p.Modifiers.Select(x => x.Text).ToList()
                    },

                ConstructorDeclarationSyntax c when IsPublicOrInternal(c.Modifiers) =>
                    new MemberInfo
                    {
                        Name = c.Identifier.Text,
                        Kind = MemberKind.Constructor,
                        Signature = $"{c.Identifier}({FormatParams(c.ParameterList)})",
                        Modifiers = c.Modifiers.Select(x => x.Text).ToList()
                    },

                _ => null
            };

            if (info != null) members.Add(info);
        }

        return members;
    }

    private static string FormatParams(ParameterListSyntax? paramList)
    {
        if (paramList == null) return "";
        return string.Join(", ", paramList.Parameters.Select(p => $"{p.Type} {p.Identifier}"));
    }

    private static bool IsPublicOrInternal(SyntaxTokenList modifiers)
    {
        return modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword) || m.IsKind(SyntaxKind.InternalKeyword));
    }
}