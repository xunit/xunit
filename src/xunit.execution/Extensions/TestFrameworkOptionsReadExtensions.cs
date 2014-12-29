using System;
using Xunit.Abstractions;
using Xunit.Sdk;

/// <summary>
/// Extension methods for reading <see cref="ITestFrameworkDiscoveryOptions"/> and <see cref="ITestFrameworkExecutionOptions"/>.
/// </summary>
public static class TestFrameworkOptionsReadExtensions
{
    // Read methods for ITestFrameworkDiscoveryOptions

    /// <summary>
    /// Gets a flag that determines whether diagnostic messages will be emitted.
    /// </summary>
    public static bool DiagnosticMessages(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        return discoveryOptions.GetValue<bool>(TestOptionsNames.Discovery.DiagnosticMessages, false);
    }

    /// <summary>
    /// Gets a flag that determines whether diagnostic messages will be emitted.
    /// </summary>
    public static TestMethodDisplay MethodDisplay(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        var methodDisplayString = discoveryOptions.GetValue<string>(TestOptionsNames.Discovery.MethodDisplay, null);
        return methodDisplayString == null ? TestMethodDisplay.ClassAndMethod : (TestMethodDisplay)Enum.Parse(typeof(TestMethodDisplay), methodDisplayString);
    }

    /// <summary>
    /// Gets a flag that determines whether theories are pre-enumerated. If they enabled, then the
    /// discovery system will return a test case for each row of test data; they are disabled, then the
    /// discovery system will return a single test case for the theory.
    /// </summary>
    public static bool PreEnumerateTheories(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        return discoveryOptions.GetValue<bool>(TestOptionsNames.Discovery.PreEnumerateTheories, true);
    }

    // Read methods for ITestFrameworkExecutionOptions

    /// <summary>
    /// Gets a flag that determines whether diagnostic messages will be emitted.
    /// </summary>
    public static bool DiagnosticMessages(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetValue<bool>(TestOptionsNames.Execution.DiagnosticMessages, false);
    }

    /// <summary>
    /// Gets a flag to disable parallelization.
    /// </summary>
    public static bool DisableParallelization(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetValue<bool>(TestOptionsNames.Execution.DisableParallelization, false);
    }

    /// <summary>
    /// Gets the maximum number of threads to use when running tests in parallel.
    /// If set to 0 (the default value), does not limit the number of threads.
    /// </summary>
    public static int MaxParallelThreads(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetValue<int>(TestOptionsNames.Execution.MaxParallelThreads, 0);
    }

    /// <summary>
    /// Gets a flag that determines whether xUnit.net should report test results synchronously.
    /// </summary>
    public static bool SynchronousMessageReporting(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetValue<bool>(TestOptionsNames.Execution.SynchronousMessageReporting, false);
    }
}