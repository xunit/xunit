using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Extension methods for <see cref="ITestFrameworkDiscoverer"/> and <see cref="ITestFrameworkExecutor"/>.
/// </summary>
public static class TestFrameworkExtensions
{
    /// <summary>
    /// Starts the process of finding all tests in an assembly.
    /// </summary>
    /// <param name="discoverer">The discoverer.</param>
    /// <param name="includeSourceInformation">Whether to include source file information, if possible.</param>
    /// <param name="discoveryMessageSink">The message sink to report results back to.</param>
    /// <param name="discoveryOptions">The options used by the test framework during discovery.</param>
    public static void Find(this ITestFrameworkDiscoverer discoverer,
                            bool includeSourceInformation,
                            IMessageSinkWithTypes discoveryMessageSink,
                            ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        discoverer.Find(includeSourceInformation, MessageSinkAdapter.Wrap(discoveryMessageSink), discoveryOptions);
    }

    /// <summary>
    /// Starts the process of finding all tests in a class.
    /// </summary>
    /// <param name="discoverer">The discoverer.</param>
    /// <param name="typeName">The fully qualified type name to find tests in.</param>
    /// <param name="includeSourceInformation">Whether to include source file information, if possible.</param>
    /// <param name="discoveryMessageSink">The message sink to report results back to.</param>
    /// <param name="discoveryOptions">The options used by the test framework during discovery.</param>
    public static void Find(this ITestFrameworkDiscoverer discoverer,
                            string typeName,
                            bool includeSourceInformation,
                            IMessageSinkWithTypes discoveryMessageSink,
                            ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        discoverer.Find(typeName, includeSourceInformation, MessageSinkAdapter.Wrap(discoveryMessageSink), discoveryOptions);
    }

    /// <summary>
    /// Starts the process of running all the tests in the assembly.
    /// </summary>
    /// <param name="executor">The executor.</param>
    /// <param name="executionMessageSink">The message sink to report results back to.</param>
    /// <param name="discoveryOptions">The options to be used during test discovery.</param>
    /// <param name="executionOptions">The options to be used during test execution.</param>
    public static void RunAll(this ITestFrameworkExecutor executor,
                              IMessageSinkWithTypes executionMessageSink,
                              ITestFrameworkDiscoveryOptions discoveryOptions,
                              ITestFrameworkExecutionOptions executionOptions)
    {
        executor.RunAll(MessageSinkAdapter.Wrap(executionMessageSink), discoveryOptions, executionOptions);
    }

    /// <summary>
    /// Starts the process of running selected tests in the assembly.
    /// </summary>
    /// <param name="executor">The executor.</param>
    /// <param name="testCases">The test cases to run.</param>
    /// <param name="executionMessageSink">The message sink to report results back to.</param>
    /// <param name="executionOptions">The options to be used during test execution.</param>
    public static void RunTests(this ITestFrameworkExecutor executor,
                                IEnumerable<ITestCase> testCases,
                                IMessageSinkWithTypes executionMessageSink,
                                ITestFrameworkExecutionOptions executionOptions)
    {
        executor.RunTests(testCases, MessageSinkAdapter.Wrap(executionMessageSink), executionOptions);
    }
}
