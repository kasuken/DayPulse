using Microsoft.Extensions.DependencyInjection;
using DayPulse.Core.Repositories;
using DayPulse.Infrastructure.Repositories;
using DayPulse.Infrastructure.Storage;

namespace DayPulse.Infrastructure;

/// <summary>
/// Registers client-side infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds browser storage and IndexedDB services to the application service collection.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <returns>The same service collection for further configuration.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IClientStorage, BrowserStorage>();
        services.AddScoped<IAtomicClientStorage>(serviceProvider =>
            (IAtomicClientStorage)serviceProvider.GetRequiredService<IClientStorage>());
        services.AddScoped<IIndexedDbStorage, IndexedDbStorage>();
        services.AddScoped<ICheckInRepository, BrowserCheckInRepository>();
        services.AddScoped<IActivityRepository, BrowserActivityRepository>();
        services.AddScoped<ISettingsRepository, BrowserSettingsRepository>();

        return services;
    }
}
