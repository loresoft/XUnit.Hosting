using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using XUnit.Hosting.Logging;

namespace XUnit.Hosting;

/// <summary>
/// XUnit collection fixture that supports <see cref="IHostBuilder"/>
/// </summary>
public abstract class TestHostFixture
{
    private readonly IHostBuilder _builder;
    private readonly Lazy<IHost> _host;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestHostFixture"/> class.
    /// </summary>
    protected TestHostFixture()
    {
        _builder = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(ConfigureApplication)
            .ConfigureLogging(ConfigureLogging)
            .ConfigureServices(ConfigureServices);

        // trigger start only when host is used
        _host = new Lazy<IHost>(_builder.Start);
    }

    /// <summary>
    /// Gets the host builder this test.
    /// </summary>
    /// <value>
    /// The host builder for this test.
    /// </value>
    protected IHostBuilder HostBuilder => _builder;

    /// <summary>
    /// Gets the host for this test.
    /// </summary>
    /// <value>
    /// The host for this test.
    /// </value>
    protected IHost Host => _host.Value;

    /// <summary>
    /// Gets the services configured for this test
    /// </summary>
    /// <value>
    /// The services configured for this test.
    /// </value>
    public IServiceProvider Services => Host.Services;


    /// <summary>
    /// Sets up the configuration for the remainder of the build process and application
    /// </summary>
    /// <param name="context">Context containing the common services on the <see cref="IHost" /></param>
    /// <param name="builder">Represents a type used to build application configuration</param>
    protected virtual void ConfigureApplication(HostBuilderContext context, IConfigurationBuilder builder)
    {

    }

    /// <summary>
    /// Adds a delegate for configuring the provided <see cref="ILoggingBuilder"/>
    /// </summary>
    /// <param name="context">Context containing the common services on the <see cref="IHost" /></param>
    /// <param name="builder">An interface for configuring logging providers</param>
    protected virtual void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
    {
        builder.SetMinimumLevel(LogLevel.Debug);
        builder.AddMemoryLogger();
    }

    /// <summary>
    /// Adds services to the container
    /// </summary>
    /// <param name="context">Context containing the common services on the <see cref="IHost" /></param>
    /// <param name="collection">Specifies the contract for a collection of service descriptors</param>
    protected virtual void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {

    }
}
