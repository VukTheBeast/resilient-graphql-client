namespace ResilientGraphQLClient;

/// <summary>
/// Configuration options for the resilient GraphQL client.
/// </summary>
public class ResilientGraphQLClientOptions
{
    /// <summary>
    /// The GraphQL endpoint URL.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// HTTP request timeout.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Number of retry attempts.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Delay between retry attempts.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Circuit breaker failure threshold.
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>
    /// Circuit breaker sampling duration.
    /// </summary>
    public TimeSpan CircuitBreakerSamplingDuration { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Circuit breaker minimum throughput.
    /// </summary>
    public int CircuitBreakerMinimumThroughput { get; set; } = 10;

    /// <summary>
    /// Additional HTTP headers to include with requests.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();
}