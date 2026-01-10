namespace CdCSharp.BlazorUI.Components;

public interface ITreeNodeRegistry<TRegistration>
    where TRegistration : TreeNodeRegistration
{
    void Register(TRegistration registration);
}

internal sealed class TreeNodeRegistry<TRegistration> : ITreeNodeRegistry<TRegistration>
    where TRegistration : TreeNodeRegistration
{
    private readonly List<TRegistration> _registrations = [];

    public void Register(TRegistration registration)
        => _registrations.Add(registration);

    public IReadOnlyList<TRegistration> GetRegistrations()
        => _registrations.ToList();  // Return a copy to prevent external modification or clear external by reference

    public void Clear() => _registrations.Clear();
}