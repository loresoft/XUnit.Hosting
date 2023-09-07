using Xunit;
using Xunit.Abstractions;

namespace XUnit.Hosting.Tests;

[Collection(DatabaseCollection.CollectionName)]
public abstract class DatabaseTestBase : TestHostBase<DatabaseFixture>
{
    protected DatabaseTestBase(ITestOutputHelper output, DatabaseFixture ficture)
        : base(output, ficture)
    {
    }
}
