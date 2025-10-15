using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace XUnit.Hosting.Tests;

[Collection(DatabaseCollection.CollectionName)]
public class HostTests(DatabaseFixture databaseFixture)
    : TestHostBase<DatabaseFixture>(databaseFixture)
{
    [Fact]
    public void GetService()
    {
        var service = Services.GetRequiredService<IService>();

        Assert.False(Service.IsRun);
        service.Run();

        Assert.True(Service.IsRun);
        Assert.True(DatabaseInitialize.IsStarted);
    }

    [Fact]
    public void SecretConfiguration()
    {
        var configuration = Services.GetRequiredService<IConfiguration>();
        Assert.NotNull(configuration);

        var secretValue = configuration["SecretKey"];

        var configurationRoot = configuration as IConfigurationRoot;
        Assert.NotNull(configurationRoot);

        var configurationDebugView = configurationRoot.GetDebugView();
        Assert.NotNull(configurationDebugView);

    }
}
