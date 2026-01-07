using CdCSharp.BlazorUI.SyntaxHighlight.Patterns;
using System.Drawing;

namespace CdCSharp.BlazorUI.SyntaxHighlight.Languages;

public partial class Definitions
{
    public static Definition RazorDefinition = new(
        name: "Razor",
        caseSensitive: true,
        style: new Style(
            new ColorPair(
                foreColor: Color.FromName("black"),
                backColor: Color.FromName("transparent")
            ),
            new Font(
                name: "monospace",
                size: 16f,
                style: FontStyle.Regular
            )
        ),
        patterns: new Dictionary<string, Pattern>()
        {
            {
                "RazorComment", new BlockPattern(
                    name: "RazorComment",
                    style: new Style(
                        new ColorPair(
                            foreColor: Color.FromName("green"),
                            backColor: Color.FromName("transparent")
                        ),
                        new Font(
                            name: "monospace",
                            size: 16f,
                            style: FontStyle.Regular
                        )
                    ),
                    beginsWith: "@*",
                    endsWith: "*@",
                    escapesWith: ""
                )
            },
            {
                "HtmlComment", new BlockPattern(
                    name: "HtmlComment",
                    style: new Style(
                        new ColorPair(
                            foreColor: Color.FromName("green"),
                            backColor: Color.FromName("transparent")
                        ),
                        new Font(
                            name: "monospace",
                            size: 16f,
                            style: FontStyle.Regular
                        )
                    ),
                    beginsWith: "&lt;!--",
                    endsWith: "--&gt;",
                    escapesWith: ""
                )
            },
            {
                "RazorCodeBlock", new BlockPattern(
                    name: "RazorCodeBlock",
                    style: new Style(
                        new ColorPair(
                            foreColor: Color.FromArgb(139,0,139),
                            backColor: Color.FromName("transparent")
                        ),
                        new Font(
                            name: "monospace",
                            size: 16f,
                            style: FontStyle.Regular
                        )
                    ),
                    beginsWith: "@code",
                    endsWith: "}",
                    escapesWith: ""
                )
            },
            {
                "RazorBlock", new BlockPattern(
                    name: "RazorBlock",
                    style: new Style(
                        new ColorPair(
                            foreColor: Color.FromArgb(139,0,139),
                            backColor: Color.FromName("transparent")
                        ),
                        new Font(
                            name: "monospace",
                            size: 16f,
                            style: FontStyle.Regular
                        )
                    ),
                    beginsWith: "@{",
                    endsWith: "}",
                    escapesWith: ""
                )
            },
            {
                "String", new BlockPattern(
                    name: "String",
                    style: new Style(
                        new ColorPair(
                            foreColor: Color.FromArgb(163,21,21),
                            backColor: Color.FromName("transparent")
                        ),
                        new Font(
                            name: "monospace",
                            size: 16f,
                            style: FontStyle.Regular
                        )
                    ),
                    beginsWith: "&quot;",
                    endsWith: "&quot;",
                    escapesWith: "\\"
                )
            },
            {
                "RazorDirective", new WordPattern(
                    name: "RazorDirective",
                    style: new Style(
                        new ColorPair(
                            foreColor: Color.FromArgb(139,0,139),
                            backColor: Color.FromName("transparent")
                        ),
                        new Font(
                            name: "monospace",
                            size: 16f,
                            style: FontStyle.Regular
                        )
                    ),
                    words:
                    [
                        "@page",
                        "@using",
                        "@inject",
                        "@inherits",
                        "@implements",
                        "@namespace",
                        "@layout",
                        "@typeparam",
                        "@attribute",
                        "@preservewhitespace",
                        "@rendermode",
                        "@formname",
                    ]
                )
            },
            {
                "RazorControlFlow", new WordPattern(
                    name: "RazorControlFlow",
                    style: new Style(
                        new ColorPair(
                            foreColor: Color.FromName("blue"),
                            backColor: Color.FromName("transparent")
                        ),
                        new Font(
                            name: "monospace",
                            size: 16f,
                            style: FontStyle.Regular
                        )
                    ),
                    words:
                    [
                        "@if",
                        "@else",
                        "@foreach",
                        "@for",
                        "@while",
                        "@switch",
                        "@case",
                        "@try",
                        "@catch",
                        "@finally",
                        "@lock",
                        "@await",
                    ]
                )
            },
            {
                "CSharpKeyword", new WordPattern(
                    name: "CSharpKeyword",
                    style: new Style(
                        new ColorPair(
                            foreColor: Color.FromName("blue"),
                            backColor: Color.FromName("transparent")
                        ),
                        new Font(
                            name: "monospace",
                            size: 16f,
                            style: FontStyle.Regular
                        )
                    ),
                    words:
                    [
                        "public",
                        "private",
                        "protected",
                        "internal",
                        "static",
                        "readonly",
                        "const",
                        "async",
                        "await",
                        "new",
                        "override",
                        "virtual",
                        "abstract",
                        "sealed",
                        "partial",
                        "class",
                        "struct",
                        "interface",
                        "enum",
                        "record",
                        "delegate",
                        "event",
                        "var",
                        "void",
                        "bool",
                        "byte",
                        "sbyte",
                        "char",
                        "decimal",
                        "double",
                        "float",
                        "int",
                        "uint",
                        "long",
                        "ulong",
                        "short",
                        "ushort",
                        "object",
                        "string",
                        "dynamic",
                        "true",
                        "false",
                        "null",
                        "this",
                        "base",
                        "typeof",
                        "sizeof",
                        "nameof",
                        "default",
                        "if",
                        "else",
                        "switch",
                        "case",
                        "for",
                        "foreach",
                        "in",
                        "while",
                        "do",
                        "break",
                        "continue",
                        "return",
                        "yield",
                        "throw",
                        "try",
                        "catch",
                        "finally",
                        "using",
                        "lock",
                        "goto",
                        "is",
                        "as",
                        "ref",
                        "out",
                        "params",
                        "get",
                        "set",
                        "init",
                        "value",
                        "where",
                        "select",
                        "from",
                        "orderby",
                        "ascending",
                        "descending",
                        "group",
                        "by",
                        "into",
                        "join",
                        "on",
                        "equals",
                        "let",
                        "when",
                        "and",
                        "or",
                        "not",
                        "with",
                        "required",
                    ]
                )
            },
            {
                "CSharpType", new WordPattern(
                    name: "CSharpType",
                    style: new Style(
                        new ColorPair(
                            foreColor: Color.FromArgb(43,145,175),
                            backColor: Color.FromName("transparent")
                        ),
                        new Font(
                            name: "monospace",
                            size: 16f,
                            style: FontStyle.Regular
                        )
                    ),
                    words:
                    [
                        "Task",
                        "ValueTask",
                        "Action",
                        "Func",
                        "List",
                        "Dictionary",
                        "HashSet",
                        "IEnumerable",
                        "IList",
                        "ICollection",
                        "IDictionary",
                        "IReadOnlyList",
                        "IReadOnlyCollection",
                        "EventCallback",
                        "RenderFragment",
                        "MarkupString",
                        "ElementReference",
                        "ComponentBase",
                        "Parameter",
                        "CascadingParameter",
                        "Inject",
                        "SupplyParameterFromQuery",
                        "EditorRequired",
                    ]
                )
            },
            {
                "Markup", new MarkupPattern(
                    name: "Markup",
                    style: new Style(
                        new ColorPair(
                            foreColor: Color.FromName("maroon"),
                            backColor: Color.FromName("transparent")
                        ),
                        new Font(
                            name: "monospace",
                            size: 16f,
                            style: FontStyle.Regular
                        )
                    ),
                    highlightAttributes: true,
                    bracketColors: new ColorPair(
                        foreColor: Color.FromName("blue"),
                        backColor: Color.FromName("transparent")
                    ),
                    attributeNameColors: new ColorPair(
                        foreColor: Color.FromName("red"),
                        backColor: Color.FromName("transparent")
                    ),
                    attributeValueColors: new ColorPair(
                        foreColor: Color.FromName("blue"),
                        backColor: Color.FromName("transparent")
                    )
                )
            },
        }
    );
}