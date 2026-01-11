using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace CdCSharp.BlazorUI.Components;

public abstract class BUIDataGridColumnBase<TItem> : ComponentBase
{
    private bool _registered;

    [CascadingParameter(Name = "DataGridColumnRegistry")]
    internal IDataGridColumnRegistry<TItem>? Registry { get; set; }

    [Parameter] public string? Header { get; set; }
    [Parameter] public TableColumnAlign Align { get; set; } = TableColumnAlign.Left;
    [Parameter] public string? Width { get; set; }
    [Parameter] public string? HeaderClass { get; set; }
    [Parameter] public string? CellClass { get; set; }
    [Parameter] public bool Visible { get; set; } = true;
    [Parameter] public RenderFragment<TItem>? Template { get; set; }

    // DataGrid-specific
    [Parameter] public bool Sortable { get; set; }
    [Parameter] public bool Filterable { get; set; }

    protected override void OnParametersSet()
    {
        if (Registry != null && !_registered)
        {
            DataGridColumnRegistration<TItem> registration = CreateRegistration();
            Registry.RegisterColumn(registration);
            _registered = true;
        }
    }

    protected abstract DataGridColumnRegistration<TItem> CreateRegistration();
}

public abstract class BUIDataGridColumnBase<TItem, TValue> : BUIDataGridColumnBase<TItem>
{
    [Parameter] public Expression<Func<TItem, TValue>>? Property { get; set; }
    [Parameter] public string? Format { get; set; }
    [Parameter] public Func<TItem, TItem, int>? CustomComparer { get; set; }
    [Parameter] public Func<TItem, string, bool>? CustomFilter { get; set; }

    protected Func<TItem, object?>? CompiledSelector { get; private set; }
    protected string? PropertyName { get; private set; }

    protected override void OnParametersSet()
    {
        if (Property != null)
        {
            Func<TItem, TValue> compiled = Property.Compile();
            CompiledSelector = item => compiled(item);

            if (Property.Body is MemberExpression memberExpr)
            {
                PropertyName = memberExpr.Member.Name;

                if (string.IsNullOrEmpty(Header))
                {
                    Header = PropertyName;
                }
            }
        }

        base.OnParametersSet();
    }
}