using System;
using Xunit.Internal;
using Xunit.v3;

/// <summary>
/// Extension methods for reading and writing <see cref="_ITestFrameworkDiscoveryOptions"/> and <see cref="_ITestFrameworkExecutionOptions"/>.
/// </summary>
public static class TestFrameworkOptionsReadWriteExtensions
{
	// Read/write methods for discovery options

	/// <summary>
	/// Gets a flag that determines whether diagnostic messages will be emitted.
	/// </summary>
	public static bool? GetDiagnosticMessages(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.DiagnosticMessages);
	}

	/// <summary>
	/// Gets a flag that determines whether diagnostic messages will be emitted. If the flag is not
	/// set, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetDiagnosticMessagesOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		return discoveryOptions.GetDiagnosticMessages() ?? false;
	}

	/// <summary>
	/// Gets a flag that determines whether discovered test cases should include source information.
	/// Note that not all runners have access to source information, so this flag does not guarantee
	/// that source information will be provided.
	/// </summary>
	public static bool? GetIncludeSourceInformation(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.IncludeSourceInformation);
	}

	/// <summary>
	/// Gets a flag that determines whether discovered test cases should include source information.
	/// Note that not all runners have access to source information, so this flag does not guarantee
	/// that source information will be provided. If the flag is not present, returns the default
	/// value (<c>false</c>).
	/// </summary>
	public static bool GetIncludeSourceInformationOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		return discoveryOptions.GetIncludeSourceInformation() ?? false;
	}

	/// <summary>
	/// Gets a flag that determines whether internal diagnostic messages will be emitted.
	/// </summary>
	public static bool? GetInternalDiagnosticMessages(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.InternalDiagnosticMessages);
	}

	/// <summary>
	/// Gets a flag that determines whether internal diagnostic messages will be emitted. If the flag is not
	/// set, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetInternalDiagnosticMessagesOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		return discoveryOptions.GetInternalDiagnosticMessages() ?? false;
	}

	/// <summary>
	/// Gets a flag that determines the default display name format for test methods.
	/// </summary>
	public static TestMethodDisplay? GetMethodDisplay(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		var methodDisplayString = discoveryOptions.GetValue<string>(TestOptionsNames.Discovery.MethodDisplay);
		return methodDisplayString != null ? (TestMethodDisplay?)Enum.Parse(typeof(TestMethodDisplay), methodDisplayString) : null;
	}

	/// <summary>
	/// Gets a flag that determines the default display name format for test methods. If the flag is not present,
	/// returns the default value (<see cref="TestMethodDisplay.ClassAndMethod"/>).
	/// </summary>
	public static TestMethodDisplay GetMethodDisplayOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		return discoveryOptions.GetMethodDisplay() ?? TestMethodDisplay.ClassAndMethod;
	}

	/// <summary>
	/// Gets a flag that determines the default display name format options for test methods.
	/// </summary>
	public static TestMethodDisplayOptions? GetMethodDisplayOptions(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		var methodDisplayOptionsString = discoveryOptions.GetValue<string>(TestOptionsNames.Discovery.MethodDisplayOptions);
		return methodDisplayOptionsString != null ? (TestMethodDisplayOptions?)Enum.Parse(typeof(TestMethodDisplayOptions), methodDisplayOptionsString) : null;
	}

	/// <summary>
	/// Gets a flag that determines the default display name format options for test methods. If the flag is not present,
	/// returns the default value (<see cref="TestMethodDisplayOptions.None"/>).
	/// </summary>
	public static TestMethodDisplayOptions GetMethodDisplayOptionsOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		return discoveryOptions.GetMethodDisplayOptions() ?? TestMethodDisplayOptions.None;
	}

	/// <summary>
	/// Gets a flag that determines whether theories are pre-enumerated. If they enabled, then the
	/// discovery system will return a test case for each row of test data; they are disabled, then the
	/// discovery system will return a single test case for the theory.
	/// </summary>
	public static bool? GetPreEnumerateTheories(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.PreEnumerateTheories);
	}

	/// <summary>
	/// Gets a flag that determines whether theories are pre-enumerated. If they enabled, then the
	/// discovery system will return a test case for each row of test data; they are disabled, then the
	/// discovery system will return a single test case for the theory. If the flag is not present,
	/// returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetPreEnumerateTheoriesOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		return discoveryOptions.GetPreEnumerateTheories() ?? false;
	}

	/// <summary>
	/// Gets a flag that determines whether xUnit.net should report test results synchronously.
	/// </summary>
	public static bool? GetSynchronousMessageReporting(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.SynchronousMessageReporting);
	}

	/// <summary>
	/// Gets a flag that determines whether xUnit.net should report test results synchronously.
	/// If the flag is not set, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetSynchronousMessageReportingOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		return discoveryOptions.GetSynchronousMessageReporting() ?? false;
	}

	/// <summary>
	/// Sets a flag that determines whether diagnostic messages will be emitted.
	/// </summary>
	public static void SetDiagnosticMessages(
		this _ITestFrameworkDiscoveryOptions discoveryOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.DiagnosticMessages, value);
	}

	/// <summary>
	/// Sets a flag that determines whether discovered test cases should include source information.
	/// Note that not all runners have access to source information, so this flag does not guarantee
	/// that source information will be provided.
	/// </summary>
	public static void SetIncludeSourceInformation(
		this _ITestFrameworkDiscoveryOptions discoveryOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.IncludeSourceInformation, value);
	}

	/// <summary>
	/// Sets a flag that determines whether internal diagnostic messages will be emitted.
	/// </summary>
	public static void SetInternalDiagnosticMessages(
		this _ITestFrameworkDiscoveryOptions discoveryOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.InternalDiagnosticMessages, value);
	}

	/// <summary>
	/// Sets a flag that determines the default display name format for test methods.
	/// </summary>
	public static void SetMethodDisplay(
		this _ITestFrameworkDiscoveryOptions discoveryOptions,
		TestMethodDisplay? value)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.MethodDisplay, value.HasValue ? value.GetValueOrDefault().ToString() : null);
	}

	/// <summary>
	/// Sets the flags that determine the default display options for test methods.
	/// </summary>
	public static void SetMethodDisplayOptions(
		this _ITestFrameworkDiscoveryOptions discoveryOptions,
		TestMethodDisplayOptions? value)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.MethodDisplayOptions, value.HasValue ? value.GetValueOrDefault().ToString() : null);
	}

	/// <summary>
	/// Sets a flag that determines whether theories are pre-enumerated. If they enabled, then the
	/// discovery system will return a test case for each row of test data; they are disabled, then the
	/// discovery system will return a single test case for the theory.
	/// </summary>
	public static void SetPreEnumerateTheories(
		this _ITestFrameworkDiscoveryOptions discoveryOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.PreEnumerateTheories, value);
	}

	/// <summary>
	/// Sets a flag that determines whether xUnit.net should report test results synchronously.
	/// </summary>
	public static void SetSynchronousMessageReporting(
		this _ITestFrameworkDiscoveryOptions discoveryOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.SynchronousMessageReporting, value);
	}

	// Read/write methods for execution options

	/// <summary>
	/// Gets a flag that determines whether diagnostic messages will be emitted.
	/// </summary>
	public static bool? GetDiagnosticMessages(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.DiagnosticMessages);
	}

	/// <summary>
	/// Gets a flag that determines whether internal diagnostic messages will be emitted.
	/// </summary>
	public static bool? GetInternalDiagnosticMessages(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.InternalDiagnosticMessages);
	}

	/// <summary>
	/// Gets a flag that determines whether diagnostic messages will be emitted. If the flag is not
	/// present, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetDiagnosticMessagesOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		return executionOptions.GetDiagnosticMessages() ?? false;
	}

	/// <summary>
	/// Gets a flag that determines whether internal diagnostic messages will be emitted. If the flag is not
	/// present, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetInternalDiagnosticMessagesOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		return executionOptions.GetInternalDiagnosticMessages() ?? false;
	}

	/// <summary>
	/// Gets a flag to disable parallelization.
	/// </summary>
	public static bool? GetDisableParallelization(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.DisableParallelization);
	}

	/// <summary>
	/// Gets a flag to disable parallelization. If the flag is not present, returns the
	/// default value (<c>false</c>).
	/// </summary>
	public static bool GetDisableParallelizationOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		return executionOptions.GetDisableParallelization() ?? false;
	}

	/// <summary>
	/// Gets the maximum number of threads to use when running tests in parallel.
	/// </summary>
	public static int? GetMaxParallelThreads(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		return executionOptions.GetValue<int?>(TestOptionsNames.Execution.MaxParallelThreads);
	}

	/// <summary>
	/// Gets the maximum number of threads to use when running tests in parallel. If set to 0 (or not set),
	/// the value of <see cref="Environment.ProcessorCount"/> is used; if set to a value less
	/// than 0, does not limit the number of threads.
	/// </summary>
	public static int GetMaxParallelThreadsOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		var result = executionOptions.GetMaxParallelThreads();
		if (result == null || result == 0)
			return Environment.ProcessorCount;

		return result.GetValueOrDefault();
	}

	/// <summary>
	/// Gets a flag that determines whether xUnit.net should report test results synchronously.
	/// </summary>
	public static bool? GetSynchronousMessageReporting(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.SynchronousMessageReporting);
	}

	/// <summary>
	/// Gets a flag that determines whether xUnit.net should report test results synchronously.
	/// If the flag is not set, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetSynchronousMessageReportingOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		return executionOptions.GetSynchronousMessageReporting() ?? false;
	}

	/// <summary>
	/// Sets a flag that determines whether diagnostic messages will be emitted.
	/// </summary>
	public static void SetDiagnosticMessages(
		this _ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.DiagnosticMessages, value);
	}

	/// <summary>
	/// Sets a flag that determines whether internal diagnostic messages will be emitted.
	/// </summary>
	public static void SetInternalDiagnosticMessages(
		this _ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.InternalDiagnosticMessages, value);
	}

	/// <summary>
	/// Sets a flag that determines whether xUnit.net stop testing when a test fails.
	/// </summary>
	public static void SetStopOnTestFail(
		this _ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.StopOnFail, value);
	}

	/// <summary>
	/// Sets a flag to disable parallelization.
	/// </summary>
	public static void SetDisableParallelization(
		this _ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.DisableParallelization, value);
	}

	/// <summary>
	/// Sets the maximum number of threads to use when running tests in parallel.
	/// If set to 0 (the default value), does not limit the number of threads.
	/// </summary>
	public static void SetMaxParallelThreads(
		this _ITestFrameworkExecutionOptions executionOptions,
		int? value)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.MaxParallelThreads, value);
	}

	/// <summary>
	/// Sets a flag that determines whether xUnit.net should report test results synchronously.
	/// </summary>
	public static void SetSynchronousMessageReporting(
		this _ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.SynchronousMessageReporting, value);
	}
}
