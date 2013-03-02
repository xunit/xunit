using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public interface IXunitTestCase : IMethodTestCase
    {
        IEnumerable<object> Arguments { get; }

        void Run(IMessageSink messageSink);
    }
}