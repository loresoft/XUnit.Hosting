using Xunit;

namespace XUnit.Hosting;

/// <summary>
/// Provides a base class for XUnit test classes that require access to a hosted test environment 
/// with dependency injection and service provider functionality.
/// </summary>
/// <typeparam name="TFixture">
/// The type of the collection fixture that implements <see cref="ITestHostFixture"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// This abstract class simplifies writing integration tests by providing convenient access to 
/// the test fixture, service provider, and XUnit test context. Derive from this class when your 
/// tests need access to a configured host and its dependency injection container.
/// </para>
/// <para>
/// The class implements <see cref="IDisposable"/> to ensure proper cleanup of test resources. 
/// Override <see cref="Dispose(bool)"/> in derived classes if additional cleanup is needed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [Collection(nameof(MyTestCollection))]
/// public class MyTests : TestHostBase&lt;MyTestFixture&gt;
/// {
///     public MyTests(MyTestFixture fixture) : base(fixture)
///     {
///     }
///     
///     [Fact]
///     public void TestMyService()
///     {
///         var service = Services.GetRequiredService&lt;IMyService&gt;();
///         Assert.NotNull(service);
///     }
/// }
/// </code>
/// </example>
public abstract class TestHostBase<TFixture>(TFixture fixture) : IDisposable
    where TFixture : ITestHostFixture
{
    private bool _disposed;

    /// <summary>
    /// Gets the <see cref="ITestOutputHelper"/> for the current test, which can be used to write diagnostic output.
    /// </summary>
    /// <value>
    /// An <see cref="ITestOutputHelper"/> instance for the currently executing test, 
    /// or <see langword="null"/> if no test is currently executing.
    /// </value>
    /// <remarks>
    /// This property provides access to XUnit's test output helper, allowing you to write 
    /// diagnostic messages that will be captured and associated with the current test execution.
    /// </remarks>
    public ITestOutputHelper? OutputHelper
        => Xunit.TestContext.Current.TestOutputHelper;

    /// <summary>
    /// Gets the <see cref="ITestContext"/> for the current test, providing access to test metadata and state.
    /// </summary>
    /// <value>
    /// An <see cref="ITestContext"/> instance containing information about the currently executing test.
    /// </value>
    /// <remarks>
    /// The test context provides access to test metadata such as the test method, test class, 
    /// display name, and other contextual information about the current test execution.
    /// </remarks>
    public ITestContext TestContext
        => Xunit.TestContext.Current;

    /// <summary>
    /// Gets the collection test fixture that provides the hosted test environment.
    /// </summary>
    /// <value>
    /// The <typeparamref name="TFixture"/> instance that was injected through the constructor.
    /// </value>
    /// <remarks>
    /// The fixture is shared across all tests in the collection and provides access to the 
    /// configured host and its services. Use this property to access fixture-specific functionality.
    /// </remarks>
    public TFixture Fixture { get; } = fixture;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> containing the dependency injection services configured for the test.
    /// </summary>
    /// <value>
    /// An <see cref="IServiceProvider"/> instance from the test fixture that can be used to resolve 
    /// services registered in the host's dependency injection container.
    /// </value>
    /// <remarks>
    /// This is a convenience property that provides direct access to <c>Fixture.Services</c>, 
    /// making it easier to resolve services in your test methods.
    /// </remarks>
    public IServiceProvider Services
        => Fixture.Services;

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="TestHostBase{TFixture}"/> 
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources; 
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    /// <remarks>
    /// <para>
    /// Override this method in derived classes to perform test-specific cleanup operations.
    /// Always call the base implementation to ensure proper disposal.
    /// </para>
    /// <para>
    /// This method is called once per test instance after the test has completed execution.
    /// </para>
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;

        if (!disposing)
            return;

        // output logs?
    }

    /// <summary>
    /// Releases all resources used by the <see cref="TestHostBase{TFixture}"/>.
    /// </summary>
    /// <remarks>
    /// This method is called by XUnit after each test method completes, ensuring proper cleanup 
    /// of test resources. Override <see cref="Dispose(bool)"/> to add custom cleanup logic.
    /// </remarks>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

