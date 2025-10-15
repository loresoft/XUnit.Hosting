// support capturing console and trace output
[assembly: CaptureConsole]
[assembly: CaptureTrace]

namespace XUnit.Hosting.Tests;

[CollectionDefinition(CollectionName)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string CollectionName = nameof(DatabaseCollection);
}
