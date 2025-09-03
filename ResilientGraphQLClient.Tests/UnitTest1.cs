using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ResilientGraphQLClient;

namespace ResilientGraphQLClient.Tests;

public class ResilientGraphQLClientTests
{
    [Fact]
    public void Constructor_WithEndpoint_ShouldCreateInstance()
    {
        // Arrange
        var endpoint = "https://api.example.com/graphql";

        // Act & Assert (should not throw)
        using var client = new ResilientGraphQLClient(endpoint);
        Assert.NotNull(client);
    }

    [Fact]
    public void ServiceCollectionExtensions_AddResilientGraphQLClient_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var endpoint = "https://api.example.com/graphql";

        // Act
        services.AddResilientGraphQLClient(endpoint);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IResilientGraphQLClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void ServiceCollectionExtensions_AddResilientGraphQLClientWithOptions_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddResilientGraphQLClient(options =>
        {
            options.Endpoint = "https://api.example.com/graphql";
            options.RetryCount = 5;
            options.Timeout = TimeSpan.FromSeconds(60);
        });
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IResilientGraphQLClient>();
        Assert.NotNull(client);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var client = new ResilientGraphQLClient("https://api.example.com/graphql");

        // Act & Assert (should not throw)
        client.Dispose();
    }

    [Fact]
    public void ResilientGraphQLClientOptions_DefaultValues_ShouldBeSet()
    {
        // Arrange & Act
        var options = new ResilientGraphQLClientOptions();

        // Assert
        Assert.Equal(string.Empty, options.Endpoint);
        Assert.Equal(TimeSpan.FromSeconds(30), options.Timeout);
        Assert.Equal(3, options.RetryCount);
        Assert.Equal(TimeSpan.FromSeconds(1), options.RetryDelay);
        Assert.Equal(5, options.CircuitBreakerFailureThreshold);
        Assert.Equal(TimeSpan.FromSeconds(60), options.CircuitBreakerSamplingDuration);
        Assert.Equal(10, options.CircuitBreakerMinimumThroughput);
        Assert.NotNull(options.Headers);
        Assert.Empty(options.Headers);
    }
}