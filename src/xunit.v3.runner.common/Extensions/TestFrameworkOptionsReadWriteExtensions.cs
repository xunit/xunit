using System;
using System.Globalization;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Extension methods for reading and writing <see cref="_ITestFrameworkDiscoveryOptions"/> and <see cref="_ITestFrameworkExecutionOptions"/>.
/// </summary>
public static class TestFrameworkOptionsReadWriteExtensions
{
	// Read/write methods for discovery options

	/// <summary>
	/// Gets the culture to use for discovering tests. <c>null</c> uses the default OS culture;
	/// <see cref="string.Empty"/> uses the invariant culture; any other value passes the
	/// provided value to <see cref="CultureInfo(string)"/> and uses the resulting object
	/// with <see cref="CultureInfo.DefaultThreadCurrentCulture"/> and
	/// <see cref="CultureInfo.DefaultThreadCurrentUICulture"/>.
	/// </summary>
	public static string? GetCulture(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		return discoveryOptions.GetValue<string?>(TestOptionsNames.Discovery.Culture);
	}

	/// <summary>
	/// Gets a flag that determines whether diagnostic messages will be emitted.
	/// </summary>
	public static bool? GetDiagnosticMessages(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.DiagnosticMessages);
	}

	/// <summary>
	/// Gets a flag that determines whether diagnostic messages will be emitted. If the flag is not
	/// set, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetDiagnosticMessagesOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		return discoveryOptions.GetDiagnosticMessages() ?? false;
	}

	/// <summary>
	/// Gets a flag that determines whether discovered test cases should include source information.
	/// Note that not all runners have access to source information, so this flag does not guarantee
	/// that source information will be provided.
	/// </summary>
	public static bool? GetIncludeSourceInformation(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

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
		Guard.ArgumentNotNull(discoveryOptions);

		return discoveryOptions.GetIncludeSourceInformation() ?? false;
	}

	/// <summary>
	/// Gets a flag that determines whether internal diagnostic messages will be emitted.
	/// </summary>
	public static bool? GetInternalDiagnosticMessages(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.InternalDiagnosticMessages);
	}

	/// <summary>
	/// Gets a flag that determines whether internal diagnostic messages will be emitted. If the flag is not
	/// set, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetInternalDiagnosticMessagesOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		return discoveryOptions.GetInternalDiagnosticMessages() ?? false;
	}

	/// <summary>
	/// Gets a flag that determines the default display name format for test methods.
	/// </summary>
	public static TestMethodDisplay? GetMethodDisplay(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		var methodDisplayString = discoveryOptions.GetValue<string>(TestOptionsNames.Discovery.MethodDisplay);
		return methodDisplayString is not null ? (TestMethodDisplay?)Enum.Parse(typeof(TestMethodDisplay), methodDisplayString) : null;
	}

	/// <summary>
	/// Gets a flag that determines the default display name format for test methods. If the flag is not present,
	/// returns the default value (<see cref="TestMethodDisplay.ClassAndMethod"/>).
	/// </summary>
	public static TestMethodDisplay GetMethodDisplayOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		return discoveryOptions.GetMethodDisplay() ?? TestMethodDisplay.ClassAndMethod;
	}

	/// <summary>
	/// Gets a flag that determines the default display name format options for test methods.
	/// </summary>
	public static TestMethodDisplayOptions? GetMethodDisplayOptions(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		var methodDisplayOptionsString = discoveryOptions.GetValue<string>(TestOptionsNames.Discovery.MethodDisplayOptions);
		return methodDisplayOptionsString is not null ? (TestMethodDisplayOptions?)Enum.Parse(typeof(TestMethodDisplayOptions), methodDisplayOptionsString) : null;
	}

	/// <summary>
	/// Gets a flag that determines the default display name format options for test methods. If the flag is not present,
	/// returns the default value (<see cref="TestMethodDisplayOptions.None"/>).
	/// </summary>
	public static TestMethodDisplayOptions GetMethodDisplayOptionsOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		return discoveryOptions.GetMethodDisplayOptions() ?? TestMethodDisplayOptions.None;
	}

	/// <summary>
	/// Gets a flag that determines whether theories are pre-enumerated. If they enabled, then the
	/// discovery system will return a test case for each row of test data; they are disabled, then the
	/// discovery system will return a single test case for the theory.
	/// </summary>
	public static bool? GetPreEnumerateTheories(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

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
		Guard.ArgumentNotNull(discoveryOptions);

		return discoveryOptions.GetPreEnumerateTheories() ?? false;
	}

	/// <summary>
	/// Gets a flag that determines whether xUnit.net should report test results synchronously.
	/// </summary>
	public static bool? GetSynchronousMessageReporting(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.SynchronousMessageReporting);
	}

	/// <summary>
	/// Gets a flag that determines whether xUnit.net should report test results synchronously.
	/// If the flag is not set, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetSynchronousMessageReportingOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		return discoveryOptions.GetSynchronousMessageReporting() ?? false;
	}

	/// <summary>
	/// Sets the culture to use for discovering tests. <c>null</c> uses the default OS culture;
	/// <see cref="string.Empty"/> uses the invariant culture; any other value passes the
	/// provided value to <see cref="CultureInfo(string)"/> and uses the resulting object
	/// with <see cref="CultureInfo.DefaultThreadCurrentCulture"/> and
	/// <see cref="CultureInfo.DefaultThreadCurrentUICulture"/>.
	/// </summary>
	public static void SetCulture(
		this _ITestFrameworkDiscoveryOptions discoveryOptions,
		string? culture)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.Culture, culture);
	}

	/// <summary>
	/// Sets a flag that determines whether diagnostic messages will be emitted.
	/// </summary>
	public static void SetDiagnosticMessages(
		this _ITestFrameworkDiscoveryOptions discoveryOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(discoveryOptions);

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
		Guard.ArgumentNotNull(discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.IncludeSourceInformation, value);
	}

	/// <summary>
	/// Sets a flag that determines whether internal diagnostic messages will be emitted.
	/// </summary>
	public static void SetInternalDiagnosticMessages(
		this _ITestFrameworkDiscoveryOptions discoveryOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.InternalDiagnosticMessages, value);
	}

	/// <summary>
	/// Sets a flag that determines the default display name format for test methods.
	/// </summary>
	public static void SetMethodDisplay(
		this _ITestFrameworkDiscoveryOptions discoveryOptions,
		TestMethodDisplay? value)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.MethodDisplay, value.HasValue ? value.GetValueOrDefault().ToString() : null);
	}

	/// <summary>
	/// Sets the flags that determine the default display options for test methods.
	/// </summary>
	public static void SetMethodDisplayOptions(
		this _ITestFrameworkDiscoveryOptions discoveryOptions,
		TestMethodDisplayOptions? value)
	{
		Guard.ArgumentNotNull(discoveryOptions);

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
		Guard.ArgumentNotNull(discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.PreEnumerateTheories, value);
	}

	/// <summary>
	/// Sets a flag that determines whether xUnit.net should report test results synchronously.
	/// </summary>
	public static void SetSynchronousMessageReporting(
		this _ITestFrameworkDiscoveryOptions discoveryOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.SynchronousMessageReporting, value);
	}

	// Read/write methods for execution options

	/// <summary>
	/// Gets the culture to use for running tests. <c>null</c> uses the default OS culture;
	/// <see cref="string.Empty"/> uses the invariant culture; any other value passes the
	/// provided value to <see cref="CultureInfo(string)"/> and uses the resulting object
	/// with <see cref="CultureInfo.DefaultThreadCurrentCulture"/> and
	/// <see cref="CultureInfo.DefaultThreadCurrentUICulture"/>.
	/// </summary>
	public static string? GetCulture(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<string?>(TestOptionsNames.Execution.Culture);
	}

	/// <summary>
	/// Gets a flag that determines whether diagnostic messages will be emitted.
	/// </summary>
	public static bool? GetDiagnosticMessages(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.DiagnosticMessages);
	}

	/// <summary>
	/// Gets a flag that indicates how explicit tests should be handled.
	/// </summary>
	public static ExplicitOption? GetExplicitOption(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		var explicitText = executionOptions.GetValue<string?>(TestOptionsNames.Execution.ExplicitOption);
		if (Enum.TryParse<ExplicitOption>(explicitText, out var result))
			return result;

		return null;
	}

	/// <summary>
	/// Gets a flag that indicates how explicit tests should be handled. If the flag is not present,
	/// returns the default value (<see cref="ExplicitOption.Off"/>).
	/// </summary>
	public static ExplicitOption GetExplicitOptionOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetExplicitOption() ?? ExplicitOption.Off;
	}

	/// <summary>
	/// Gets a flag that determines whether diagnostic messages will be emitted. If the flag is not
	/// present, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetDiagnosticMessagesOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetDiagnosticMessages() ?? false;
	}

	/// <summary>
	/// Gets a flag to disable parallelization.
	/// </summary>
	public static bool? GetDisableParallelization(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.DisableParallelization);
	}

	/// <summary>
	/// Gets a flag to disable parallelization. If the flag is not present, returns the
	/// default value (<c>false</c>).
	/// </summary>
	public static bool GetDisableParallelizationOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetDisableParallelization() ?? false;
	}

	/// <summary>
	/// Gets a flag that determines whether internal diagnostic messages will be emitted.
	/// </summary>
	public static bool? GetInternalDiagnosticMessages(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.InternalDiagnosticMessages);
	}

	/// <summary>
	/// Gets a flag that determines whether internal diagnostic messages will be emitted. If the flag is not
	/// present, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetInternalDiagnosticMessagesOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetInternalDiagnosticMessages() ?? false;
	}

	/// <summary>
	/// Gets the maximum number of threads to use when running tests in parallel.
	/// </summary>
	public static int? GetMaxParallelThreads(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<int?>(TestOptionsNames.Execution.MaxParallelThreads);
	}

	/// <summary>
	/// Gets the maximum number of threads to use when running tests in parallel. If set to 0 (or not set),
	/// the value of <see cref="Environment.ProcessorCount"/> is used; if set to a value less
	/// than 0, does not limit the number of threads.
	/// </summary>
	public static int GetMaxParallelThreadsOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		var result = executionOptions.GetMaxParallelThreads();
		if (result is null || result == 0)
			return Environment.ProcessorCount;

		return result.GetValueOrDefault();
	}

	/// <summary>
	/// Gets the value that should be used to seed randomness.
	/// </summary>
	public static int? GetSeed(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<int?>(TestOptionsNames.Execution.Seed);
	}

	/// <summary>
	/// Gets a flag that determines whether xUnit.net stop testing when a test fails.
	/// </summary>
	public static bool? GetStopOnTestFail(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.StopOnFail);
	}

	/// <summary>
	/// Gets a flag that determines whether xUnit.net stop testing when a test fails. If the flag
	/// is not set, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetStopOnTestFailOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetStopOnTestFail() ?? false;
	}

	/// <summary>
	/// Gets a flag that determines whether xUnit.net should report test results synchronously.
	/// </summary>
	public static bool? GetSynchronousMessageReporting(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.SynchronousMessageReporting);
	}

	/// <summary>
	/// Gets a flag that determines whether xUnit.net should report test results synchronously.
	/// If the flag is not set, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetSynchronousMessageReportingOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetSynchronousMessageReporting() ?? false;
	}

	/// <summary>
	/// Sets the culture to use for running tests. <c>null</c> uses the default OS culture;
	/// <see cref="string.Empty"/> uses the invariant culture; any other value passes the
	/// provided value to <see cref="CultureInfo(string)"/> and uses the resulting object
	/// with <see cref="CultureInfo.DefaultThreadCurrentCulture"/> and
	/// <see cref="CultureInfo.DefaultThreadCurrentUICulture"/>.
	/// </summary>
	public static void SetCulture(
		this _ITestFrameworkExecutionOptions executionOptions,
		string? culture)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.Culture, culture);
	}

	/// <summary>
	/// Sets a flag that determines whether diagnostic messages will be emitted.
	/// </summary>
	public static void SetDiagnosticMessages(
		this _ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.DiagnosticMessages, value);
	}

	/// <summary>
	/// Sets a flag that determines whether internal diagnostic messages will be emitted.
	/// </summary>
	public static void SetInternalDiagnosticMessages(
		this _ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.InternalDiagnosticMessages, value);
	}

	/// <summary>
	/// Sets the value that should be used to seed randomness.
	/// </summary>
	public static void SetSeed(
		this _ITestFrameworkExecutionOptions executionOptions,
		int? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.Seed, value);
	}

	/// <summary>
	/// Sets a flag that determines whether xUnit.net stop testing when a test fails.
	/// </summary>
	public static void SetStopOnTestFail(
		this _ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.StopOnFail, value);
	}

	/// <summary>
	/// Sets a flag to disable parallelization.
	/// </summary>
	public static void SetDisableParallelization(
		this _ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.DisableParallelization, value);
	}

	/// <summary>
	/// Sets a flag to describe how explicit tests should be handled.
	/// </summary>
	public static void SetExplicitOption(
		this _ITestFrameworkExecutionOptions executionOptions,
		ExplicitOption? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.ExplicitOption, value?.ToString());
	}

	/// <summary>
	/// Sets the maximum number of threads to use when running tests in parallel.
	/// If set to 0 (the default value), does not limit the number of threads.
	/// </summary>
	public static void SetMaxParallelThreads(
		this _ITestFrameworkExecutionOptions executionOptions,
		int? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.MaxParallelThreads, value);
	}

	/// <summary>
	/// Sets a flag that determines whether xUnit.net should report test results synchronously.
	/// </summary>
	public static void SetSynchronousMessageReporting(
		this _ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.SynchronousMessageReporting, value);
	}
}
