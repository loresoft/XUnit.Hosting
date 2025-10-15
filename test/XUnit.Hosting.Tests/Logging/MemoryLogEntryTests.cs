using Microsoft.Extensions.Logging;

using XUnit.Hosting.Logging;

namespace XUnit.Hosting.Tests.Logging;

public class MemoryLogEntryTests
{
    [Fact]
    public void Constructor_WithRequiredProperties_ShouldSucceed()
    {
        // Arrange & Act
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: default,
            eventId: default,
            category: "TestCategory",
            message: "Test message");

        // Assert
        Assert.Equal("TestCategory", entry.Category);
        Assert.Equal("Test message", entry.Message);
        Assert.Equal(default(DateTime), entry.Timestamp);
        Assert.Equal(default(LogLevel), entry.LogLevel);
        Assert.Equal(default(EventId), entry.EventId);
        Assert.Null(entry.Exception);
        Assert.Null(entry.State);
        Assert.NotNull(entry.Scopes);
        Assert.Empty(entry.Scopes);
    }

    [Fact]
    public void Constructor_WithAllProperties_ShouldSetAllValues()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var eventId = new EventId(100, "TestEvent");
        var exception = new InvalidOperationException("Test error");
        var scopes = new List<object?> { "Scope1", "Scope2" };
        var state = new { UserId = 123, RequestId = "abc-123" };

        // Act
        var entry = new MemoryLogEntry(
            timestamp: timestamp,
            logLevel: LogLevel.Error,
            eventId: eventId,
            category: "TestCategory",
            message: "Test message",
            exception: exception,
            state: state,
            scopes: scopes);

        // Assert
        Assert.Equal(timestamp, entry.Timestamp);
        Assert.Equal(LogLevel.Error, entry.LogLevel);
        Assert.Equal(eventId, entry.EventId);
        Assert.Equal("TestCategory", entry.Category);
        Assert.Equal("Test message", entry.Message);
        Assert.Equal(exception, entry.Exception);
        Assert.Equal(state, entry.State);
        Assert.Equal(scopes, entry.Scopes);
        Assert.Equal(2, entry.Scopes.Count);
    }

    [Fact]
    public void ToString_WithMinimalData_ShouldFormatCorrectly()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: new EventId(0),
            category: "TestCategory",
            message: "Test message");

        // Act
        var result = entry.ToString();

        // Assert
        Assert.Contains("info: TestCategory[0]", result);
        Assert.Contains("      Test message", result);
    }

    [Fact]
    public void ToString_WithEventId_ShouldIncludeEventId()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Warning,
            eventId: new EventId(42, "TestEvent"),
            category: "TestCategory",
            message: "Test message");

        // Act
        var result = entry.ToString();

        // Assert
        Assert.Contains("warn: TestCategory[42]", result);
        Assert.Contains("      Test message", result);
    }

    [Fact]
    public void ToString_WithException_ShouldIncludeException()
    {
        // Arrange
        var exception = new InvalidOperationException("Something went wrong");
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Error,
            eventId: default,
            category: "TestCategory",
            message: "Error occurred",
            exception: exception);

        // Act
        var result = entry.ToString();

        // Assert
        Assert.Contains("fail: TestCategory[0]", result);
        Assert.Contains("      Error occurred", result);
        Assert.Contains("InvalidOperationException", result);
        Assert.Contains("Something went wrong", result);
    }

    [Fact]
    public void ToString_WithScopes_ShouldIncludeScopes()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: default,
            category: "TestCategory",
            message: "Test message",
            scopes: new List<object?> { "Scope1", "Scope2" });

        // Act
        var result = entry.ToString();

        // Assert
        Assert.Contains("info: TestCategory[0]", result);
        Assert.Contains("      Test message", result);
        Assert.Contains("      => ", result);
        Assert.Contains("\"Scope1\"", result);
        Assert.Contains("\"Scope2\"", result);
    }

    [Fact]
    public void ToString_WithEmptyScopes_ShouldNotIncludeScopesSection()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: default,
            category: "TestCategory",
            message: "Test message",
            scopes: new List<object?>());

        // Act
        var result = entry.ToString();

        // Assert
        Assert.Contains("info: TestCategory[0]", result);
        Assert.Contains("      Test message", result);
        Assert.DoesNotContain("=> ", result);
    }

    [Theory]
    [InlineData(LogLevel.Trace, "trce")]
    [InlineData(LogLevel.Debug, "dbug")]
    [InlineData(LogLevel.Information, "info")]
    [InlineData(LogLevel.Warning, "warn")]
    [InlineData(LogLevel.Error, "fail")]
    [InlineData(LogLevel.Critical, "crit")]
    public void ToString_WithDifferentLogLevels_ShouldFormatCorrectly(LogLevel logLevel, string expectedPrefix)
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: logLevel,
            eventId: default,
            category: "TestCategory",
            message: "Test message");

        // Act
        var result = entry.ToString();

        // Assert
        Assert.Contains($"{expectedPrefix}: TestCategory[0]", result);
        Assert.Contains("      Test message", result);
    }

    [Fact]
    public void ToString_WithCompleteEntry_ShouldFormatAllParts()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");
        var state = new { UserId = 42, Action = "UpdateProfile" };
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Error,
            eventId: new EventId(500, "ValidationError"),
            category: "App.Validation",
            message: "Validation failed for user input",
            exception: exception,
            state: state,
            scopes: new List<object?> { "Scope1" });

        // Act
        var result = entry.ToString();

        // Assert
        Assert.Contains("fail: App.Validation[500]", result);
        Assert.Contains("      Validation failed for user input", result);
        Assert.Contains("      => ", result);
        Assert.Contains("ArgumentException", result);
        Assert.Contains("Invalid argument", result);
    }

    [Fact]
    public void ToString_WithLongMessage_ShouldIncludeFullMessage()
    {
        // Arrange
        var longMessage = new string('A', 1000);
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: default,
            category: "TestCategory",
            message: longMessage);

        // Act
        var result = entry.ToString();

        // Assert
        Assert.Contains(longMessage, result);
    }

    [Fact]
    public void ToString_WithSpecialCharactersInMessage_ShouldHandleCorrectly()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: default,
            category: "TestCategory",
            message: "Message with special chars: @#$%^&*()_+-=[]{}|;':\",./<>?");

        // Act
        var result = entry.ToString();

        // Assert
        Assert.Contains("Message with special chars: @#$%^&*()_+-=[]{}|;':\",./<>?", result);
    }

    [Fact]
    public void Scopes_DefaultValue_ShouldBeEmptyList()
    {
        // Arrange & Act
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: default,
            eventId: default,
            category: "TestCategory",
            message: "Test message");

        // Assert
        Assert.NotNull(entry.Scopes);
        Assert.Empty(entry.Scopes);
    }

    [Fact]
    public void ToString_WithNestedExceptions_ShouldIncludeInnerException()
    {
        // Arrange
        var innerException = new ArgumentException("Inner error");
        var outerException = new InvalidOperationException("Outer error", innerException);
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Error,
            eventId: default,
            category: "TestCategory",
            message: "Error with nested exceptions",
            exception: outerException);

        // Act
        var result = entry.ToString();

        // Assert
        Assert.Contains("InvalidOperationException", result);
        Assert.Contains("Outer error", result);
        Assert.Contains("ArgumentException", result);
        Assert.Contains("Inner error", result);
    }

    [Fact]
    public void ToString_MultiLineFormat_ShouldHaveCorrectStructure()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: new EventId(42),
            category: "TestCategory",
            message: "Test message");

        // Act
        var result = entry.ToString();
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        Assert.Equal(2, lines.Length);
        Assert.Equal("info: TestCategory[42]", lines[0]);
        Assert.Equal("      Test message", lines[1]);
    }

    [Fact]
    public void ToString_WithMultipleScopes_ShouldIncludeAllScopes()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: default,
            category: "TestCategory",
            message: "Test message",
            scopes: new List<object?> { "Scope1", "Scope2", "Scope3" });

        // Act
        var result = entry.ToString();

        // Assert
        Assert.Contains("      => ", result);
        Assert.Contains("\"Scope1\"", result);
        Assert.Contains("\"Scope2\"", result);
        Assert.Contains("\"Scope3\"", result);
    }

    [Fact]
    public void ToString_LogLevelNone_ShouldUseNoneAsPrefix()
    {
        // Arrange
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.None,
            eventId: default,
            category: "TestCategory",
            message: "Test message");

        // Act
        var result = entry.ToString();

        // Assert
        Assert.Contains("none: TestCategory[0]", result);
    }

    [Fact]
    public void ToString_WithState_ShouldIncludeState()
    {
        // Arrange
        var state = new { UserId = 123, RequestId = "abc-123" };
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: LogLevel.Information,
            eventId: default,
            category: "TestCategory",
            message: "Test message",
            state: state);

        // Act
        var result = entry.ToString();

        // Assert
        Assert.Contains("info: TestCategory[0]", result);
        Assert.Contains("      Test message", result);
        Assert.Contains("      => ", result);
        Assert.Contains("UserId", result);
        Assert.Contains("123", result);
        Assert.Contains("RequestId", result);
        Assert.Contains("abc-123", result);
    }

    [Fact]
    public void State_DefaultValue_ShouldBeNull()
    {
        // Arrange & Act
        var entry = new MemoryLogEntry(
            timestamp: default,
            logLevel: default,
            eventId: default,
            category: "TestCategory",
            message: "Test message");

        // Assert
        Assert.Null(entry.State);
    }
}
