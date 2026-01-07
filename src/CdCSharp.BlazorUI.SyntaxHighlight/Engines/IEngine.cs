using CdCSharp.BlazorUI.SyntaxHighlight.Patterns;

namespace CdCSharp.BlazorUI.SyntaxHighlight.Engines;

public interface IEngine
{
    string Highlight(Definition definition, string input);
}