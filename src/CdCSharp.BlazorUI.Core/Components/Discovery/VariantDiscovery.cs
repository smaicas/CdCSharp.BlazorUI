using CdCSharp.BlazorUI.Core.Components.Abstractions;
using CdCSharp.BlazorUI.Core.Components.Attributes;
using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace CdCSharp.BlazorUI.Core.Components.Discovery;

public static class VariantDiscovery
{
    public static void DiscoverAndRegisterVariants(IServiceProvider services, Assembly assembly)
    {
        // Discover attribute-based variants
        DiscoverAttributeBasedVariants(services, assembly);

        // Discover IVariantProvider implementations
        DiscoverProviderBasedVariants(services, assembly);
    }

    private static void DiscoverAttributeBasedVariants(IServiceProvider services, Assembly assembly)
    {
        IEnumerable<MethodInfo> methods = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => m.GetCustomAttributes<VariantAttribute>().Any());

        foreach (MethodInfo method in methods)
        {
            foreach (VariantAttribute attr in method.GetCustomAttributes<VariantAttribute>())
            {
                Type? attrType = attr.GetType();
                if (!attrType.IsGenericType) continue;

                Type genericDef = attrType.GetGenericTypeDefinition();
                if (genericDef != typeof(VariantAttribute<,>))
                {
                    // Check if it inherits from VariantAttribute<,>
                    Type? baseType = attrType.BaseType;
                    while (baseType != null && baseType != typeof(object))
                    {
                        if (baseType.IsGenericType &&
                            baseType.GetGenericTypeDefinition() == typeof(VariantAttribute<,>))
                        {
                            RegisterMethodVariant(services, method, attr, baseType);
                            break;
                        }
                        baseType = baseType.BaseType;
                    }
                }
                else
                {
                    RegisterMethodVariant(services, method, attr, attrType);
                }
            }
        }
    }

    private static void RegisterMethodVariant(
        IServiceProvider services,
        MethodInfo method,
        VariantAttribute attr,
        Type variantAttributeType)
    {
        Type[] genericArgs = variantAttributeType.GetGenericArguments();
        Type componentType = genericArgs[0];
        Type variantType = genericArgs[1];

        // Create variant instance
        MethodInfo? customMethod = variantType.GetMethod("Custom", new[] { typeof(string) });
        object? variant = customMethod?.IsStatic == true
            ? customMethod.Invoke(null, new object[] { attr.VariantName })
            : Activator.CreateInstance(variantType, attr.VariantName);

        if (variant == null) return;

        // Create delegate
        Type delegateType = typeof(Func<,>).MakeGenericType(componentType, typeof(RenderFragment));
        Delegate? templateDelegate = Delegate.CreateDelegate(delegateType, method, false);

        if (templateDelegate == null) return;

        // Get registry and register
        Type registryType = typeof(IVariantRegistry<,>).MakeGenericType(componentType, variantType);
        object? registry = services.GetService(registryType);

        if (registry != null)
        {
            MethodInfo? registerMethod = registryType.GetMethod("Register");
            registerMethod?.Invoke(registry, new[] { variant, templateDelegate });
        }
    }

    private static void DiscoverProviderBasedVariants(IServiceProvider services, Assembly assembly)
    {
        IEnumerable<Type> providerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IVariantProvider<,>)));

        foreach (Type providerType in providerTypes)
        {
            Type? providerInterface = providerType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType &&
                                    i.GetGenericTypeDefinition() == typeof(IVariantProvider<,>));

            if (providerInterface == null) continue;

            Type[] genericArgs = providerInterface.GetGenericArguments();
            Type componentType = genericArgs[0];
            Type variantType = genericArgs[1];

            // Create provider instance
            object? provider = Activator.CreateInstance(providerType);
            if (provider == null) continue;

            // Get variants
            MethodInfo? getVariantsMethod = providerInterface.GetMethod("GetVariants");
            object? variants = getVariantsMethod?.Invoke(provider, null);

            if (variants == null) continue;

            // Register each variant
            Type registryType = typeof(IVariantRegistry<,>).MakeGenericType(componentType, variantType);
            object? registry = services.GetService(registryType);

            if (registry != null)
            {
                MethodInfo? registerMethod = registryType.GetMethod("Register");

                foreach (object item in (System.Collections.IEnumerable)variants)
                {
                    PropertyInfo? variantProp = item.GetType().GetProperty("Item1");
                    PropertyInfo? templateProp = item.GetType().GetProperty("Item2");

                    if (variantProp != null && templateProp != null)
                    {
                        object? variant = variantProp.GetValue(item);
                        object? template = templateProp.GetValue(item);

                        if (variant != null && template != null)
                        {
                            registerMethod?.Invoke(registry, new[] { variant, template });
                        }
                    }
                }
            }
        }
    }
}
