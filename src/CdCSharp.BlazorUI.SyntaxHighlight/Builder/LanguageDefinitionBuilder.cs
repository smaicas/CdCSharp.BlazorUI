using CdCSharp.BlazorUI.SyntaxHighlight.Rules;
using CdCSharp.BlazorUI.SyntaxHighlight.Tokens;

namespace CdCSharp.BlazorUI.SyntaxHighlight.Builder;

public sealed class LanguageDefinitionBuilder
{
    private readonly string _name;
    private readonly List<ITokenRule> _rules = [];
    private bool _caseSensitive = true;
    private int _nextPriority = 1000;

    internal LanguageDefinitionBuilder(string name)
    {
        _name = name;
    }

    public LanguageDefinitionBuilder CaseSensitive(bool value = true)
    {
        _caseSensitive = value;
        return this;
    }

    public LanguageDefinitionBuilder AddDelimited(
        TokenType tokenType,
        string start,
        string end,
        string? escape = null,
        bool multiline = true,
        int? priority = null)
    {
        _rules.Add(new DelimitedRule(tokenType, start, end, escape, multiline, priority ?? _nextPriority--));
        return this;
    }

    public LanguageDefinitionBuilder AddLineComment(string start, int? priority = null)
    {
        return AddDelimited(TokenType.Comment, start, "\n", multiline: false, priority: priority);
    }

    public LanguageDefinitionBuilder AddBlockComment(string start, string end, int? priority = null)
    {
        return AddDelimited(TokenType.Comment, start, end, multiline: true, priority: priority);
    }

    public LanguageDefinitionBuilder AddString(
        string start,
        string end,
        string? escape = "\\",
        TokenType tokenType = TokenType.String,
        int? priority = null)
    {
        return AddDelimited(tokenType, start, end, escape, multiline: true, priority: priority);
    }

    public LanguageDefinitionBuilder AddKeywords(TokenType tokenType, IEnumerable<string> keywords, int? priority = null)
    {
        _rules.Add(new KeywordRule(tokenType, keywords, priority ?? _nextPriority--));
        return this;
    }

    public LanguageDefinitionBuilder AddPattern(
        TokenType tokenType,
        string pattern,
        bool requireWordBoundary = false,
        int? priority = null)
    {
        _rules.Add(new RegexRule(tokenType, pattern, requireWordBoundary, priority ?? _nextPriority--));
        return this;
    }

    public LanguageDefinitionBuilder AddBalanced(
        TokenType tokenType,
        string prefix,
        char open,
        char close,
        Func<string, IReadOnlyList<Token>>? innerTokenizer = null,
        int? priority = null,
        int maxDepth = 100)
    {
        _rules.Add(new BalancedRule(tokenType, prefix, open, close, innerTokenizer, priority ?? _nextPriority--, maxDepth));
        return this;
    }

    public LanguageDefinitionBuilder AddMarkup(int? priority = null)
    {
        _rules.Add(new MarkupRule(priority ?? _nextPriority--));
        return this;
    }

    public LanguageDefinitionBuilder AddSequence(TokenType tokenType, string sequence, int? priority = null)
    {
        _rules.Add(new SequenceRule(tokenType, sequence, priority ?? _nextPriority--));
        return this;
    }
    public LanguageDefinitionBuilder AddSequences(TokenType tokenType, IEnumerable<string> sequences, int? priority = null)
    {
        foreach (string seq in sequences.OrderByDescending(s => s.Length))
        {
            _rules.Add(new SequenceRule(tokenType, seq, priority ?? _nextPriority--));
        }
        return this;
    }
    public LanguageDefinitionBuilder AddOperators(string operators, int? priority = null)
    {
        foreach (char op in operators)
        {
            _rules.Add(new SequenceRule(TokenType.Operator, op.ToString(), priority ?? _nextPriority--));
        }
        return this;
    }

    public LanguageDefinitionBuilder AddOperators(IEnumerable<string> operators, int? priority = null)
    {
        foreach (string op in operators.OrderByDescending(o => o.Length))
        {
            _rules.Add(new SequenceRule(TokenType.Operator, op, priority ?? _nextPriority--));
        }
        return this;
    }

    public LanguageDefinitionBuilder AddPunctuation(string punctuation, int? priority = null)
    {
        foreach (char p in punctuation)
        {
            _rules.Add(new SequenceRule(TokenType.Punctuation, p.ToString(), priority ?? _nextPriority--));
        }
        return this;
    }

    public LanguageDefinitionBuilder Embed(LanguageDefinition other, int priorityOffset = 0)
    {
        foreach (ITokenRule rule in other.Rules)
        {
            _rules.Add(new PriorityOffsetRule(rule, priorityOffset));
        }
        return this;
    }

    public LanguageDefinitionBuilder AddRule(ITokenRule rule)
    {
        _rules.Add(rule);
        return this;
    }

    public LanguageDefinitionBuilder AddContextualKeywords(
    TokenType tokenType,
    IEnumerable<string> keywords,
    Func<string, int, bool> contextPredicate,
    int? priority = null)
    {
        _rules.Add(new ContextualKeywordRule(tokenType, keywords, contextPredicate, priority ?? _nextPriority--));
        return this;
    }

    public LanguageDefinitionBuilder AddContextualPattern(
    TokenType tokenType,
    string pattern,
    Func<string, int, bool> contextPredicate,
    bool requireWordBoundary = false,
    int? priority = null)
    {
        _rules.Add(new ContextualRegexRule(tokenType, pattern, contextPredicate, requireWordBoundary, priority ?? _nextPriority--));
        return this;
    }

    public LanguageDefinition Build()
    {
        return new LanguageDefinition(_name, _caseSensitive, [.. _rules]);
    }

    private sealed class PriorityOffsetRule : ITokenRule
    {
        private readonly ITokenRule _inner;

        public PriorityOffsetRule(ITokenRule inner, int offset)
        {
            _inner = inner;
            Priority = offset;
        }

        public int Priority => _inner.Priority + field;

        public TokenMatch? TryMatch(string input, int position, TokenizerContext context) =>
            _inner.TryMatch(input, position, context);
    }
}