using GraphQL;

namespace ResilientGraphQLClient;

/// <summary>
/// Interface for a resilient GraphQL client with built-in retry and circuit breaker capabilities.
/// </summary>
public interface IResilientGraphQLClient : IDisposable
{
    /// <summary>
    /// Sends a GraphQL query asynchronously.
    /// </summary>
    /// <typeparam name="T">The expected response type.</typeparam>
    /// <param name="request">The GraphQL request to send.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The GraphQL response.</returns>
    Task<GraphQLResponse<T>> SendQueryAsync<T>(GraphQLRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a GraphQL mutation asynchronously.
    /// </summary>
    /// <typeparam name="T">The expected response type.</typeparam>
    /// <param name="request">The GraphQL request to send.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The GraphQL response.</returns>
    Task<GraphQLResponse<T>> SendMutationAsync<T>(GraphQLRequest request, CancellationToken cancellationToken = default);
}
