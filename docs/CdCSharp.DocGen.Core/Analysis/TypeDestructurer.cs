using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeKind = CdCSharp.DocGen.Core.Models.TypeKind;

namespace CdCSharp.DocGen.Core.Analysis;

public class TypeDestructurer
{
    private readonly ILogger _logger;

    public TypeDestructurer(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }

    public async Task<List<DestructuredNamespace>> DestructureAsync(string rootPath, List<string> csFiles)
    {
        Dictionary<string, List<DestructuredType>> namespaces = [];

        foreach (string relativePath in csFiles)
        {
            string fullPath = Path.Combine(rootPath, relativePath);
            if (!File.Exists(fullPath))
                continue;

            try
            {
                string code = await File.ReadAllTextAsync(fullPath);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

                TypeVisitor visitor = new(relativePath);
                visitor.Visit(root);

                foreach ((string ns, DestructuredType type) in visitor.Types)
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
            .Select(kvp => new DestructuredNamespace
            {
                Name = kvp.Key,
                Types = kvp.Value.OrderBy(t => t.Name).ToList()
            })
            .ToList();
    }

    private class TypeVisitor : CSharpSyntaxWalker
    {
        private readonly string _filePath;
        private string _currentNamespace = string.Empty;
        public List<(string Namespace, DestructuredType Type)> Types { get; } = [];

        public TypeVisitor(string filePath) => _filePath = filePath;

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            _currentNamespace = node.Name.ToString();
            base.VisitNamespaceDeclaration(node);
        }

        public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        {
            _currentNamespace = node.Name.ToString();
            base.VisitFileScopedNamespaceDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (IsPublicOrInternal(node.Modifiers))
                Types.Add((_currentNamespace, CreateType(node, TypeKind.Class)));
            base.VisitClassDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            if (IsPublicOrInternal(node.Modifiers))
                Types.Add((_currentNamespace, CreateType(node, TypeKind.Interface)));
            base.VisitInterfaceDeclaration(node);
        }

        public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
        {
            if (IsPublicOrInternal(node.Modifiers))
                Types.Add((_currentNamespace, CreateType(node, TypeKind.Record)));
            base.VisitRecordDeclaration(node);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            if (IsPublicOrInternal(node.Modifiers))
                Types.Add((_currentNamespace, CreateType(node, TypeKind.Struct)));
            base.VisitStructDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            if (IsPublicOrInternal(node.Modifiers))
                Types.Add((_currentNamespace, CreateEnumType(node)));
            base.VisitEnumDeclaration(node);
        }

        public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            if (IsPublicOrInternal(node.Modifiers))
                Types.Add((_currentNamespace, CreateDelegateType(node)));
            base.VisitDelegateDeclaration(node);
        }

        private DestructuredType CreateType(TypeDeclarationSyntax node, TypeKind kind)
        {
            return new DestructuredType
            {
                Kind = kind,
                Name = GetTypeName(node),
                File = _filePath,
                Modifiers = GetModifiers(node.Modifiers),
                Base = GetBaseTypes(node),
                Attributes = GetAttributes(node.AttributeLists),
                Members = GetMembers(node),
                NestedTypes = GetNestedTypes(node)
            };
        }

        private DestructuredType CreateEnumType(EnumDeclarationSyntax node)
        {
            List<DestructuredMember> members = node.Members
                .Select(m => new DestructuredMember
                {
                    Kind = MemberKind.Field,
                    Name = m.Identifier.Text,
                    Signature = m.EqualsValue != null ? $"{m.Identifier} = {m.EqualsValue.Value}" : m.Identifier.Text,
                    Modifiers = [],
                    Attributes = []
                })
                .ToList();

            return new DestructuredType
            {
                Kind = TypeKind.Enum,
                Name = node.Identifier.Text,
                File = _filePath,
                Modifiers = GetModifiers(node.Modifiers),
                Base = node.BaseList?.Types.Select(t => t.ToString()).ToList() ?? [],
                Attributes = GetAttributes(node.AttributeLists),
                Members = members,
                NestedTypes = []
            };
        }

        private DestructuredType CreateDelegateType(DelegateDeclarationSyntax node)
        {
            string signature = $"{node.ReturnType} {node.Identifier}{node.TypeParameterList}({string.Join(", ", node.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"))})";

            return new DestructuredType
            {
                Kind = TypeKind.Delegate,
                Name = node.Identifier.Text,
                File = _filePath,
                Modifiers = GetModifiers(node.Modifiers),
                Base = [],
                Attributes = GetAttributes(node.AttributeLists),
                Members = [new DestructuredMember { Kind = MemberKind.Method, Name = "Invoke", Signature = signature, Modifiers = [], Attributes = [] }],
                NestedTypes = []
            };
        }

        private static string GetTypeName(TypeDeclarationSyntax node)
        {
            string name = node.Identifier.Text;
            if (node.TypeParameterList != null)
                name += node.TypeParameterList.ToString();
            return name;
        }

        private static List<string> GetModifiers(SyntaxTokenList modifiers)
        {
            return modifiers
                .Where(m => !m.IsKind(SyntaxKind.PartialKeyword))
                .Select(m => m.Text)
                .ToList();
        }

        private static List<string> GetBaseTypes(TypeDeclarationSyntax node)
        {
            return node.BaseList?.Types.Select(t => t.Type.ToString()).ToList() ?? [];
        }

        private static List<string> GetAttributes(SyntaxList<AttributeListSyntax> attrLists)
        {
            return attrLists
                .SelectMany(al => al.Attributes)
                .Select(a => a.Name.ToString())
                .ToList();
        }

        private List<DestructuredMember> GetMembers(TypeDeclarationSyntax node)
        {
            List<DestructuredMember> members = [];

            foreach (MemberDeclarationSyntax member in node.Members)
            {
                if (member is TypeDeclarationSyntax)
                    continue;

                DestructuredMember? m = member switch
                {
                    ConstructorDeclarationSyntax ctor when IsPublicOrInternal(ctor.Modifiers) => CreateConstructor(ctor),
                    MethodDeclarationSyntax method when IsPublicOrInternal(method.Modifiers) => CreateMethod(method),
                    PropertyDeclarationSyntax prop when IsPublicOrInternal(prop.Modifiers) => CreateProperty(prop),
                    FieldDeclarationSyntax field when IsPublicOrInternal(field.Modifiers) => CreateField(field),
                    EventDeclarationSyntax evt when IsPublicOrInternal(evt.Modifiers) => CreateEvent(evt),
                    IndexerDeclarationSyntax idx when IsPublicOrInternal(idx.Modifiers) => CreateIndexer(idx),
                    _ => null
                };

                if (m != null)
                    members.Add(m);
            }

            return members;
        }

        private List<DestructuredType> GetNestedTypes(TypeDeclarationSyntax node)
        {
            List<DestructuredType> nested = [];

            foreach (MemberDeclarationSyntax member in node.Members)
            {
                if (member is TypeDeclarationSyntax nestedType && IsPublicOrInternal(nestedType.Modifiers))
                {
                    TypeKind kind = nestedType switch
                    {
                        ClassDeclarationSyntax => TypeKind.Class,
                        InterfaceDeclarationSyntax => TypeKind.Interface,
                        RecordDeclarationSyntax => TypeKind.Record,
                        StructDeclarationSyntax => TypeKind.Struct,
                        _ => TypeKind.Class
                    };
                    nested.Add(CreateType(nestedType, kind));
                }
            }

            return nested;
        }

        private static DestructuredMember CreateConstructor(ConstructorDeclarationSyntax node)
        {
            string pars = string.Join(", ", node.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"));
            return new DestructuredMember
            {
                Kind = MemberKind.Constructor,
                Name = node.Identifier.Text,
                Signature = $"{node.Identifier}({pars})",
                Modifiers = GetModifiers(node.Modifiers),
                Attributes = GetAttributes(node.AttributeLists)
            };
        }

        private static DestructuredMember CreateMethod(MethodDeclarationSyntax node)
        {
            string pars = string.Join(", ", node.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"));
            string typeParams = node.TypeParameterList?.ToString() ?? "";
            return new DestructuredMember
            {
                Kind = MemberKind.Method,
                Name = node.Identifier.Text,
                Signature = $"{node.ReturnType} {node.Identifier}{typeParams}({pars})",
                Modifiers = GetModifiers(node.Modifiers),
                Attributes = GetAttributes(node.AttributeLists)
            };
        }

        private static DestructuredMember CreateProperty(PropertyDeclarationSyntax node)
        {
            string accessors = "";
            if (node.AccessorList != null)
            {
                List<string> acc = [];
                if (node.AccessorList.Accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)))
                    acc.Add("get");
                if (node.AccessorList.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)))
                    acc.Add("set");
                if (node.AccessorList.Accessors.Any(a => a.IsKind(SyntaxKind.InitAccessorDeclaration)))
                    acc.Add("init");
                accessors = $" {{ {string.Join("; ", acc)}; }}";
            }

            return new DestructuredMember
            {
                Kind = MemberKind.Property,
                Name = node.Identifier.Text,
                Signature = $"{node.Type} {node.Identifier}{accessors}",
                Modifiers = GetModifiers(node.Modifiers),
                Attributes = GetAttributes(node.AttributeLists)
            };
        }

        private static DestructuredMember CreateField(FieldDeclarationSyntax node)
        {
            VariableDeclaratorSyntax variable = node.Declaration.Variables.First();
            return new DestructuredMember
            {
                Kind = MemberKind.Field,
                Name = variable.Identifier.Text,
                Signature = $"{node.Declaration.Type} {variable.Identifier}",
                Modifiers = GetModifiers(node.Modifiers),
                Attributes = GetAttributes(node.AttributeLists)
            };
        }

        private static DestructuredMember CreateEvent(EventDeclarationSyntax node)
        {
            return new DestructuredMember
            {
                Kind = MemberKind.Event,
                Name = node.Identifier.Text,
                Signature = $"event {node.Type} {node.Identifier}",
                Modifiers = GetModifiers(node.Modifiers),
                Attributes = GetAttributes(node.AttributeLists)
            };
        }

        private static DestructuredMember CreateIndexer(IndexerDeclarationSyntax node)
        {
            string pars = string.Join(", ", node.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"));
            return new DestructuredMember
            {
                Kind = MemberKind.Indexer,
                Name = "this",
                Signature = $"{node.Type} this[{pars}]",
                Modifiers = GetModifiers(node.Modifiers),
                Attributes = GetAttributes(node.AttributeLists)
            };
        }

        private static bool IsPublicOrInternal(SyntaxTokenList modifiers)
        {
            return modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword) || m.IsKind(SyntaxKind.InternalKeyword));
        }
    }
}