using CdCSharp.BlazorUI.Components.Utils.Patterns.JsInterop;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Utils.Patterns.Abstractions;

public abstract class BUIBasePattern : ComponentBase, IPatternJsCallback, IAsyncDisposable
{
    protected ElementReference _containerBox;
    protected PatternCallbacksRelay? _jsCallbacksRelay;
    protected PatternState _patternState = new();

    private string _text = string.Empty;
    private bool _isInitialized = false;

    [Parameter, EditorRequired]
    public string Format { get; set; } = string.Empty;

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

    protected string ComponentId { get; } = $"pattern_{Guid.NewGuid():N}";

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
            await Js.InitializePatternAsync(_containerBox, _jsCallbacksRelay.DotNetReference, ComponentId);
            _isInitialized = true;
        }
    }

    protected abstract PatternState CreatePatternState();
    protected abstract void InitializeFromText(string text);
    protected abstract bool ValidateComplete(string text);

    public async Task OnSpanInput(int index, string value)
    {
        if (!IsValidIndex(index)) return;

        SpanState span = _patternState.Spans[index];
        if (!span.IsEditable) return;

        // Filter input based on allowed characters
        string filtered = FilterInput(value, span.AllowedChars, span.MaxLength);

        // Update in JS if filtered
        if (filtered != value)
        {
            await Js.UpdateSpanValueAsync(ComponentId, index, filtered);
            return;
        }

        // Update state
        span.Value = filtered;
        StateHasChanged();
    }

    public async Task<bool> OnSpanComplete(int index, string value)
    {
        if (!IsValidIndex(index)) return false;

        SpanState span = _patternState.Spans[index];

        // Validate with span validator if exists
        bool isValid = span.Validator?.Invoke(value) ?? true;

        if (!isValid)
        {
            // Reset to empty and update JS
            span.Value = string.Empty;
            await Js.UpdateSpanValueAsync(ComponentId, index, "");
            await Js.SelectSpanContentAsync(ComponentId, index);
            StateHasChanged();
            return false;
        }

        // Update text if all editable spans have values
        await NotifyTextChanged();
        return true;
    }

    public async Task OnSpanFocus(int index)
    {
        if (!IsValidIndex(index)) return;

        // Select content is handled in TypeScript
        await Task.CompletedTask;
    }

    public async Task OnSpanBlur(int index)
    {
        if (!IsValidIndex(index)) return;

        SpanState span = _patternState.Spans[index];

        // If incomplete, clear it
        if (span.IsEditable && (!span.IsComplete || string.IsNullOrEmpty(span.Value)))
        {
            span.Value = string.Empty;
            await Js.UpdateSpanValueAsync(ComponentId, index, span.Placeholder);
            StateHasChanged();
        }
    }

    public async Task OnPaste(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        // Try to validate complete text
        string normalized = NormalizeSeparators(text);

        if (ValidateComplete(normalized))
        {
            InitializeFromText(normalized);
            await NotifyTextChanged();
            StateHasChanged();
        }
    }

    protected virtual string NormalizeSeparators(string text)
    {
        // Default implementation - can be overridden
        return text;
    }

    private string FilterInput(string input, string allowedChars, int maxLength)
    {
        System.Text.StringBuilder result = new();

        foreach (char c in input)
        {
            if (result.Length >= maxLength) break;

            bool valid = allowedChars switch
            {
                "d" => char.IsDigit(c),
                "w" => char.IsLetter(c),
                "a" => char.IsLetterOrDigit(c),
                _ => true
            };

            if (valid) result.Append(c);
        }

        return result.ToString();
    }

    private bool IsValidIndex(int index)
        => index >= 0 && index < _patternState.Spans.Count;

    private async Task NotifyTextChanged()
    {
        string actualText = _patternState.GetActualText();

        if (!string.IsNullOrEmpty(actualText) && actualText != _text)
        {
            _text = actualText;
            await TextChanged.InvokeAsync(_text);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsCallbacksRelay != null && _isInitialized)
        {
            await Js.DisposePatternAsync(ComponentId);
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