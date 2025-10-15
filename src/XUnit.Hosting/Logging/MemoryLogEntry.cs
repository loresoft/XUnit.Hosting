using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;

namespace XUnit.Hosting.Logging;

/// <summary>
/// Represents a single log entry captured by the memory logger.
/// </summary>
public record MemoryLogEntry
{
    /// <summary>
    /// Gets the timestamp when the log entry was created.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets the log level of the entry.
    /// </summary>
    public LogLevel LogLevel { get; init; }

    /// <summary>
    /// Gets the event identifier associated with the log entry.
    /// </summary>
    public EventId EventId { get; init; }

    /// <summary>
    /// Gets the category name of the logger that created this entry.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Gets the formatted log message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the exception associated with the log entry, if any.
    /// </summary>
    public Exception? Exception { get; init; }

    public object? State { get; init; }

    /// <summary>
    /// Gets the collection of scope values that were active when the log entry was created.
    /// </summary>
    public IReadOnlyList<object?> Scopes { get; init; } = [];

    /// <summary>
    /// Returns a formatted string representation of the log entry.
    /// </summary>
    /// <returns>A string containing the log level, category, event ID, message, scopes, and exception (if present).</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();

        // Format: <log level>: <category>[<event id>]
        sb.Append(GetLogLevelString(LogLevel));
        sb.Append(": ");
        sb.Append(Category);
        sb.Append('[');
        sb.Append(EventId.Id);
        sb.Append(']');
        sb.AppendLine();

        // Message on next line with indentation
        sb.Append(' ', 6);
        sb.Append(Message);

        if (State is not null)
        {
            sb.AppendLine();
            sb.Append(' ', 6);
            sb.Append("=> ");
            sb.Append(JsonSerializer.Serialize(State));
        }

        // Scopes if present
        foreach (var scope in Scopes)
        {
            sb.AppendLine();
            sb.Append(' ', 6);
            sb.Append("=> ");
            sb.Append(JsonSerializer.Serialize(scope));
        }

        // Exception if present
        if (Exception != null)
        {
            sb.AppendLine();
            sb.Append(Exception);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a <see cref="LogLevel"/> to its short string representation.
    /// </summary>
    /// <param name="logLevel">The log level to convert.</param>
    /// <returns>A short string representation of the log level (e.g., "info", "warn", "fail").</returns>
    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => logLevel.ToString().ToLowerInvariant()
        };
    }
}
