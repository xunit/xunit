using System.Collections.Generic;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a test framework. Instances of this interface are created inside the testing app domain
    /// (not the runner app domain), and thus must always derive from <see cref="LongLivedMarshalByRefObject"/>.
    /// All operations are asynchronous, and the message sink is how the framework communicates results.
    /// </summary>
    public interface ITestFramework
    {
        /// <summary>
        /// Starts the process of finding all tests in an assembly.
        /// </summary>
        /// <param name="includeSourceInformation">Whether to include source file information, if possible.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        void Find(bool includeSourceInformation, IMessageSink messageSink);

        /// <summary>
        /// Starts the process of finding all tests in a class.
        /// </summary>
        /// <param name="type">The class to find tests in.</param>
        /// <param name="includeSourceInformation">Whether to include source file information, if possible.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        void Find(ITypeInfo type, bool includeSourceInformation, IMessageSink messageSink);

        /// <summary>
        /// Starts the process of running tests.
        /// </summary>
        /// <param name="testMethods">The test methods to run; if null, all tests in the assembly are run.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        void Run(IEnumerable<ITestCase> testMethods, IMessageSink messageSink/*, CancellationToken token = default(CancellationToken)*/);
    }
}
