using CdCSharp.BlazorUI.Core.Components.Abstractions;

namespace CdCSharp.BlazorUI.Core.Components.Configuration;

public class BlazorUIOptions : IBlazorUIOptions
{
    private readonly Dictionary<Type, object> _builders = [];

    public IComponentVariantBuilder<TComponent, TVariant> Configure<TComponent, TVariant>()
        where TComponent : UIVariantComponentBase<TComponent, TVariant>
        where TVariant : Variant
    {
        Type key = typeof(ComponentVariantBuilder<TComponent, TVariant>);

        if (!_builders.TryGetValue(key, out object? builder))
        {
            builder = new ComponentVariantBuilder<TComponent, TVariant>();
            _builders[key] = builder;
        }

        return (IComponentVariantBuilder<TComponent, TVariant>)builder;
    }

    public void ApplyTo(IServiceProvider services)
    {
        foreach (object builder in _builders.Values)
        {
            if (builder is IVariantBuilderApplier applier)
            {
                applier.ApplyTo(services);
            }
        }
    }
}

internal interface IVariantBuilderApplier
{
    void ApplyTo(IServiceProvider services);
}
