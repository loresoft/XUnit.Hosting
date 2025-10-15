using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using XUnit.Hosting.Logging;

namespace XUnit.Hosting.Tests;

[Collection(DatabaseCollection.CollectionName)]
public class LoggingTests(DatabaseFixture databaseFixture)
    : TestHostBase<DatabaseFixture>(databaseFixture)
{
    [Fact]
    public void MemoryLogger()
    {
        var logger = Services.GetRequiredService<ILogger<LoggingTests>>();
        Assert.NotNull(logger);

        logger.LogInformation("This is a test log message.");

        // Retrieve the MemoryLoggerProvider to access the logs
        var memoryLoggerProvider = Services.GetRequiredService<MemoryLoggerProvider>();
        Assert.NotNull(memoryLoggerProvider);

        // Verify that the log message was captured
        var logs = memoryLoggerProvider.Logs();
        Assert.Contains(logs, log => log.Message.Contains("This is a test log message."));
    }
}
