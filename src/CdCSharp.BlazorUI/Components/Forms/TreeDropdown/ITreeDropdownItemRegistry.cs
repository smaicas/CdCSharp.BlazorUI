namespace CdCSharp.BlazorUI.Components.Forms;

public interface ITreeDropdownItemRegistry
{
    void Register(TreeDropdownItemRegistration registration);
    Type ValueType { get; }
}

internal sealed class TreeDropdownItemRegistry : ITreeDropdownItemRegistry
{
    private readonly List<TreeDropdownItemRegistration> _registrations = [];

    public Type ValueType { get; }

    public TreeDropdownItemRegistry(Type valueType)
    {
        ValueType = valueType;
    }

    public void Register(TreeDropdownItemRegistration registration)
        => _registrations.Add(registration);

    public IReadOnlyList<TreeDropdownItemRegistration> GetRegistrations()
        => _registrations.ToList();

    public void Clear() => _registrations.Clear();
}