using CdCSharp.BlazorUI.SyntaxHighlight.Engines;
using CdCSharp.BlazorUI.SyntaxHighlight.Patterns;
using NSubstitute; // Cambio de namespace

namespace CdCSharp.BlazorUI.SyntaxHighlight.Tests;

public class HighlighterTests
{
    private readonly Highlighter _highlighter;

    public HighlighterTests() => _highlighter = new Highlighter(new HtmlEngine { UseCss = true });

    [Fact]
    public void Highlight_ShouldCallEngineHighlight_WhenDefinitionExists()
    {
        // Arrange
        IEngine mockEngine = Substitute.For<IEngine>(); // Sintaxis NSubstitute
        Definition mockDefinition = Languages.Definitions.CSharpDefinition;
        Highlighter highlighter = new(mockEngine);

        mockEngine
            .Highlight(Arg.Any<Definition>(), Arg.Any<string>())
            .Returns("highlighted code");

        // Act
        string result = highlighter.Highlight("csharp", "sample code");

        // Assert
        // Verificamos que se llamó con los argumentos específicos
        mockEngine.Received(1).Highlight(mockDefinition, "sample code");
        Assert.Equal("highlighted code", result);
    }

    [Fact]
    public void Highlight_ShouldEscapeHtmlCharactersInInput()
    {
        // Arrange
        string input = "<div>int x = 0;</div>";
        string definitionName = "csharp";

        // Act
        string result = _highlighter.Highlight(definitionName, input);

        // Assert
        Assert.DoesNotContain("<div>", result);
        Assert.Contains("&lt;div&gt;", result);
    }

    [Fact]
    public void Highlight_ShouldHandleOperatorsCorrectly()
    {
        // Arrange
        string input = "int x = a + b;";
        string definitionName = "csharp";

        // Act
        string result = _highlighter.Highlight(definitionName, input);

        // Assert
        Assert.Contains(@"<span class=""CSharpOperator"">=</span>", result);
        Assert.Contains(@"<span class=""CSharpOperator"">+</span>", result);
    }

    [Fact]
    public void Highlight_ShouldHighlightCommentsCorrectly()
    {
        // Arrange
        string input = "// This is a comment\nint x = 0;";
        string definitionName = "csharp";

        // Act
        string result = _highlighter.Highlight(definitionName, input);

        // Assert
        Assert.Contains("<span class=\"CSharpComment\">// This is a comment\n</span>", result);
    }

    [Fact]
    public void Highlight_ShouldHighlightCSharpKeywordsCorrectly()
    {
        // Arrange
        string input = "if (true) { return; }";
        string definitionName = "csharp";

        // Act
        string result = _highlighter.Highlight(definitionName, input);

        // Assert
        Assert.Contains("<span class=\"CSharpStatement\">if</span>", result);
        Assert.Contains("<span class=\"CSharpKeyword\">true</span>", result);
        Assert.Contains("<span class=\"CSharpStatement\">return</span>", result);
    }

    [Fact]
    public void Highlight_ShouldHighlightMultilineCommentsCorrectly()
    {
        // Arrange
        string input = "/* Multi-line\nComment */\nint y = 1;";
        string definitionName = "csharp";

        // Act
        string result = _highlighter.Highlight(definitionName, input);

        // Assert
        Assert.Contains("<span class=\"CSharpMultiLineComment\">/* Multi-line\nComment */</span>", result);
    }

    [Fact]
    public void Highlight_ShouldHighlightStringLiteralsCorrectly()
    {
        // Arrange
        string input = "string text = \"Hello, World!\";";
        string definitionName = "csharp";

        // Act
        string result = _highlighter.Highlight(definitionName, input);

        // Assert
        Assert.Contains(@"<span class=""CSharpString"">&quot;Hello, World!&quot;</span>", result);
    }

    [Fact]
    public void Highlight_ShouldHighlightVerbatimStringLiteralsCorrectly()
    {
        // Arrange
        string input = @"string path = @""C:\Temp\Files"";";
        string definitionName = "csharp";

        // Act
        string result = _highlighter.Highlight(definitionName, input);

        // Assert
        Assert.Contains("<span class=\"CSharpVerbatimString\">@&quot;C:\\Temp\\Files&quot;</span>", result);
    }

    [Fact]
    public void Highlight_ShouldThrowArgumentNullException_WhenDefinitionNameIsNull()
    {
        // Arrange
        IEngine mockEngine = Substitute.For<IEngine>();
        Highlighter highlighter = new(mockEngine);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => highlighter.Highlight(null!, "sample code"));
    }

    [Fact]
    public void Highlight_ShouldUseDefaultLanguage_WhenDefinitionNameIsInvalid()
    {
        // Arrange
        string input = @"string path = @""C:\Temp\Files"";";
        string definitionName = "invalidDefinitionName";

        // Act
        string result = _highlighter.Highlight(definitionName, input);

        // Assert
        Assert.Contains("<span class=\"CSharpVerbatimString\">@&quot;C:\\Temp\\Files&quot;</span>", result);
    }
}