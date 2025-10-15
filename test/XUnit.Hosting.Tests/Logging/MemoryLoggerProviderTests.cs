using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using XUnit.Hosting.Logging;

namespace XUnit.Hosting.Tests.Logging;

public class MemoryLoggerProviderTests
{
    [Fact]
    public void Constructor_WithValidOptions_ShouldSucceed()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);

        // Act
        var provider = new MemoryLoggerProvider(options);

        // Assert
        Assert.NotNull(provider);
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MemoryLoggerProvider(null!));
    }

    [Fact]
    public void CreateLogger_WithValidCategoryName_ShouldReturnLogger()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);

        // Act
        var logger = provider.CreateLogger("TestCategory");

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public void CreateLogger_WithNullCategoryName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => provider.CreateLogger(null!));
    }

    [Fact]
    public void Logger_Log_ShouldAddEntryToLogs()
    {
        // Arrange
        var settings = new MemoryLoggerSettings { MinimumLevel = LogLevel.Information };
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Test message");

        // Assert
        var logs = provider.Logs();
        Assert.Single(logs);
        Assert.Equal("Test message", logs[0].Message);
        Assert.Equal("TestCategory", logs[0].Category);
        Assert.Equal(LogLevel.Information, logs[0].LogLevel);
    }

    [Fact]
    public void Logger_Log_WithException_ShouldCaptureException()
    {
        // Arrange
        var settings = new MemoryLoggerSettings { MinimumLevel = LogLevel.Error };
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");
        var exception = new InvalidOperationException("Test exception");

        // Act
        logger.LogError(exception, "Error occurred");

        // Assert
        var logs = provider.Logs();
        Assert.Single(logs);
        Assert.Equal("Error occurred", logs[0].Message);
        Assert.Equal(exception, logs[0].Exception);
    }

    [Fact]
    public void Logger_Log_WithEventId_ShouldCaptureEventId()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");
        var eventId = new EventId(100, "TestEvent");

        // Act
        logger.Log(LogLevel.Information, eventId, "Test message");

        // Assert
        var logs = provider.Logs();
        Assert.Single(logs);
        Assert.Equal(eventId, logs[0].EventId);
    }

    [Fact]
    public void Logger_Log_BelowMinimumLevel_ShouldNotLog()
    {
        // Arrange
        var settings = new MemoryLoggerSettings { MinimumLevel = LogLevel.Warning };
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogDebug("Debug message");
        logger.LogInformation("Info message");

        // Assert
        var logs = provider.Logs();
        Assert.Empty(logs);
    }

    [Fact]
    public void Logger_Log_WithCustomFilter_ShouldRespectFilter()
    {
        // Arrange
        var settings = new MemoryLoggerSettings
        {
            MinimumLevel = LogLevel.Debug,
            Filter = (category, level) => category.StartsWith("Allow")
        };
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var allowedLogger = provider.CreateLogger("AllowedCategory");
        var blockedLogger = provider.CreateLogger("BlockedCategory");

        // Act
        allowedLogger.LogInformation("Allowed message");
        blockedLogger.LogInformation("Blocked message");

        // Assert
        var logs = provider.Logs();
        Assert.Single(logs);
        Assert.Equal("Allowed message", logs[0].Message);
    }

    [Fact]
    public void Logs_WithCapacityExceeded_ShouldEnforceLimit()
    {
        // Arrange
        var settings = new MemoryLoggerSettings
        {
            MinimumLevel = LogLevel.Debug,
            Capacity = 5
        };
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        for (int i = 0; i < 10; i++)
        {
            logger.LogInformation($"Message {i}");
        }

        // Assert
        var logs = provider.Logs();
        Assert.Equal(5, logs.Count);
        // Should have kept the last 5 messages
        Assert.Equal("Message 5", logs[0].Message);
        Assert.Equal("Message 9", logs[4].Message);
    }

    [Fact]
    public void Logs_ByCategoryName_ShouldFilterByCategory()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger1 = provider.CreateLogger("Category1");
        var logger2 = provider.CreateLogger("Category2");

        // Act
        logger1.LogInformation("Message from Category1");
        logger2.LogInformation("Message from Category2");
        logger1.LogInformation("Another message from Category1");

        // Assert
        var category1Logs = provider.Logs("Category1");
        var category2Logs = provider.Logs("Category2");

        Assert.Equal(2, category1Logs.Count);
        Assert.Single(category2Logs);
    }

    [Fact]
    public void Logs_ByCategoryName_ShouldBeCaseInsensitive()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Test message");

        // Assert
        var logs = provider.Logs("testcategory");
        Assert.Single(logs);
    }

    [Fact]
    public void Logs_ByLogLevel_ShouldFilterByLevel()
    {
        // Arrange
        var settings = new MemoryLoggerSettings { MinimumLevel = LogLevel.Trace };
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogDebug("Debug message");
        logger.LogInformation("Info message");
        logger.LogWarning("Warning message");
        logger.LogError("Error message");

        // Assert
        var warningAndAbove = provider.Logs(LogLevel.Warning);
        Assert.Equal(2, warningAndAbove.Count);
        Assert.All(warningAndAbove, log => Assert.True(log.LogLevel >= LogLevel.Warning));
    }

    [Fact]
    public void Logs_MultipleCategories_ShouldReturnAllLogs()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger1 = provider.CreateLogger("Category1");
        var logger2 = provider.CreateLogger("Category2");

        // Act
        logger1.LogInformation("Message 1");
        logger2.LogInformation("Message 2");
        logger1.LogInformation("Message 3");

        // Assert
        var allLogs = provider.Logs();
        Assert.Equal(3, allLogs.Count);
    }

    [Fact]
    public void Clear_ShouldRemoveAllLogs()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");
        logger.LogInformation("Message 1");
        logger.LogInformation("Message 2");

        // Act
        provider.Clear();

        // Assert
        var logs = provider.Logs();
        Assert.Empty(logs);
    }

    [Fact]
    public void Logger_WithScope_ShouldCaptureScopes()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        using (logger.BeginScope("Scope1"))
        {
            using (logger.BeginScope("Scope2"))
            {
                logger.LogInformation("Message with scopes");
            }
        }

        // Assert
        var logs = provider.Logs();
        Assert.Single(logs);
        Assert.Equal(2, logs[0].Scopes.Count);
    }

    [Fact]
    public void Logger_IsEnabled_ShouldRespectMinimumLevel()
    {
        // Arrange
        var settings = new MemoryLoggerSettings { MinimumLevel = LogLevel.Warning };
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act & Assert
        Assert.False(logger.IsEnabled(LogLevel.Debug));
        Assert.False(logger.IsEnabled(LogLevel.Information));
        Assert.True(logger.IsEnabled(LogLevel.Warning));
        Assert.True(logger.IsEnabled(LogLevel.Error));
    }

    [Fact]
    public void Dispose_ShouldNotThrowException()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);

        // Act & Assert
        provider.Dispose();
    }

    [Fact]
    public void Logger_WithStructuredLogging_ShouldFormatMessage()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("User {UserId} logged in from {IpAddress}", 123, "192.168.1.1");

        // Assert
        var logs = provider.Logs();
        Assert.Single(logs);
        Assert.Contains("123", logs[0].Message);
        Assert.Contains("192.168.1.1", logs[0].Message);
    }

    [Fact]
    public void Logs_ShouldHaveTimestamp()
    {
        // Arrange
        var settings = new MemoryLoggerSettings();
        var options = Options.Create(settings);
        var provider = new MemoryLoggerProvider(options);
        var logger = provider.CreateLogger("TestCategory");
        var beforeLog = DateTime.UtcNow;

        // Act
        logger.LogInformation("Test message");
        var afterLog = DateTime.UtcNow;

        // Assert
        var logs = provider.Logs();
        Assert.Single(logs);
        Assert.InRange(logs[0].Timestamp, beforeLog.AddSeconds(-1), afterLog.AddSeconds(1));
    }
}
