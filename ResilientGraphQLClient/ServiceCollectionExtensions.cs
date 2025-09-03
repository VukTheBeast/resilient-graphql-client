using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ResilientGraphQLClient;

/// <summary>
/// Extension methods for configuring ResilientGraphQLClient services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ResilientGraphQLClient services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configureOptions">An action to configure the ResilientGraphQLClientOptions.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddResilientGraphQLClient(
        this IServiceCollection services,
        Action<ResilientGraphQLClientOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.TryAddScoped<IResilientGraphQLClient, ResilientGraphQLClient>();
        
        return services;
    }

    /// <summary>
    /// Adds ResilientGraphQLClient services to the specified IServiceCollection with a specific endpoint.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="endpoint">The GraphQL endpoint URL.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddResilientGraphQLClient(
        this IServiceCollection services,
        string endpoint)
    {
        return services.AddResilientGraphQLClient(options =>
        {
            options.Endpoint = endpoint;
        });
    }
}