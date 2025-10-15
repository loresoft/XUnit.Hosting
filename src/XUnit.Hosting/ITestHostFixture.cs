using Microsoft.Extensions.Hosting;

namespace XUnit.Hosting;

/// <summary>
/// Defines a contract for an XUnit collection fixture that provides access to a configured 
/// <see cref="IHost"/> instance and its dependency injection container for use in integration tests.
/// </summary>
/// <remarks>
/// Implement this interface in your XUnit collection fixtures to provide a consistent way to access 
/// the host and service provider across your test classes. The fixture is created once per test collection 
/// and shared among all test classes in that collection.
/// </remarks>
public interface ITestHostFixture
{
    /// <summary>
    /// Gets the <see cref="IHost"/> instance for the test collection.
    /// </summary>
    /// <value>
    /// An <see cref="IHost"/> instance that has been configured and started for the test collection.
    /// This host provides access to the application's services, configuration, and lifetime management.
    /// </value>
    IHost Host { get; }

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> containing the dependency injection services configured for the test collection.
    /// </summary>
    /// <value>
    /// An <see cref="IServiceProvider"/> instance that can be used to resolve services registered 
    /// in the host's dependency injection container. This is typically equivalent to <c>Host.Services</c>.
    /// </value>
    IServiceProvider Services { get; }
}
