using CdCSharp.BlazorUI.Components.Utils.Patterns.JsInterop;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Utils.Patterns.Abstractions;

public abstract class BUIBasePattern : ComponentBase, ITextPatternJsCallback, IAsyncDisposable
{
    protected ElementReference _containerBox;
    protected TextPatternCallbacksRelay? _jsCallbacksRelay;

    private string _text = string.Empty;
    private string _prevText = string.Empty;
    private string _prevFormat = string.Empty;
    private bool _isInitialized = false;
    private bool _needsRefresh = false;

    [Parameter, EditorRequired]
    public string Format { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string DefaultText { get; set; } = string.Empty;

    [Parameter]
    public bool Editable { get; set; } = true;

    [Parameter]
    public EventCallback<string> TextChanged { get; set; }

    [Parameter]
    public Func<string, bool>? IsValidFunction { get; set; }

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
            _needsRefresh = true;
        }
    }

    public string DisplayText => string.IsNullOrEmpty(_text) ? DefaultText : _text;

    protected override async Task OnParametersSetAsync()
    {
        if (_isInitialized && _needsRefresh)
        {
            _needsRefresh = false;
            await RefreshPattern();
            await TextChanged.InvokeAsync(_text);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsCallbacksRelay = new TextPatternCallbacksRelay(this);
            _isInitialized = true;
            await RefreshPattern();
        }
    }

    protected override bool ShouldRender()
    {
        bool should = _prevText != Text || _prevFormat != Format;
        _prevText = Text;
        _prevFormat = Format;
        return should;
    }

    protected async Task RefreshPattern()
    {
        if (!_isInitialized || _jsCallbacksRelay == null) return;

        List<ElementPattern> patterns = PreparePatterns();
        await Js.TextPatternAddDynamicAsync(
            _containerBox,
            patterns,
            _jsCallbacksRelay.DotNetReference,
            nameof(NotifyTextChanged),
            nameof(ValidatePartial));
    }

    protected abstract List<ElementPattern> PreparePatterns();

    protected virtual bool ValidateFinal(string text)
    {
        return IsValidFunction?.Invoke(text) ?? true;
    }

    public virtual async Task NotifyTextChanged(string text)
    {
        if (!ValidateFinal(text))
        {
            _text = DefaultText;
            _needsRefresh = true;
            StateHasChanged();
            return;
        }

        _text = text;
        await TextChanged.InvokeAsync(_text);
    }

    public abstract Task<bool> ValidatePartial(int index, string text);

    public async ValueTask DisposeAsync()
    {
        _jsCallbacksRelay?.Dispose();
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    protected virtual ValueTask DisposeAsyncCore()
    {
        return ValueTask.CompletedTask;
    }
}