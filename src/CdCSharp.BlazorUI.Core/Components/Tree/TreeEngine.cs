using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Core.Components.Tree;

public sealed class TreeNodeEventArgs<TItem> : EventArgs
{
    public required ITreeNode<TItem> Node { get; init; }
    public required string Key { get; init; }
    public TItem? Item { get; init; }
    public int Depth { get; init; }
    public bool IsExpanded { get; init; }
}

public sealed class TreeNodeRegistration
{
    public required string Key { get; init; }
    public string? Text { get; init; }
    public string? Icon { get; init; }
    public bool Disabled { get; init; }
    public bool InitiallyExpanded { get; init; }
    public object? Data { get; init; }
    public RenderFragment? NodeContent { get; init; }
    public string? ParentKey { get; init; }
}

/// <summary>
/// Core tree structure and state management engine.
/// No Blazor dependencies - pure business logic.
/// </summary>
public sealed class TreeEngine<TItem> : ITreeRegistry
{
    private readonly Dictionary<string, TreeNodeState<TItem>> _nodeMap = [];
    private readonly HashSet<string> _expandedKeys = [];
    private readonly HashSet<string> _loadingKeys = [];
    private List<TreeNodeState<TItem>> _rootNodes = [];

    public IReadOnlyList<TreeNodeState<TItem>> RootNodes => _rootNodes;
    public IReadOnlySet<string> ExpandedKeys => _expandedKeys;
    public IReadOnlyDictionary<string, TreeNodeState<TItem>> NodeMap => _nodeMap;

    public TreeEngineConfiguration<TItem> Configuration { get; }

    // Events for Blazor components to subscribe to
    public event Func<TreeNodeEventArgs<TItem>, Task>? NodeExpanded;
    public event Func<TreeNodeEventArgs<TItem>, Task>? NodeCollapsed;
    public event Func<TreeNodeEventArgs<TItem>, Task>? NodeClicked;
    public event Action? StateChanged;

