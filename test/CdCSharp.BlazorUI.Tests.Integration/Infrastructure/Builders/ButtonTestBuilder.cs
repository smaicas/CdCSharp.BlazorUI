using Bunit;
using CdCSharp.BlazorUI.Components.Generic.Button;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Builders;

public class ButtonTestBuilder
{
    private readonly TestContextBase _context;
    private UIButtonVariant _variant = UIButtonVariant.Primary;
    private string _content = "Test Button";
    private bool _disabled = false;
    private EventCallback<MouseEventArgs>? _onClick;

    public ButtonTestBuilder(TestContextBase context)
    {
        _context = context;
    }

    public ButtonTestBuilder WithVariant(UIButtonVariant variant)
    {
        _variant = variant;
        return this;
    }

    public ButtonTestBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public ButtonTestBuilder Disabled()
    {
        _disabled = true;
        return this;
    }

    public ButtonTestBuilder WithClick(Action action)
    {
        _onClick = EventCallback.Factory.Create<MouseEventArgs>(_context, action);
        return this;
    }

    public IRenderedComponent<UIButton> Build()
    {
        return _context.Render<UIButton>(parameters =>
        {
            parameters
                .Add(p => p.Variant, _variant)
                .Add(p => p.ChildContent, _content)
                .Add(p => p.Disabled, _disabled);

            if (_onClick.HasValue)
            {
                parameters.Add(p => p.OnClick, _onClick.Value);
            }
        });
    }
}
