
using Microsoft.Extensions.Hosting;

namespace XUnit.Hosting;

public interface ITestHostFixture
{
    IHost Host { get; }

    IServiceProvider Services { get; }
}
