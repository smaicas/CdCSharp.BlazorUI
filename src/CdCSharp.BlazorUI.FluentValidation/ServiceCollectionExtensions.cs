using FluentValidation;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBUIFluentValidation<TEntry>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        Func<AssemblyScanner.AssemblyScanResult, bool>? filter = null,
        bool includeInternalTypes = false)
    {
        services.AddValidatorsFromAssemblyContaining<TEntry>(lifetime, filter, includeInternalTypes);
        return services;
    }

    public static IServiceCollection AddBUIFluentValidation(
        this IServiceCollection services,
        Assembly? assembly = null,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        Func<AssemblyScanner.AssemblyScanResult, bool>? filter = null,
        bool includeInternalTypes = false)
    {
        assembly ??= Assembly.GetExecutingAssembly();
        services.AddValidatorsFromAssembly(assembly, lifetime, filter, includeInternalTypes);
        return services;
    }
}
