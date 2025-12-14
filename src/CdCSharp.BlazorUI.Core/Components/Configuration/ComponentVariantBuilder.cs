using CdCSharp.BlazorUI.Core.Components.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Core.Components.Configuration;

internal class ComponentVariantBuilder<TComponent, TVariant>
    : IComponentVariantBuilder<TComponent, TVariant>, IVariantBuilderApplier
    where TComponent : UIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    private readonly List<(TVariant variant, Func<TComponent, RenderFragment> template)> _registrations = [];

    public IComponentVariantBuilder<TComponent, TVariant> AddVariant(
        TVariant variant,
        Func<TComponent, RenderFragment> template)
    {
        _registrations.Add((variant, template));
        return this;
    }

    public IComponentVariantBuilder<TComponent, TVariant> AddVariant(
        string variantName,
        Func<TComponent, RenderFragment> template)
    {
        // Use reflection to create variant instance
        TVariant variant = CreateVariant(variantName);
        return AddVariant(variant, template);
    }

    void IVariantBuilderApplier.ApplyTo(IServiceProvider services)
    {
        IVariantRegistry<TComponent, TVariant>? registry =
            services.GetService<IVariantRegistry<TComponent, TVariant>>();

        if (registry != null)
        {
            foreach ((TVariant variant, Func<TComponent, RenderFragment> template) in _registrations)
            {
                registry.Register(variant, template);
            }
        }
    }

    private static TVariant CreateVariant(string name)
    {
        // Look for Custom method first
        Type variantType = typeof(TVariant);
        System.Reflection.MethodInfo? customMethod = variantType
            .GetMethod("Custom", new[] { typeof(string) });

        if (customMethod != null && customMethod.IsStatic)
        {
            return (TVariant)customMethod.Invoke(null, new object[] { name })!;
        }

        // Fallback to constructor
        return (TVariant)Activator.CreateInstance(typeof(TVariant), name)!;
    }
}
