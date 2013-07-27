using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public interface IXunitTestCollectionFactory
    {
        string DisplayName { get; }

        ITestCollection Get(ITypeInfo testClass);
    }
}