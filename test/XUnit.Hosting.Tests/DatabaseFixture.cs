using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace XUnit.Hosting.Tests;

public class DatabaseFixture : TestApplicationFixture
{
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        builder.Configuration.AddUserSecrets<DatabaseFixture>();

        builder.Services.AddSingleton<IService, Service>();
        builder.Services.AddHostedService<DatabaseInitializer>();
    }
}
