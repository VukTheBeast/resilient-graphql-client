using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;

namespace ResilientGraphQLClient;

/// <summary>
/// A resilient GraphQL client implementation with built-in retry policies and circuit breaker patterns.
/// </summary>
public class ResilientGraphQLClient : IResilientGraphQLClient
{
    private readonly GraphQLHttpClient _graphQLClient;
    private readonly IAsyncPolicy<GraphQLResponse<object>> _resiliencePolicy;
    private readonly ILogger<ResilientGraphQLClient>? _logger;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the ResilientGraphQLClient class.
    /// </summary>
    /// <param name="endpoint">The GraphQL endpoint URL.</param>
    /// <param name="resiliencePolicy">Optional custom resilience policy.</param>
    /// <param name="logger">Optional logger instance.</param>
    public ResilientGraphQLClient(
        string endpoint,
        IAsyncPolicy<GraphQLResponse<object>>? resiliencePolicy = null,
        ILogger<ResilientGraphQLClient>? logger = null)
    {
        _logger = logger;
        
        var httpClient = new HttpClient();
        var options = new GraphQLHttpClientOptions
        {
            EndPoint = new Uri(endpoint)
        };
        _graphQLClient = new GraphQLHttpClient(options, new NewtonsoftJsonSerializer(), httpClient);
        
        _resiliencePolicy = resiliencePolicy ?? CreateDefaultResiliencePolicy();
    }

    /// <summary>
    /// Initializes a new instance of the ResilientGraphQLClient class with options.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">Optional logger instance.</param>
    public ResilientGraphQLClient(
        IOptions<ResilientGraphQLClientOptions> options,
        ILogger<ResilientGraphQLClient>? logger = null)
    {
        _logger = logger;
        
        var config = options.Value;
        var httpClient = new HttpClient
        {
            Timeout = config.Timeout
        };

        // Add custom headers
        foreach (var header in config.Headers)
        {
            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        _graphQLClient = new GraphQLHttpClient(new GraphQLHttpClientOptions
        {
            EndPoint = new Uri(config.Endpoint)
        }, new NewtonsoftJsonSerializer(), httpClient);
        _resiliencePolicy = CreateResiliencePolicyFromOptions(config);
    }

    /// <inheritdoc />
    public async Task<GraphQLResponse<T>> SendQueryAsync<T>(GraphQLRequest request, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ResilientGraphQLClient));
        
        _logger?.LogDebug("Sending GraphQL query: {Query}", request.Query);
        
        try
        {
            var response = await _resiliencePolicy.ExecuteAsync(async (ct) =>
            {
                var result = await _graphQLClient.SendQueryAsync<object>(request, ct);
                return result;
            }, cancellationToken);

            // Convert the response to the desired type
            var typedResponse = new GraphQLResponse<T>
            {
                Data = response.Data != null ? 
                    Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Newtonsoft.Json.JsonConvert.SerializeObject(response.Data)) : 
                    default(T),
                Errors = response.Errors,
                Extensions = response.Extensions
            };

            _logger?.LogDebug("GraphQL query completed successfully");
            return typedResponse;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error executing GraphQL query");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<GraphQLResponse<T>> SendMutationAsync<T>(GraphQLRequest request, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ResilientGraphQLClient));
        
        _logger?.LogDebug("Sending GraphQL mutation: {Query}", request.Query);
        
        try
        {
            var response = await _resiliencePolicy.ExecuteAsync(async (ct) =>
            {
                var result = await _graphQLClient.SendMutationAsync<object>(request, ct);
                return result;
            }, cancellationToken);

            // Convert the response to the desired type
            var typedResponse = new GraphQLResponse<T>
            {
                Data = response.Data != null ? 
                    Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Newtonsoft.Json.JsonConvert.SerializeObject(response.Data)) : 
                    default(T),
                Errors = response.Errors,
                Extensions = response.Extensions
            };

            _logger?.LogDebug("GraphQL mutation completed successfully");
            return typedResponse;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error executing GraphQL mutation");
            throw;
        }
    }

    private IAsyncPolicy<GraphQLResponse<object>> CreateDefaultResiliencePolicy()
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger?.LogWarning("Retry {RetryCount} after {Delay}ms due to: {Exception}",
                        retryCount, timespan.TotalMilliseconds, outcome?.Message);
                });

        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    _logger?.LogWarning("Circuit breaker opened for {Duration}ms due to: {Exception}",
                        duration.TotalMilliseconds, exception.Message);
                },
                onReset: () =>
                {
                    _logger?.LogInformation("Circuit breaker reset");
                });

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy)
            .AsAsyncPolicy<GraphQLResponse<object>>();
    }

    private IAsyncPolicy<GraphQLResponse<object>> CreateResiliencePolicyFromOptions(ResilientGraphQLClientOptions options)
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: options.RetryCount,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(options.RetryDelay.TotalMilliseconds * retryAttempt),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger?.LogWarning("Retry {RetryCount} after {Delay}ms due to: {Exception}",
                        retryCount, timespan.TotalMilliseconds, outcome?.Message);
                });

        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: options.CircuitBreakerFailureThreshold,
                durationOfBreak: options.CircuitBreakerSamplingDuration,
                onBreak: (exception, duration) =>
                {
                    _logger?.LogWarning("Circuit breaker opened for {Duration}ms due to: {Exception}",
                        duration.TotalMilliseconds, exception.Message);
                },
                onReset: () =>
                {
                    _logger?.LogInformation("Circuit breaker reset");
                });

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy)
            .AsAsyncPolicy<GraphQLResponse<object>>();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _graphQLClient?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}