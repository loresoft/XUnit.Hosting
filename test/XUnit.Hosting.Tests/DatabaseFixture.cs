using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Testcontainers.MsSql;

using XUnit.Hosting.Logging;
using XUnit.Hosting.Tests.Data;

namespace XUnit.Hosting.Tests;

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

        builder.Configuration.AddUserSecrets<DatabaseFixture>();
        builder.Logging.AddMemoryLogger();

        // change database from container default
        var connectionBuilder = new SqlConnectionStringBuilder(_msSqlContainer.GetConnectionString());
        connectionBuilder.InitialCatalog = "SampleDataDocker";

        // Register DbContext with container connection string
        builder.Services.AddDbContext<SampleDataContext>(options =>
            options.UseSqlServer(connectionBuilder.ToString())
        );

        builder.Services.AddSingleton<IService, Service>();
        builder.Services.AddHostedService<DatabaseInitialize>();
    }
}
