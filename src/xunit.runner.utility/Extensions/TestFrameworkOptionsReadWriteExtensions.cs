using System;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Extension methods for reading and writing <see cref="ITestFrameworkDiscoveryOptions"/> and <see cref="ITestFrameworkExecutionOptions"/>.
/// </summary>
public static class TestFrameworkOptionsReadWriteExtensions
{
    // Read/write methods for ITestFrameworkDiscoveryOptions

    /// <summary>
    /// Gets a flag that determines whether diagnostic messages will be emitted.
    /// </summary>
    public static bool GetDiagnosticMessages(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        return discoveryOptions.GetValue<bool>(TestOptionsNames.Discovery.DiagnosticMessages, false);
    }

    /// <summary>
    /// Gets a flag that determines whether diagnostic messages will be emitted.
    /// </summary>
    public static TestMethodDisplay GetMethodDisplay(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        var methodDisplayString = discoveryOptions.GetValue<string>(TestOptionsNames.Discovery.MethodDisplay, null);
        return methodDisplayString == null ? TestMethodDisplay.ClassAndMethod : (TestMethodDisplay)Enum.Parse(typeof(TestMethodDisplay), methodDisplayString);
    }

    /// <summary>
    /// Sets a flag that determines whether diagnostic messages will be emitted.
    /// </summary>
    public static void SetDiagnosticMessages(this ITestFrameworkDiscoveryOptions discoveryOptions, bool value)
    {
        discoveryOptions.SetValue(TestOptionsNames.Discovery.DiagnosticMessages, value);
    }

    /// <summary>
    /// Sets a flag that determines whether diagnostic messages will be emitted.
    /// </summary>
    public static void SetMethodDisplay(this ITestFrameworkDiscoveryOptions discoveryOptions, TestMethodDisplay value)
    {
        discoveryOptions.SetValue(TestOptionsNames.Discovery.MethodDisplay, value.ToString());
    }

    // Read/write methods for ITestFrameworkExecutionOptions

    /// <summary>
    /// Gets a flag that determines whether diagnostic messages will be emitted.
    /// </summary>
    public static bool GetDiagnosticMessages(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetValue<bool>(TestOptionsNames.Execution.DiagnosticMessages, false);
    }

    /// <summary>
    /// Gets a flag to disable parallelization.
    /// </summary>
    public static bool GetDisableParallelization(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetValue<bool>(TestOptionsNames.Execution.DisableParallelization, false);
    }

    /// <summary>
    /// Gets the maximum number of threads to use when running tests in parallel.
    /// If set to 0 (the default value), does not limit the number of threads.
    /// </summary>
    public static int GetMaxParallelThreads(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetValue<int>(TestOptionsNames.Execution.MaxParallelThreads, 0);
    }

    /// <summary>
    /// Gets a flag that determines whether xUnit.net should report test results synchronously.
    /// </summary>
    public static bool GetSynchronousMessageReporting(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetValue<bool>(TestOptionsNames.Execution.SynchronousMessageReporting, false);
    }

    /// <summary>
    /// Sets a flag that determines whether diagnostic messages will be emitted.
    /// </summary>
    public static void SetDiagnosticMessages(this ITestFrameworkExecutionOptions executionOptions, bool value)
    {
        executionOptions.SetValue(TestOptionsNames.Execution.DiagnosticMessages, value);
    }

    /// <summary>
    /// Sets a flag to disable parallelization.
    /// </summary>
    public static void SetDisableParallelization(this ITestFrameworkExecutionOptions executionOptions, bool value)
    {
        executionOptions.SetValue(TestOptionsNames.Execution.DisableParallelization, value);
    }

    /// <summary>
    /// Sets the maximum number of threads to use when running tests in parallel.
    /// If set to 0 (the default value), does not limit the number of threads.
    /// </summary>
    public static void SetMaxParallelThreads(this ITestFrameworkExecutionOptions executionOptions, int value)
    {
        executionOptions.SetValue(TestOptionsNames.Execution.MaxParallelThreads, value);
    }

    /// <summary>
    /// Sets a flag that determines whether xUnit.net should report test results synchronously.
    /// </summary>
    public static void SetSynchronousMessageReporting(this ITestFrameworkExecutionOptions executionOptions, bool value)
    {
        executionOptions.SetValue(TestOptionsNames.Execution.SynchronousMessageReporting, value);
    }
}
