
using Microsoft.Extensions.Hosting;

namespace XUnit.Hosting;

/// <summary>
/// XUnit collection fixture that supports <see cref="HostApplicationBuilder"/>
/// </summary>
public interface ITestHostFixture
{
    /// <summary>
    /// Gets the host for this test.
    /// </summary>
    /// <value>
    /// The host for this test.
    /// </value>
    IHost Host { get; }

    /// <summary>
    /// Gets the services configured for this test
    /// </summary>
    /// <value>
    /// The services configured for this test.
    /// </value>
    IServiceProvider Services { get; }
}
