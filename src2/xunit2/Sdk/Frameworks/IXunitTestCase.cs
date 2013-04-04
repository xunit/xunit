using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public interface IXunitTestCase : ITestCase
    {
        object[] Arguments { get; }

        bool Run(IMessageSink messageSink);
    }
}