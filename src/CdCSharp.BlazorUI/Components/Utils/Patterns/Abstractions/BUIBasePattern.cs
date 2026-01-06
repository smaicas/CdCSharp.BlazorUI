using CdCSharp.BlazorUI.Components.Utils.Patterns.JsInterop;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Utils.Patterns.Abstractions;

public abstract class BUIBasePattern : ComponentBase, IPatternJsCallback, IAsyncDisposable
{
    protected ElementReference _containerBox;
    protected PatternCallbacksRelay? _jsCallbacksRelay;
    protected PatternState _patternState = new();
    protected int _focusedSpanIndex = -1;

    private string _text = string.Empty;
    private bool _isInitialized = false;
    private readonly string _componentId = $"pattern_{Guid.NewGuid():N}";

    [Parameter, EditorRequired]
    public string Format { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string DefaultText { get; set; } = string.Empty;

    [Parameter]
    public bool Editable { get; set; } = true;

    [Parameter]
    public EventCallback<string> TextChanged { get; set; }

    [Inject]
    protected IPatternJsInterop Js { get; set; } = default!;

    [Parameter]
    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value;
            if (_isInitialized)
            {
                InitializeFromText(_text);
                StateHasChanged();
            }
        }
    }

    protected string ComponentId => _componentId;

    protected override void OnInitialized()
    {
        _patternState = CreatePatternState();
        InitializeFromText(Text);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsCallbacksRelay = new PatternCallbacksRelay(this);
            await Js.InitializePatternAsync(_containerBox, _jsCallbacksRelay.DotNetReference, _componentId);
            _isInitialized = true;
        }
    }

    protected abstract PatternState CreatePatternState();
    protected abstract void InitializeFromText(string text);
    protected abstract bool ValidateSpan(int index, string value);
    protected abstract bool ValidateComplete(string text);

    public async Task HandleSpanInput(int index, string value)
    {
        if (index < 0 || index >= _patternState.Spans.Count) return;

        SpanState span = _patternState.Spans[index];
        if (!span.IsEditable) return;

        // Filter input based on span pattern and max length
        string filtered = FilterInput(value, span.Pattern, span.MaxLength);

        // If we're typing the first character and the span had default value, clear it
        if (value.Length == 1 && span.Value == string.Empty && span.DisplayValue == span.DefaultValue)
        {
            // Just starting to type, accept the character
            span.Value = filtered;
            StateHasChanged();
            await NotifyTextChanged();
            return;
        }

        if (filtered != value)
        {
            await Js.UpdateSpanValueAsync(_componentId, index, filtered);
            return;
        }

        span.Value = filtered;
        StateHasChanged();

        // If span is complete, validate
        if (span.IsComplete)
        {
            if (ValidateSpan(index, span.Value))
            {
                await MoveToNextSpan(index);
            }
            else
            {
                // Reset to default
                span.Value = string.Empty;
                await Js.UpdateSpanValueAsync(_componentId, index, span.DefaultValue);
                await Js.SelectSpanContentAsync(_componentId, index);
                StateHasChanged();
            }
        }

        await NotifyTextChanged();
    }

    public async Task HandleSpanFocus(int index)
    {
        if (index < 0 || index >= _patternState.Spans.Count) return;

        _focusedSpanIndex = index;
        await Js.SelectSpanContentAsync(_componentId, index);
    }

    public async Task HandleSpanBlur(int index)
    {
        if (index < 0 || index >= _patternState.Spans.Count) return;

        SpanState span = _patternState.Spans[index];

        if (!span.IsComplete && !string.IsNullOrEmpty(span.Value))
        {
            span.Value = string.Empty;
            await NotifyTextChanged();
            StateHasChanged();
        }
    }

    public async Task HandlePaste(string text)
    {
        // Try to parse the pasted text with normalized separators
        string normalized = NormalizeSeparators(text);

        if (ValidateComplete(normalized))
        {
            InitializeFromText(normalized);
            await NotifyTextChanged();
            StateHasChanged();
        }
    }

    private string FilterInput(string input, string pattern, int maxLength)
    {
        System.Text.StringBuilder result = new();

        foreach (char c in input)
        {
            if (result.Length >= maxLength) break;

            bool valid = pattern switch
            {
                "d" => char.IsDigit(c),
                "w" => char.IsLetter(c),
                _ => true
            };

            if (valid) result.Append(c);
        }

        return result.ToString();
    }

    protected virtual string NormalizeSeparators(string text)
    {
        // Replace common separators with the expected ones based on format
        return text
            .Replace('-', '/')
            .Replace('.', '/')
            .Replace(',', '/')
            .Replace('\\', '/')
            .Replace(' ', '/');
    }

    private async Task MoveToNextSpan(int currentIndex)
    {
        int nextIndex = -1;

        for (int i = currentIndex + 1; i < _patternState.Spans.Count; i++)
        {
            if (_patternState.Spans[i].IsEditable)
            {
                nextIndex = i;
                break;
            }
        }

        if (nextIndex >= 0)
        {
            await Js.FocusSpanAsync(_componentId, nextIndex);
        }
    }

    private async Task NotifyTextChanged()
    {
        string actualText = _patternState.GetActualText();

        if (!string.IsNullOrEmpty(actualText))
        {
            _text = actualText;
            await TextChanged.InvokeAsync(_text);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsCallbacksRelay != null && _isInitialized)
        {
            await Js.DisposePatternAsync(_componentId);
            _jsCallbacksRelay.Dispose();
        }
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    protected virtual ValueTask DisposeAsyncCore()
    {
        return ValueTask.CompletedTask;
    }
}