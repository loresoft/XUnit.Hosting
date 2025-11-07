using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace XUnit.Hosting;

/// <summary>
/// Provides a base implementation of an XUnit collection fixture that creates and manages
/// a <see cref="HostApplicationBuilder"/> instance for integration testing.
/// </summary>
/// <remarks>
/// <para>
/// This abstract class implements <see cref="ITestHostFixture"/> and provides lazy initialization
/// of the host, ensuring it's only created when first accessed. Derived classes should override
/// <see cref="ConfigureApplication"/> to customize the host configuration for their specific test scenarios.
/// </para>
/// <para>
/// The fixture automatically starts the host and properly disposes of it when the test collection completes.
/// By default, the minimum log level is set to <see cref="LogLevel.Debug"/> to aid in test diagnostics.
/// </para>
/// </remarks>
public abstract class TestApplicationFixture : ITestHostFixture, IDisposable
{
#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif

    private bool _disposed;
    private IHost? _host;

    /// <summary>
    /// Gets the <see cref="IHost"/> instance for the test collection.
    /// </summary>
    /// <value>
    /// An <see cref="IHost"/> instance that has been configured and started for the test collection.
    /// This host provides access to the application's services, configuration, and lifetime management.
    /// </value>
    /// <remarks>
    /// The host is lazily initialized on first access. Once created, the same instance is reused
    /// for all tests in the collection.
    /// </remarks>
    public IHost Host
    {
        get
        {
            if (_host != null)
                return _host;

            // using Lazy<T> causes issues. Use double-check locking instead.
            lock (_lock)
                _host ??= CreateHost();

            return _host;
        }
    }

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> containing the dependency injection services configured for the test collection.
    /// </summary>
    /// <value>
    /// An <see cref="IServiceProvider"/> instance that can be used to resolve services registered
    /// in the host's dependency injection container. This is typically equivalent to <c>Host.Services</c>.
    /// </value>
    public IServiceProvider Services => Host.Services;

    /// <summary>
    /// Creates and initializes the test host.
    /// </summary>
    /// <returns>
    /// An initialized and started <see cref="IHost"/> instance configured for testing.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method follows these steps:
    /// <list type="number">
    /// <item><description>Creates builder settings via <see cref="CreateBuilderSettings"/>.</description></item>
    /// <item><description>Creates a <see cref="HostApplicationBuilder"/> with those settings.</description></item>
    /// <item><description>Configures the application via <see cref="ConfigureApplication"/>.</description></item>
    /// <item><description>Builds and starts the host.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Override this method to completely customize the host creation process. For most scenarios,
    /// overriding <see cref="ConfigureApplication"/> is sufficient.
    /// </para>
    /// </remarks>
    protected virtual IHost CreateHost()
    {
        var settings = CreateBuilderSettings();

        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(settings);

        ConfigureApplication(builder);

        var app = builder.Build();

        app.Start();

        return app;
    }

    /// <summary>
    /// Creates the settings used to construct the <see cref="HostApplicationBuilder"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="HostApplicationBuilderSettings"/> instance with custom configuration,
    /// or <see langword="null"/> to use default settings.
    /// </returns>
    /// <remarks>
    /// Override this method to customize the builder settings, such as specifying a different
    /// environment name, application name, or content root path for the test host.
    /// </remarks>
    protected virtual HostApplicationBuilderSettings? CreateBuilderSettings()
    {
        return null;
    }

    /// <summary>
    /// Configures the application services, logging, and other host settings for testing.
    /// </summary>
    /// <param name="builder">The <see cref="HostApplicationBuilder"/> to configure.</param>
    /// <remarks>
    /// <para>
    /// Override this method in derived classes to register services, configure logging,
    /// add configuration sources, or customize other aspects of the host for your tests.
    /// </para>
    /// <para>
    /// The default implementation sets the minimum log level to <see cref="LogLevel.Debug"/>
    /// to ensure detailed diagnostic information is available during test execution.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// protected override void ConfigureApplication(HostApplicationBuilder builder)
    /// {
    ///     base.ConfigureApplication(builder);
    ///
    ///     // Register test services
    ///     builder.Services.AddSingleton&lt;IMyService, MyTestService&gt;();
    ///
    ///     // Add test configuration
    ///     builder.Configuration.AddInMemoryCollection(new Dictionary&lt;string, string&gt;
    ///     {
    ///         ["TestSetting"] = "TestValue"
    ///     });
    /// }
    /// </code>
    /// </example>
    protected virtual void ConfigureApplication(HostApplicationBuilder builder)
    {
        // Set the minimum log level to Debug for detailed test diagnostics
        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        // Configure console logging with scopes
        builder.Services.Configure<ConsoleFormatterOptions>(options => options.IncludeScopes = true);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="TestApplicationFixture"/>.
    /// </summary>
    /// <remarks>
    /// Disposes the host if it has been created, ensuring all hosted services are stopped
    /// and resources are properly released.
    /// </remarks>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="TestApplicationFixture"/>
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    /// <remarks>
    /// When <paramref name="disposing"/> is <see langword="true"/> and the host has been initialized,
    /// this method disposes the host, stopping all hosted services and releasing associated resources.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;

        if (disposing && _host != null)
            _host.Dispose();
    }
}
