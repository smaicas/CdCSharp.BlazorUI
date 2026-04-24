using FluentValidation;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBUIFluentValidation<TEntry>(
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        Func<AssemblyScanner.AssemblyScanResult, bool>? filter = null,
        bool includeInternalTypes = false)
        {
            services.AddValidatorsFromAssemblyContaining<TEntry>(lifetime, filter, includeInternalTypes);
            return services;
        }

        public IServiceCollection AddBUIFluentValidation(
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
}
