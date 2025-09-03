# ResilientGraphQLClient

A resilient .NET GraphQL client library with built-in retry policies, circuit breakers, and comprehensive error handling capabilities.

## Features

- **Resilient Communication**: Built-in retry policies and circuit breaker patterns using Polly
- **GraphQL Support**: Full GraphQL query, mutation, and subscription support
- **Configurable**: Flexible configuration options for timeout, retry policies, and circuit breaker settings
- **Dependency Injection**: Native support for .NET dependency injection container
- **Logging**: Integrated logging support using Microsoft.Extensions.Logging
- **Type Safety**: Strongly-typed request and response handling

## Installation

```bash
dotnet add package ResilientGraphQLClient
```

## Quick Start

### Basic Usage

```csharp
using ResilientGraphQLClient;

// Create a resilient GraphQL client
var client = new ResilientGraphQLClient("https://api.example.com/graphql");

// Execute a query
var query = new GraphQLRequest
{
    Query = @"
        query GetUser($id: ID!) {
            user(id: $id) {
                id
                name
                email
            }
        }",
    Variables = new { id = "123" }
};

var response = await client.SendQueryAsync<UserResponse>(query);
```

### Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using ResilientGraphQLClient;

// In Startup.cs or Program.cs
services.AddResilientGraphQLClient(options =>
{
    options.Endpoint = "https://api.example.com/graphql";
    options.Timeout = TimeSpan.FromSeconds(30);
    options.RetryCount = 3;
    options.CircuitBreakerFailureThreshold = 5;
});

// Inject and use
public class UserService
{
    private readonly IResilientGraphQLClient _graphQLClient;
    
    public UserService(IResilientGraphQLClient graphQLClient)
    {
        _graphQLClient = graphQLClient;
    }
    
    public async Task<User> GetUserAsync(string id)
    {
        var query = new GraphQLRequest
        {
            Query = "query GetUser($id: ID!) { user(id: $id) { id name email } }",
            Variables = new { id }
        };
        
        var response = await _graphQLClient.SendQueryAsync<UserResponse>(query);
        return response.Data.User;
    }
}
```

### Configuration Options

```csharp
services.AddResilientGraphQLClient(options =>
{
    options.Endpoint = "https://api.example.com/graphql";
    options.Timeout = TimeSpan.FromSeconds(30);
    options.RetryCount = 3;
    options.RetryDelay = TimeSpan.FromSeconds(1);
    options.CircuitBreakerFailureThreshold = 5;
    options.CircuitBreakerSamplingDuration = TimeSpan.FromSeconds(60);
    options.CircuitBreakerMinimumThroughput = 10;
    options.Headers = new Dictionary<string, string>
    {
        ["Authorization"] = "Bearer your-token-here"
    };
});
```

## Advanced Usage

### Custom Resilience Policies

```csharp
var retryPolicy = Policy
    .Handle<GraphQLHttpRequestException>()
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var client = new ResilientGraphQLClient("https://api.example.com/graphql", retryPolicy);
```

### Error Handling

```csharp
try
{
    var response = await client.SendQueryAsync<UserResponse>(query);
    if (response.Errors?.Any() == true)
    {
        // Handle GraphQL errors
        foreach (var error in response.Errors)
        {
            Console.WriteLine($"GraphQL Error: {error.Message}");
        }
    }
}
catch (GraphQLHttpRequestException ex)
{
    // Handle HTTP-level errors
    Console.WriteLine($"HTTP Error: {ex.Message}");
}
catch (TimeoutException ex)
{
    // Handle timeout errors
    Console.WriteLine($"Timeout Error: {ex.Message}");
}
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.