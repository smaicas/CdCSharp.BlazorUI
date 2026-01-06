using CdCSharp.BlazorUI.Components.Utils.Patterns.JsInterop;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Utils.Patterns.Abstractions;

public abstract class BUIBasePattern : ComponentBase, IPatternJsCallback, IAsyncDisposable
{
    protected ElementReference _containerBox;
    protected PatternCallbacksRelay? _jsCallbacksRelay;
    protected PatternState _patternState = new();

    private string? _text = null;
    private bool _isInitialized = false;
    private bool _suppressRender = false;
    private int? _activeSpanIndex = null;

    [Parameter, EditorRequired]
    public string Format { get; set; } = string.Empty;

    [Parameter]
    public bool Editable { get; set; } = true;

    [Parameter]
    public EventCallback<string?> TextChanged { get; set; }

    [Inject]
    protected IPatternJsInterop Js { get; set; } = default!;

    [Parameter]
    public string? Text
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

    protected override bool ShouldRender()
    {
        if (_suppressRender)
        {
            _suppressRender = false;
            return false;
        }
        return true;
    }

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
    protected abstract void InitializeFromText(string? text);
    protected abstract bool ValidateComplete(string text);

    public async Task OnSpanInput(int index, string value)
    {
        if (!IsValidIndex(index)) return;

        SpanState span = _patternState.Spans[index];
        if (!span.IsEditable) return;

        string filtered = FilterInput(value, span.AllowedChars, span.MaxLength);

        if (filtered != value)
        {
            await Js.UpdateSpanValueAsync(ComponentId, index, filtered);
            await Js.SetCaretToEndAsync(ComponentId, index);
        }

        span.Value = filtered;

        // Suprimir render durante la notificación
        _suppressRender = true;
        await NotifyTextChanged();
    }

    public async Task<bool> OnSpanComplete(int index, string value)
    {
        if (!IsValidIndex(index)) return false;

        SpanState span = _patternState.Spans[index];
        bool isValid = span.Validator?.Invoke(value) ?? true;

        if (!isValid)
        {
            span.Value = string.Empty;
            await Js.UpdateSpanValueAsync(ComponentId, index, span.Placeholder);
            await Js.SelectSpanContentAsync(ComponentId, index);
            _suppressRender = true;
            await NotifyTextChanged();
            return false;
        }

        _suppressRender = true;
        await NotifyTextChanged();
        return true;
    }

    public async Task OnSpanFocus(int index)
    {
        if (!IsValidIndex(index)) return;
        _activeSpanIndex = index;
    }

    public async Task OnSpanBlur(int index)
    {
        if (!IsValidIndex(index)) return;

        _activeSpanIndex = null;

        SpanState span = _patternState.Spans[index];

        if (span.IsEditable && (!span.IsComplete || string.IsNullOrEmpty(span.Value)))
        {
            span.Value = string.Empty;
            await Js.UpdateSpanValueAsync(ComponentId, index, span.Placeholder);
            _suppressRender = true;
            await NotifyTextChanged();
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
        // Get the actual text only if pattern is complete
        string? actualText = _patternState.IsComplete ? _patternState.GetActualText() : null;

        if (_text != actualText)
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