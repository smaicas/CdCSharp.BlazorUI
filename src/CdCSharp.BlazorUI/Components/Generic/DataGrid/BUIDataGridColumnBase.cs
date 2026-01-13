using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace CdCSharp.BlazorUI.Components;

public abstract class BUIDataGridColumnBase<TItem> : ComponentBase
{
    private bool _registered;

    [Parameter] public TableColumnAlign Align { get; set; } = TableColumnAlign.Left;

    [Parameter] public string? CellClass { get; set; }

    [Parameter] public bool Filterable { get; set; }

    [Parameter] public string? Header { get; set; }

    [Parameter] public string? HeaderClass { get; set; }

    // DataGrid-specific
    [Parameter] public bool Sortable { get; set; }

    [Parameter] public RenderFragment<TItem>? Template { get; set; }

    [Parameter] public bool Visible { get; set; } = true;

    [Parameter] public string? Width { get; set; }

    [CascadingParameter(Name = "DataGridColumnRegistry")]
    internal IDataGridColumnRegistry<TItem>? Registry { get; set; }

    protected abstract DataGridColumnRegistration<TItem> CreateRegistration();

    protected override void OnParametersSet()
    {
        if (Registry != null && !_registered)
        {
            DataGridColumnRegistration<TItem> registration = CreateRegistration();
            Registry.RegisterColumn(registration);
            _registered = true;
        }
    }
}

public abstract class BUIDataGridColumnBase<TItem, TValue> : BUIDataGridColumnBase<TItem>
{
    [Parameter] public Func<TItem, TItem, int>? CustomComparer { get; set; }
    [Parameter] public Func<TItem, string, bool>? CustomFilter { get; set; }
    [Parameter] public string? Format { get; set; }
    [Parameter] public Expression<Func<TItem, TValue>>? Property { get; set; }
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