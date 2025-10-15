# XUnit.Hosting

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Build Project](https://github.com/loresoft/XUnit.Hosting/actions/workflows/dotnet.yml/badge.svg)](https://github.com/loresoft/XUnit.Hosting/actions/workflows/dotnet.yml)
[![Coverage Status](https://coveralls.io/repos/github/loresoft/XUnit.Hosting/badge.svg?branch=main)](https://coveralls.io/github/loresoft/XUnit.Hosting?branch=main)
[![XUnit.Hosting](https://img.shields.io/nuget/v/XUnit.Hosting.svg)](https://www.nuget.org/packages/XUnit.Hosting/)

A testing library that integrates Microsoft.Extensions.Hosting with xUnit v3, enabling dependency injection, configuration management, and hosted services in your unit tests.

## Features

- **Host Builder Integration** - Leverage `HostApplicationBuilder` in your xUnit tests
- **Dependency Injection** - Full support for Microsoft.Extensions.DependencyInjection
- **Configuration Management** - Use appsettings.json, user secrets, and other configuration providers
- **Memory Logger** - Built-in in-memory logging provider for test verification
- **Collection Fixtures** - Share application host across multiple tests

## Installation

```bash
dotnet add package XUnit.Hosting
```

## Quick Start

### 1. Create a Test Fixture

Create a fixture that inherits from `TestApplicationFixture` and configure your application services:

```csharp
public class MyTestFixture : TestApplicationFixture
{
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        // Add configuration
        builder.Configuration.AddUserSecrets<MyTestFixture>();

        // Register services
        builder.Services.AddSingleton<IMyService, MyService>();
        
        // Add hosted services
        builder.Services.AddHostedService<MyBackgroundService>();
    }
}
```

### 2. Define a Collection

Create a collection definition to share the fixture across tests:

```csharp
// support capturing console and trace output in xunit v3
[assembly: CaptureConsole]
[assembly: CaptureTrace]

[CollectionDefinition(CollectionName)]
public class MyTestCollection : ICollectionFixture<MyTestFixture>
{
    public const string CollectionName = nameof(MyTestCollection);
}
```

### 3. Write Your Tests

```csharp
[Collection(MyTestCollection.CollectionName)]
public class ServiceTests : TestHostBase<MyTestFixture>
{
    public ServiceTests(MyTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void TestMyService()
    {
        // Arrange - Get service from DI container
        var service = Services.GetRequiredService<IMyService>();

        // Act
        var result = service.DoSomething();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void TestConfiguration()
    {
        // Access configuration
        var config = Services.GetRequiredService<IConfiguration>();
        var value = config["MyKey"];
        
        Assert.NotNull(value);
    }
}
```

## Core Components

### TestApplicationFixture

Abstract base class for creating test fixtures with host support. This class implements `ITestHostFixture` and `IDisposable`, providing lazy initialization of the host to ensure it's only created when first accessed.

**Key Properties:**

- **`Host`** - The IHost instance (lazily initialized and automatically started)
- **`Services`** - IServiceProvider for dependency resolution

**Methods to Override:**

- **`ConfigureApplication(HostApplicationBuilder builder)`** - Configure services, logging, and configuration. The default implementation sets the minimum log level to `LogLevel.Debug` and configures console logging with scopes for detailed test diagnostics.
- **`CreateBuilderSettings()`** - Customize host builder settings such as environment name, application name, or content root path. Returns `null` by default to use default settings.
- **`CreateHost()`** - Override host creation logic for advanced scenarios. The default implementation creates settings, builds the host, and starts it automatically.

### TestHostBase&lt;TFixture&gt;

Base class for test classes that provides convenient access to the hosted test environment. This class implements `IDisposable` for proper cleanup of test resources.

**Properties:**

- **`OutputHelper`** - ITestOutputHelper? for writing diagnostic output to the current test
- **`TestContext`** - ITestContext providing access to test metadata and state
- **`Fixture`** - Access to the shared test fixture (TFixture instance)
- **`Services`** - IServiceProvider for resolving services (convenience property for `Fixture.Services`)

### ITestHostFixture

Interface that defines the contract for XUnit collection fixtures providing access to a configured host and dependency injection container.

**Properties:**

- **`Host`** - The IHost instance configured and started for the test collection
- **`Services`** - IServiceProvider for dependency resolution (typically equivalent to `Host.Services`)

## Memory Logger

XUnit.Hosting includes a built-in in-memory logger for capturing and asserting log output in tests.

### Setup

```csharp
public class MyTestFixture : TestApplicationFixture
{
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        // Add memory logger (registered as singleton in DI)
        builder.Logging.AddMemoryLogger();
        
        // Or with settings
        builder.Logging.AddMemoryLogger(settings =>
        {
            settings.MinimumLevel = LogLevel.Information;
            settings.Capacity = 2048;
            settings.Filter = (category, level) => level >= LogLevel.Warning;
        });
    }
}
```

### Usage in Tests

```csharp
[Collection(MyTestCollection.CollectionName)]
public class LoggingTests(MyTestFixture fixture) : TestHostBase<MyTestFixture>(fixture)
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

        // Filter by category
        var categoryLogs = memoryLoggerProvider.Logs("XUnit.Hosting.Tests.LoggingTests");
        Assert.NotEmpty(categoryLogs);

        // Filter by log level
        var infoLogs = memoryLoggerProvider.Logs(LogLevel.Information);
        Assert.NotEmpty(infoLogs);
    }
}
```

### MemoryLoggerSettings

Configure the memory logger with these options:

- **`MinimumLevel`** - Minimum log level to capture (default: `LogLevel.Debug`)
- **`Capacity`** - Maximum number of log entries to keep (default: 1024)
- **`Filter`** - Custom filter function for fine-grained control

### MemoryLogEntry

Log entries captured include:

- **`Timestamp`** - DateTime when the log entry was created
- **`LogLevel`** - The log level of the entry (Trace, Debug, Information, Warning, Error, Critical)
- **`EventId`** - Event identifier associated with the log entry
- **`Category`** - Category name of the logger that created this entry
- **`Message`** - Formatted log message
- **`Exception`** - Exception associated with the log entry, if any (nullable)
- **`State`** - The state object passed to the logger (nullable)
- **`Scopes`** - Read-only collection of scope values that were active when the log entry was created

## Integration Testing with Docker Databases

XUnit.Hosting works seamlessly with Testcontainers to provide isolated database environments for integration tests. This approach uses `IAsyncLifetime` to manage container lifecycle and `IHostedService` to seed the database.

### Install Testcontainers

```bash
dotnet add package Testcontainers.MsSql
```

### Create a Database Fixture with Testcontainers

```csharp
public class DatabaseFixture : TestApplicationFixture, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("P@ssw0rd123!")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }

    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        // Change database from container default
        var connectionBuilder = new SqlConnectionStringBuilder(_msSqlContainer.GetConnectionString());
        connectionBuilder.InitialCatalog = "SampleDataDocker";

        // Register DbContext with container connection string
        builder.Services.AddDbContext<SampleDataContext>(options =>
            options.UseSqlServer(connectionBuilder.ToString())
        );

        // Register database seeding hosted service
        builder.Services.AddHostedService<DatabaseInitialize>();
    }
}
```

### Create a Database Seeder with IHostedService

```csharp
public class DatabaseInitialize : IHostedService
{
    private readonly ILogger<DatabaseInitialize> _logger;
    private readonly SampleDataContext _dataContext;

    public DatabaseInitialize(ILogger<DatabaseInitialize> logger, SampleDataContext dataContext)
    {
        _logger = logger;
        _dataContext = dataContext;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initialize Database StartAsync()");

        // Ensure database is created and apply migrations
        await _dataContext.Database.EnsureCreatedAsync(cancellationToken);
        // Or use migrations: await _dataContext.Database.MigrateAsync(cancellationToken);

        // Seed test data
        _dataContext.Users.AddRange(
            new User { Name = "Test User 1", Email = "user1@test.com" },
            new User { Name = "Test User 2", Email = "user2@test.com" }
        );

        await _dataContext.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initialize Database StopAsync()");
        return Task.CompletedTask;
    }
}
```

### Define the Collection

```csharp
// Support capturing console and trace output
[assembly: CaptureConsole]
[assembly: CaptureTrace]

[CollectionDefinition(CollectionName)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string CollectionName = nameof(DatabaseCollection);
}
```

### Write Database Tests

```csharp
[Collection(DatabaseCollection.CollectionName)]
public class DatabaseTests : TestHostBase<DatabaseFixture>
{
    public DatabaseTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        // Arrange
        var dbContext = Services.GetRequiredService<SampleDataContext>();

        // Act
        var user = await dbContext.Users.FindAsync([1], TestContext.CancellationToken);

        // Assert
        Assert.NotNull(user);
        Assert.Equal("Test User 1", user.Name);
        Assert.Equal("user1@test.com", user.Email);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsSeededUsers()
    {
        // Arrange
        var dbContext = Services.GetRequiredService<SampleDataContext>();

        // Act
        var users = await dbContext.Users.ToListAsync(TestContext.CancellationToken);

        // Assert
        Assert.True(users.Count >= 2);
    }
}
```

## Best Practices

1. **Share Expensive Resources** - Use collection fixtures to share database connections, HTTP clients, etc.
2. **Clear State Between Tests** - Reset or clear shared resources in test constructors if needed
3. **Use Memory Logger** - Verify logging behavior in your tests
4. **Keep Fixtures Focused** - Create separate fixtures for different test scenarios
5. **Leverage Configuration** - Use user secrets and environment variables for test configuration

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
