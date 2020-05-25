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
    public static bool? GetDiagnosticMessages(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.DiagnosticMessages);
    }

    /// <summary>
    /// Gets a flag that determines whether internal diagnostic messages will be emitted.
    /// </summary>
    public static bool? GetInternalDiagnosticMessages(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.InternalDiagnosticMessages);
    }

    /// <summary>
    /// Gets a flag that determines whether diagnostic messages will be emitted. If the flag is not
    /// set, returns the default value (<c>false</c>).
    /// </summary>
    public static bool GetDiagnosticMessagesOrDefault(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        return discoveryOptions.GetDiagnosticMessages() ?? false;
    }

    /// <summary>
    /// Gets a flag that determines whether internal diagnostic messages will be emitted. If the flag is not
    /// set, returns the default value (<c>false</c>).
    /// </summary>
    public static bool GetInternalDiagnosticMessagesOrDefault(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        return discoveryOptions.GetInternalDiagnosticMessages() ?? false;
    }

    /// <summary>
    /// Gets a flag that determines the default display name format for test methods.
    /// </summary>
    public static TestMethodDisplay? GetMethodDisplay(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        var methodDisplayString = discoveryOptions.GetValue<string>(TestOptionsNames.Discovery.MethodDisplay);
        return methodDisplayString != null ? (TestMethodDisplay?)Enum.Parse(typeof(TestMethodDisplay), methodDisplayString) : null;
    }

    /// <summary>
    /// Gets a flag that determines the default display name format for test methods. If the flag is not present,
    /// returns the default value (<see cref="TestMethodDisplay.ClassAndMethod"/>).
    /// </summary>
    public static TestMethodDisplay GetMethodDisplayOrDefault(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        return discoveryOptions.GetMethodDisplay() ?? TestMethodDisplay.ClassAndMethod;
    }

    /// <summary>
    /// Gets a flag that determines the default display name format options for test methods.
    /// </summary>
    public static TestMethodDisplayOptions? GetMethodDisplayOptions(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        var methodDisplayOptionsString = discoveryOptions.GetValue<string>(TestOptionsNames.Discovery.MethodDisplayOptions);
        return methodDisplayOptionsString != null ? (TestMethodDisplayOptions?)Enum.Parse(typeof(TestMethodDisplayOptions), methodDisplayOptionsString) : null;
    }

    /// <summary>
    /// Gets a flag that determines the default display name format options for test methods. If the flag is not present,
    /// returns the default value (<see cref="TestMethodDisplayOptions.None"/>).
    /// </summary>
    public static TestMethodDisplayOptions GetMethodDisplayOptionsOrDefault(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        return discoveryOptions.GetMethodDisplayOptions() ?? TestMethodDisplayOptions.None;
    }

    /// <summary>
    /// Gets a flag that determines whether theories are pre-enumerated. If they enabled, then the
    /// discovery system will return a test case for each row of test data; they are disabled, then the
    /// discovery system will return a single test case for the theory.
    /// </summary>
    public static bool? GetPreEnumerateTheories(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.PreEnumerateTheories);
    }

    /// <summary>
    /// Gets a flag that determines whether theories are pre-enumerated. If they enabled, then the
    /// discovery system will return a test case for each row of test data; they are disabled, then the
    /// discovery system will return a single test case for the theory. If the flag is not present,
    /// returns the default value (<c>true</c>).
    /// </summary>
    public static bool GetPreEnumerateTheoriesOrDefault(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        return discoveryOptions.GetPreEnumerateTheories() ?? true;
    }

    /// <summary>
    /// Gets a flag that determines whether xUnit.net should report test results synchronously.
    /// </summary>
    public static bool? GetSynchronousMessageReporting(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.SynchronousMessageReporting);
    }

    /// <summary>
    /// Gets a flag that determines whether xUnit.net should report test results synchronously.
    /// If the flag is not set, returns the default value (<c>false</c>).
    /// </summary>
    public static bool GetSynchronousMessageReportingOrDefault(this ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        return discoveryOptions.GetSynchronousMessageReporting() ?? false;
    }

    /// <summary>
    /// Sets a flag that determines whether diagnostic messages will be emitted.
    /// </summary>
    public static void SetDiagnosticMessages(this ITestFrameworkDiscoveryOptions discoveryOptions, bool? value)
    {
        discoveryOptions.SetValue(TestOptionsNames.Discovery.DiagnosticMessages, value);
    }

    /// <summary>
    /// Sets a flag that determines whether internal diagnostic messages will be emitted.
    /// </summary>
    public static void SetInternalDiagnosticMessages(this ITestFrameworkDiscoveryOptions discoveryOptions, bool? value)
    {
        discoveryOptions.SetValue(TestOptionsNames.Discovery.InternalDiagnosticMessages, value);
    }

    /// <summary>
    /// Sets a flag that determines the default display name format for test methods.
    /// </summary>
    public static void SetMethodDisplay(this ITestFrameworkDiscoveryOptions discoveryOptions, TestMethodDisplay? value)
    {
        discoveryOptions.SetValue(TestOptionsNames.Discovery.MethodDisplay, value.HasValue ? value.GetValueOrDefault().ToString() : null);
    }

    /// <summary>
    /// Sets the flags that determine the default display options for test methods.
    /// </summary>
    public static void SetMethodDisplayOptions(this ITestFrameworkDiscoveryOptions discoveryOptions, TestMethodDisplayOptions? value)
    {
        discoveryOptions.SetValue(TestOptionsNames.Discovery.MethodDisplayOptions, value.HasValue ? value.GetValueOrDefault().ToString() : null);
    }

    /// <summary>
    /// Sets a flag that determines whether theories are pre-enumerated. If they enabled, then the
    /// discovery system will return a test case for each row of test data; they are disabled, then the
    /// discovery system will return a single test case for the theory.
    /// </summary>
    public static void SetPreEnumerateTheories(this ITestFrameworkDiscoveryOptions discoveryOptions, bool? value)
    {
        discoveryOptions.SetValue(TestOptionsNames.Discovery.PreEnumerateTheories, value);
    }

    /// <summary>
    /// Sets a flag that determines whether xUnit.net should report test results synchronously.
    /// </summary>
    public static void SetSynchronousMessageReporting(this ITestFrameworkDiscoveryOptions discoveryOptions, bool? value)
    {
        discoveryOptions.SetValue(TestOptionsNames.Discovery.SynchronousMessageReporting, value);
    }

    // Read/write methods for ITestFrameworkExecutionOptions

    /// <summary>
    /// Gets a flag that determines whether diagnostic messages will be emitted.
    /// </summary>
    public static bool? GetDiagnosticMessages(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.DiagnosticMessages);
    }

    /// <summary>
    /// Gets a flag that determines whether internal diagnostic messages will be emitted.
    /// </summary>
    public static bool? GetInternalDiagnosticMessages(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.InternalDiagnosticMessages);
    }

    /// <summary>
    /// Gets a flag that determines whether diagnostic messages will be emitted. If the flag is not
    /// present, returns the default value (<c>false</c>).
    /// </summary>
    public static bool GetDiagnosticMessagesOrDefault(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetDiagnosticMessages() ?? false;
    }

    /// <summary>
    /// Gets a flag that determines whether internal diagnostic messages will be emitted. If the flag is not
    /// present, returns the default value (<c>false</c>).
    /// </summary>
    public static bool GetInternalDiagnosticMessagesOrDefault(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetInternalDiagnosticMessages() ?? false;
    }

    /// <summary>
    /// Gets a flag to disable parallelization.
    /// </summary>
    public static bool? GetDisableParallelization(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.DisableParallelization);
    }

    /// <summary>
    /// Gets a flag to disable parallelization. If the flag is not present, returns the
    /// default value (<c>false</c>).
    /// </summary>
    public static bool GetDisableParallelizationOrDefault(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetDisableParallelization() ?? false;
    }

    /// <summary>
    /// Gets the maximum number of threads to use when running tests in parallel.
    /// </summary>
    public static int? GetMaxParallelThreads(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetValue<int?>(TestOptionsNames.Execution.MaxParallelThreads);
    }

    /// <summary>
    /// Gets the maximum number of threads to use when running tests in parallel. If set to 0 (or not set),
    /// the value of <see cref="Environment.ProcessorCount"/> is used; if set to a value less
    /// than 0, does not limit the number of threads.
    /// </summary>
    public static int GetMaxParallelThreadsOrDefault(this ITestFrameworkExecutionOptions executionOptions)
    {
        var result = executionOptions.GetMaxParallelThreads();
        if (result == null || result == 0)
            return Environment.ProcessorCount;

        return result.GetValueOrDefault();
    }

    /// <summary>
    /// Gets a flag that determines whether xUnit.net should report test results synchronously.
    /// </summary>
    public static bool? GetSynchronousMessageReporting(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.SynchronousMessageReporting);
    }

    /// <summary>
    /// Gets a flag that determines whether xUnit.net should report test results synchronously.
    /// If the flag is not set, returns the default value (<c>false</c>).
    /// </summary>
    public static bool GetSynchronousMessageReportingOrDefault(this ITestFrameworkExecutionOptions executionOptions)
    {
        return executionOptions.GetSynchronousMessageReporting() ?? false;
    }

    /// <summary>
    /// Sets a flag that determines whether diagnostic messages will be emitted.
    /// </summary>
    public static void SetDiagnosticMessages(this ITestFrameworkExecutionOptions executionOptions, bool? value)
    {
        executionOptions.SetValue(TestOptionsNames.Execution.DiagnosticMessages, value);
    }

    /// <summary>
    /// Sets a flag that determines whether internal diagnostic messages will be emitted.
    /// </summary>
    public static void SetInternalDiagnosticMessages(this ITestFrameworkExecutionOptions executionOptions, bool? value)
    {
        executionOptions.SetValue(TestOptionsNames.Execution.InternalDiagnosticMessages, value);
    }

    /// <summary>
    /// Sets a flag that determines whether xUnit.net stop testing when a test fails.
    /// </summary>
    public static void SetStopOnTestFail(this ITestFrameworkExecutionOptions executionOptions, bool? value)
    {
        executionOptions.SetValue(TestOptionsNames.Execution.StopOnFail, value);
    }

    /// <summary>
    /// Sets a flag to disable parallelization.
    /// </summary>
    public static void SetDisableParallelization(this ITestFrameworkExecutionOptions executionOptions, bool? value)
    {
        executionOptions.SetValue(TestOptionsNames.Execution.DisableParallelization, value);
    }

    /// <summary>
    /// Sets the maximum number of threads to use when running tests in parallel.
    /// If set to 0 (the default value), does not limit the number of threads.
    /// </summary>
    public static void SetMaxParallelThreads(this ITestFrameworkExecutionOptions executionOptions, int? value)
    {
        executionOptions.SetValue(TestOptionsNames.Execution.MaxParallelThreads, value);
    }

    /// <summary>
    /// Sets a flag that determines whether xUnit.net should report test results synchronously.
    /// </summary>
    public static void SetSynchronousMessageReporting(this ITestFrameworkExecutionOptions executionOptions, bool? value)
    {
        executionOptions.SetValue(TestOptionsNames.Execution.SynchronousMessageReporting, value);
    }
}
