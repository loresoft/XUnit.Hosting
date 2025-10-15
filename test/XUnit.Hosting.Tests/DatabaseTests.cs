using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using XUnit.Hosting.Tests.Data;

namespace XUnit.Hosting.Tests;

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
