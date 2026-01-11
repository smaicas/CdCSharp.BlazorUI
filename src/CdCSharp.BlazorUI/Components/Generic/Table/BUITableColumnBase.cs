using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace CdCSharp.BlazorUI.Components;

/// <summary>
/// Base class for table column components without type inference.
/// </summary>
public abstract class BUITableColumnBase<TItem> : ComponentBase
{
    private bool _registered;

    [CascadingParameter(Name = "TableColumnRegistry")]
    internal ITableColumnRegistry<TItem>? Registry { get; set; }

    // === DISPLAY ===

    /// <summary>
    /// Gets or sets the header text for the column.
    /// If not specified and Property is provided, the property name is used.
    /// </summary>
    [Parameter]
    public string? Header { get; set; }

    /// <summary>
    /// Gets or sets the custom template for rendering cell content.
    /// When provided, this overrides automatic value rendering.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? Template { get; set; }

    // === LAYOUT ===

    /// <summary>
    /// Gets or sets the text alignment for the column.
    /// </summary>
    [Parameter]
    public TableColumnAlign Align { get; set; } = TableColumnAlign.Left;

    /// <summary>
    /// Gets or sets the width specification (e.g., "100px", "20%", "auto").
    /// </summary>
    [Parameter]
    public string? Width { get; set; }

    // === STYLING ===

    /// <summary>
    /// Gets or sets the CSS class applied to the header cell.
    /// </summary>
    [Parameter]
    public string? HeaderClass { get; set; }

    /// <summary>
    /// Gets or sets the CSS class applied to body cells.
    /// </summary>
    [Parameter]
    public string? CellClass { get; set; }

    /// <summary>
    /// Gets or sets whether the column is visible.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    // === INTERACTIVE FEATURES ===

    /// <summary>
    /// Gets or sets whether this column can be sorted.
    /// Only effective when the parent table has Sortable=true.
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; }

    /// <summary>
    /// Gets or sets whether this column should be included in filtering.
    /// Only effective when the parent table has Filterable=true.
    /// </summary>
    [Parameter]
    public bool Filterable { get; set; } = true;

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

/// <summary>
/// Base class for typed table column components with property binding.
/// </summary>
/// <typeparam name="TItem">The type of items in the table.</typeparam>
/// <typeparam name="TValue">The type of the property value.</typeparam>
public abstract class BUITableColumnBase<TItem, TValue> : BUITableColumnBase<TItem>
{
    /// <summary>
    /// Gets or sets the property expression for extracting values from items.
    /// The property name is also used as the column header if Header is not specified.
    /// </summary>
    [Parameter]
    public Expression<Func<TItem, TValue>>? Property { get; set; }

    /// <summary>
    /// Gets or sets the format string for displaying values (e.g., "C2" for currency, "d" for date).
    /// Only applies when the value implements IFormattable.
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets a custom comparer for sorting this column.
    /// Parameters: (item1, item2) => comparison result
    /// If null, default comparison using the property value is used.
    /// </summary>
    [Parameter]
    public Comparison<TItem>? CustomComparer { get; set; }

    /// <summary>
    /// Gets or sets a custom filter function for this column.
    /// Parameters: (item, filterText) => bool
    /// If null, default string Contains comparison is used on the property value.
    /// </summary>
    [Parameter]
    public Func<TItem, string, bool>? CustomFilter { get; set; }

    protected Func<TItem, object?>? CompiledSelector { get; private set; }
    protected string? PropertyName { get; private set; }

    protected override void OnParametersSet()
    {
        if (Property != null)
        {
            // Compile the expression once for performance
            Func<TItem, TValue> compiled = Property.Compile();
            CompiledSelector = item => compiled(item);

            // Extract property name for header
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