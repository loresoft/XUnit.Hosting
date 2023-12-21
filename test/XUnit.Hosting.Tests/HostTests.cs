using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;

namespace XUnit.Hosting.Tests;

[Collection(DatabaseCollection.CollectionName)]
public class HostTests : DatabaseTestBase
{
    public HostTests(ITestOutputHelper output, DatabaseFixture databaseFixture) : base(output, databaseFixture)
    {
    }

    [Fact]
    public void GetService()
    {
        var service = Services.GetRequiredService<IService>();

        Assert.False(Service.IsRun);
        service.Run();

        Assert.True(Service.IsRun);
        Assert.True(DatabaseInitializer.IsStarted);
    }
}
