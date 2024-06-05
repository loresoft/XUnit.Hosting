using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace XUnit.Hosting.Tests;

public class DatabaseInitializer : IHostedService
{
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(ILogger<DatabaseInitializer> logger)
    {
        _logger = logger;
    }

    public static bool IsStarted = false;

    public static bool IsStopped = false;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initialize Database StartAsync()");

        IsStarted = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initialize Database StopAsync()");

        IsStopped = true;
        return Task.CompletedTask;
    }
}