    public TreeEngine(TreeEngineConfiguration<TItem> configuration)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        if (configuration.InitialExpandedKeys != null)
        {
            foreach (string key in configuration.InitialExpandedKeys)
            {
                _expandedKeys.Add(key);
            }
        }
    }

    // ===== DATA-BOUND MODE =====

    public void BuildFromItems(IEnumerable<TItem> items)
    {
        if (Configuration.KeySelector == null)
            throw new InvalidOperationException("KeySelector is required for data-bound mode");

        _nodeMap.Clear();
        _rootNodes = [];

        foreach (TItem? item in items)
        {
            TreeNodeState<TItem> node = BuildNodeFromItem(item, null, 0);
            _rootNodes.Add(node);
        }

        StateChanged?.Invoke();
    }

    private TreeNodeState<TItem> BuildNodeFromItem(TItem item, TreeNodeState<TItem>? parent, int depth)
    {
        string key = Configuration.KeySelector!.Invoke(item);
        IEnumerable<TItem>? children = Configuration.ChildrenSelector?.Invoke(item);
        bool hasChildren = Configuration.HasChildrenSelector?.Invoke(item)
            ?? children?.Any()
            ?? false;

        TreeNodeState<TItem> node = new()
        {
            Key = key,
            Item = item,
            HasChildren = hasChildren,
            Depth = depth,
            Parent = parent
        };

        _nodeMap[key] = node;

        if (Configuration.ExpandAll && hasChildren)
        {
            _expandedKeys.Add(key);
        }

        if (children != null)
        {
            foreach (TItem? child in children)
            {
                TreeNodeState<TItem> childNode = BuildNodeFromItem(child, node, depth + 1);
                node.ChildrenInternal.Add(childNode);
            }
        }

        return node;
    }

    // ===== DECLARATIVE MODE =====

    private readonly List<TreeNodeRegistration> _pendingRegistrations = [];

    public void RegisterNode(TreeNodeRegistration registration)
    {
        _pendingRegistrations.Add(registration);
    }

    public void BuildFromRegistrations()
    {
        _nodeMap.Clear();
        _rootNodes = [];

        Dictionary<string, List<TreeNodeRegistration>> childrenByParent = _pendingRegistrations
            .GroupBy(r => r.ParentKey ?? string.Empty)
            .ToDictionary(g => g.Key, g => g.ToList());

        if (childrenByParent.TryGetValue(string.Empty, out List<TreeNodeRegistration> roots))
        {
            foreach (TreeNodeRegistration reg in roots)
            {
                TreeNodeState<TItem> node = BuildNodeFromRegistration(reg, null, 0, childrenByParent);
                _rootNodes.Add(node);
            }
        }

        _pendingRegistrations.Clear();
        StateChanged?.Invoke();
    }

    private TreeNodeState<TItem> BuildNodeFromRegistration(
        TreeNodeRegistration reg,
        TreeNodeState<TItem>? parent,
        int depth,
        Dictionary<string, List<TreeNodeRegistration>> childrenByParent)
    {
        bool hasChildren = childrenByParent.ContainsKey(reg.Key);

        TreeNodeState<TItem> node = new()
        {
            Key = reg.Key,
            Text = reg.Text,
            Icon = reg.Icon,
            Disabled = reg.Disabled,
            HasChildren = hasChildren,
            Depth = depth,
            Parent = parent,
            CustomContent = reg.NodeContent,
            AdditionalData = reg.Data
        };

        _nodeMap[reg.Key] = node;

        if (reg.InitiallyExpanded || Configuration.ExpandAll)
        {
            _expandedKeys.Add(reg.Key);
        }

        if (childrenByParent.TryGetValue(reg.Key, out List<TreeNodeRegistration> children))
        {
            foreach (TreeNodeRegistration childReg in children)
            {
                TreeNodeState<TItem> childNode = BuildNodeFromRegistration(childReg, node, depth + 1, childrenByParent);
                node.ChildrenInternal.Add(childNode);
            }
        }

        return node;
    }

    // ===== STATE OPERATIONS =====

    public bool IsExpanded(string key) => _expandedKeys.Contains(key);
    public bool IsLoading(string key) => _loadingKeys.Contains(key);

    public async Task ExpandAsync(string key)
    {
        if (!_nodeMap.TryGetValue(key, out TreeNodeState<TItem>? node) || IsExpanded(key))
            return;

        if (!node.HasChildren)
            return;

        // Lazy load children if needed
        if (Configuration.LoadChildrenAsync != null
            && node.Item != null
            && node.ChildrenInternal.Count == 0)
        {
            await LoadChildrenAsync(node);
        }

        _expandedKeys.Add(key);
        StateChanged?.Invoke();

        if (NodeExpanded != null)
            await NodeExpanded.Invoke(CreateEventArgs(node, true));
    }

    public async Task CollapseAsync(string key)
    {
        if (!_nodeMap.TryGetValue(key, out TreeNodeState<TItem>? node) || !IsExpanded(key))
            return;

        _expandedKeys.Remove(key);
        StateChanged?.Invoke();

        if (NodeCollapsed != null)
            await NodeCollapsed.Invoke(CreateEventArgs(node, false));
    }

    public async Task ToggleAsync(string key)
    {
        if (IsExpanded(key))
            await CollapseAsync(key);
        else
            await ExpandAsync(key);
    }

    public void ExpandAll()
    {
        foreach (TreeNodeState<TItem>? node in _nodeMap.Values.Where(n => n.HasChildren))
        {
            _expandedKeys.Add(node.Key);
        }
        StateChanged?.Invoke();
    }

    public void CollapseAll()
    {
        _expandedKeys.Clear();
        StateChanged?.Invoke();
    }

    private async Task LoadChildrenAsync(TreeNodeState<TItem> node)
    {
        if (Configuration.LoadChildrenAsync == null || node.Item == null)
            return;

        _loadingKeys.Add(node.Key);
        StateChanged?.Invoke();

        try
        {
            IEnumerable<TItem> children;

            // Check cache first
            if (Configuration.Cache?.TryGet(node.Key, out IEnumerable<TItem>? cached) == true && cached != null)
            {
                children = cached;
            }
            else
            {
                children = await Configuration.LoadChildrenAsync(node.Item);
                Configuration.Cache?.Set(node.Key, children);
            }

            node.ChildrenInternal.Clear();
            foreach (TItem? child in children)
            {
                TreeNodeState<TItem> childNode = BuildNodeFromItem(child, node, node.Depth + 1);
                node.ChildrenInternal.Add(childNode);
            }

            node.HasChildren = node.ChildrenInternal.Count > 0;
        }
        finally
        {
            _loadingKeys.Remove(node.Key);
            StateChanged?.Invoke();
        }
    }

    public TreeNodeState<TItem>? GetNode(string key)
    {
        return _nodeMap.TryGetValue(key, out TreeNodeState<TItem>? node) ? node : null;
    }

    public void InvalidateCache(string? key = null)
    {
        if (key != null)
        {
            Configuration.Cache?.Invalidate(key);
        }
        else
        {
            Configuration.Cache?.InvalidateAll();
        }
    }

    private TreeNodeEventArgs<TItem> CreateEventArgs(TreeNodeState<TItem> node, bool isExpanded) => new()
    {
        Node = node,
        Key = node.Key,
        Item = node.Item,
        Depth = node.Depth,
        IsExpanded = isExpanded
    };
}
