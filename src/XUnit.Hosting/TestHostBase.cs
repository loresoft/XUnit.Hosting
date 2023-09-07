using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

using XUnit.Hosting.Logging;

namespace XUnit.Hosting;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TFixture">The type of the fixture.</typeparam>
/// <seealso cref="System.IDisposable" />
public abstract class TestHostBase<TFixture> : IDisposable
    where TFixture : TestHostFixture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestHostBase{TFixture}"/> class.
    /// </summary>
    /// <param name="output">Represents a class which can be used to provide test output</param>
    /// <param name="ficture"> The collection test fixture.</param>
    protected TestHostBase(ITestOutputHelper output, TFixture ficture)
    {
        Output = output;
        Fixture = ficture;
    }

    /// <summary>
    /// Represents a class which can be used to provide test output.
    /// </summary>
    /// <value>
    /// The output helper.
    /// </value>
    public ITestOutputHelper Output { get; }

    /// <summary>
    /// Gets the collection test fixture.
    /// </summary>
    /// <value>
    /// The collection test fixture.
    /// </value>
    public TFixture Fixture { get; }

    /// <summary>
    /// Gets the services configured for this test
    /// </summary>
    /// <value>
    /// The services configured for this test.
    /// </value>
    public IServiceProvider Services => Fixture.Services;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() => WriteLogs();


    /// <summary>
    /// Writes the queued logs to <see cref="Output"/>.
    /// </summary>
    protected void WriteLogs()
    {
        var memoryLogger = GetMemoryLogger();
        if (memoryLogger == null)
            return;

        var logs = memoryLogger.GetEntries();
        if (logs == null || logs.Count == 0)
            return;

        foreach (var log in logs)
            Output.WriteLine(log.ToString());

        // reset logger
        memoryLogger.Clear();
    }

    /// <summary>
    /// Gets the memory log entries.
    /// </summary>
    /// <returns>A readonly list of log entries</returns>
    protected IReadOnlyList<MemoryLogEntry> GetLogEntries()
    {
        var memoryLogger = GetMemoryLogger();
        if (memoryLogger == null)
            return Array.Empty<MemoryLogEntry>();

        return memoryLogger.GetEntries();
    }

    /// <summary>
    /// Gets the memory logging provider.
    /// </summary>
    /// <returns></returns>
    protected MemoryLoggerProvider? GetMemoryLogger()
    {
        // find memory logger
        var loggers = Services.GetServices<ILoggerProvider>();
        if (loggers == null)
            return null;

        return loggers
            .OfType<MemoryLoggerProvider>()
            .FirstOrDefault();
    }
}
