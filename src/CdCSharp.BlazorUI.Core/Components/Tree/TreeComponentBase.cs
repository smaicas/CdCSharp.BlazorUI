using CdCSharp.BlazorUI.Core.Abstractions.Components;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Core.Components.Tree;

public abstract class TreeComponentBase<TItem> : BUIComponentBase
{
    private TreeEngine<TItem>? _engine;

    // ===== DATA-BOUND MODE =====
    [Parameter] public IEnumerable<TItem>? Items { get; set; }
    [Parameter] public Func<TItem, IEnumerable<TItem>?>? ChildrenSelector { get; set; }
    [Parameter] public Func<TItem, bool>? HasChildrenSelector { get; set; }
    [Parameter] public Func<TItem, string>? KeySelector { get; set; }
    [Parameter] public Func<TItem, Task<IEnumerable<TItem>>>? OnLoadChildren { get; set; }
    [Parameter] public TreeNodeCache<TItem>? Cache { get; set; }

    // ===== DECLARATIVE MODE =====
    [Parameter] public RenderFragment? ChildContent { get; set; }

    // ===== COMMON =====
    [Parameter] public HashSet<string>? ExpandedKeys { get; set; }
    [Parameter] public EventCallback<HashSet<string>> ExpandedKeysChanged { get; set; }
    [Parameter] public bool ExpandAll { get; set; }

    // ===== EVENTS =====
    [Parameter] public EventCallback<TreeNodeEventArgs<TItem>> OnNodeClick { get; set; }
    [Parameter] public EventCallback<TreeNodeEventArgs<TItem>> OnNodeExpand { get; set; }
    [Parameter] public EventCallback<TreeNodeEventArgs<TItem>> OnNodeCollapse { get; set; }

    protected TreeEngine<TItem> Engine => _engine
        ?? throw new InvalidOperationException("Tree engine not initialized");

    protected TreeMode Mode { get; private set; } = TreeMode.Uninitialized;
    protected bool IsDeclarativeMode => Mode == TreeMode.Declarative;
    protected bool DeclarativeNodesBuilt { get; private set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        ValidateAndDetermineMode();
        InitializeEngine();
    }

    private void ValidateAndDetermineMode()
    {
        bool hasDeclarative = ChildContent != null;
        bool hasDataBound = Items != null;

        if (hasDeclarative && hasDataBound)
        {
            throw new InvalidOperationException(
                $"{GetType().Name} cannot use both declarative (ChildContent) and data-bound (Items) modes simultaneously. " +
                "Use either child components OR the Items parameter, not both.");
        }

        Mode = hasDeclarative ? TreeMode.Declarative : TreeMode.DataBound;
    }

    private void InitializeEngine()
    {
        TreeEngineConfiguration<TItem> config = new()
        {
            KeySelector = KeySelector,
            ChildrenSelector = ChildrenSelector,
            HasChildrenSelector = HasChildrenSelector,
            LoadChildrenAsync = OnLoadChildren,
            Cache = Cache,
            InitialExpandedKeys = ExpandedKeys,
            ExpandAll = ExpandAll
        };

        _engine = new TreeEngine<TItem>(config);

        // Wire up events
        _engine.NodeExpanded += async args =>
        {
            await OnNodeExpand.InvokeAsync(args);
            await NotifyExpandedKeysChangedAsync();
        };

        _engine.NodeCollapsed += async args =>
        {
            await OnNodeCollapse.InvokeAsync(args);
            await NotifyExpandedKeysChangedAsync();
        };

        _engine.NodeClicked += async args => await OnNodeClick.InvokeAsync(args);

        _engine.StateChanged += () => StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (Mode == TreeMode.DataBound && Items != null)
        {
            Engine.BuildFromItems(Items);
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (firstRender && Mode == TreeMode.Declarative && !DeclarativeNodesBuilt)
        {
            Engine.BuildFromRegistrations();
            DeclarativeNodesBuilt = true;
            StateHasChanged();
        }
    }

    private async Task NotifyExpandedKeysChangedAsync()
    {
        if (ExpandedKeysChanged.HasDelegate)
        {
            await ExpandedKeysChanged.InvokeAsync([.. Engine.ExpandedKeys]);
        }
    }

    // ===== ABSTRACT METHODS FOR DERIVED COMPONENTS =====

    /// <summary>
    /// Override to provide custom rendering for each node.
    /// </summary>
    protected abstract RenderFragment RenderNode(TreeNodeState<TItem> node);

    /// <summary>
    /// Override to handle node interaction (click, selection, navigation, etc.)
    /// </summary>
    protected abstract Task HandleNodeInteractionAsync(TreeNodeState<TItem> node);

    // ===== PUBLIC API =====

    public void ExpandAllNodes() => Engine.ExpandAll();
    public void CollapseAllNodes() => Engine.CollapseAll();
    public TreeNodeState<TItem>? GetNode(string key) => Engine.GetNode(key);
    public void InvalidateCache(string? key = null) => Engine.InvalidateCache(key);
}

public enum TreeMode
{
    Uninitialized,
    DataBound,
    Declarative
}