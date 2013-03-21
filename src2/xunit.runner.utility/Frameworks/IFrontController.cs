using Xunit.Abstractions;

namespace Xunit
{
    public interface IFrontController : ITestFrameworkDiscoverer, ITestFrameworkExecutor
    {
    }
}