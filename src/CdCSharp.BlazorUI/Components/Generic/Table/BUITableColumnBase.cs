using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace CdCSharp.BlazorUI.Components;

public abstract class BUITableColumnBase<TItem> : ComponentBase
{
    private bool _registered;

    [CascadingParameter(Name = "TableColumnRegistry")]
    internal ITableColumnRegistry<TItem>? Registry { get; set; }

    [Parameter] public string? Header { get; set; }
    [Parameter] public TableColumnAlign Align { get; set; } = TableColumnAlign.Left;
    [Parameter] public string? Width { get; set; }
    [Parameter] public string? HeaderClass { get; set; }
    [Parameter] public string? CellClass { get; set; }
    [Parameter] public bool Visible { get; set; } = true;
    [Parameter] public RenderFragment<TItem>? Template { get; set; }

    protected override void OnParametersSet()
    {
        if (Registry != null && !_registered)
        {
            TableColumnRegistration<TItem> registration = CreateRegistration();
            Registry.RegisterColumn(registration);
            _registered = true;
        }
    }

    protected abstract TableColumnRegistration<TItem> CreateRegistration();
}

public abstract class BUITableColumnBase<TItem, TValue> : BUITableColumnBase<TItem>
{
    [Parameter] public Expression<Func<TItem, TValue>>? Property { get; set; }
    [Parameter] public string? Format { get; set; }

    protected Func<TItem, object?>? CompiledSelector { get; private set; }

    protected override void OnParametersSet()
    {
        if (Property != null)
        {
            // Compile the expression once for performance
            Func<TItem, TValue> compiled = Property.Compile();
            CompiledSelector = item => compiled(item);

            // Extract header from property name if not specified
            if (string.IsNullOrEmpty(Header) && Property.Body is MemberExpression memberExpr)
            {
                Header = memberExpr.Member.Name;
            }
        }

        base.OnParametersSet();
    }
}