using CdCSharp.BlazorUI.Components.Utils.Patterns.JsInterop;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Utils.Patterns.Abstractions;

public abstract class BUIBasePattern : ComponentBase, ITextPatternJsCallback
{
    protected ElementReference _containerBox;
    protected TextPatternCallbacksRelay? _jsCallbacksRelay;

    private string _prevText = string.Empty;
    private string _prevFormat = string.Empty;
    protected string _text = string.Empty;

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
        get => string.IsNullOrEmpty(_text) ? DefaultText : _text;
        set
        {
            if (_text == value) return;
            _text = value;
            InvokeAsync(RefreshPattern);
            TextChanged.InvokeAsync(_text);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _jsCallbacksRelay = new TextPatternCallbacksRelay(this);
        await RefreshPattern();
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
        List<ElementPattern> patterns = PreparePatterns();
        await Js.TextPatternAddDynamicAsync(
            _containerBox,
            patterns,
            _jsCallbacksRelay!.DotNetReference,
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
            await RefreshPattern();
            return;
        }

        Text = text;
    }
    public abstract Task<bool> ValidatePartial(int index, string text);
}