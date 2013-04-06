using System;
using System.Collections.Generic;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents an implementation of the execution part of a test framework.
    /// </summary>
    public interface ITestFrameworkExecutor : IDisposable
    {
        /// <summary>
        /// Starts the process of running tests.
        /// </summary>
        /// <param name="testMethods">The test methods to run; if null, all tests in the assembly are run.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        void Run(IEnumerable<ITestCase> testMethods, IMessageSink messageSink/*, CancellationToken token = default(CancellationToken)*/);
    }
}