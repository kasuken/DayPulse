using Microsoft.Extensions.DependencyInjection;

namespace DayPulse.App.Shared;

/// <summary>
/// Registers DayPulse localization and culture services.
/// </summary>
public static class LocalizationServiceCollectionExtensions
{
    /// <summary>
    /// Adds the shared resource localizer, culture state, and culture-aware formatter.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <returns>The same service collection for further configuration.</returns>
    public static IServiceCollection AddDayPulseLocalization(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddLocalization();
        services.AddSingleton<CultureSelector>();
        services.AddScoped<CultureState>();
        services.AddScoped<LocalizedFormatService>();
        return services;
    }
}
