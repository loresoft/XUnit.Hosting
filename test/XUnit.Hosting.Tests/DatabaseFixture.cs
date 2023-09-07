using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace XUnit.Hosting.Tests;

public class DatabaseFixture : TestHostFixture
{
    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {
        collection.AddSingleton<IService, Service>();
        collection.AddHostedService<DatabaseInitializer>();
    }
}
