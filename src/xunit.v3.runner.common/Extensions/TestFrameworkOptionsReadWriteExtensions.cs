using System;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Extension methods for reading and writing <see cref="ITestFrameworkDiscoveryOptions"/>
/// and <see cref="ITestFrameworkExecutionOptions"/>.
/// </summary>
public static class TestFrameworkOptionsReadWriteExtensions
{
	// ======================================
	//   Read methods for discovery options
	// ======================================

	/// <summary>
	/// Gets the culture to use for discovering tests. <c>null</c> uses the default OS culture;
	/// <see cref="string.Empty"/> uses the invariant culture; any other value passes the
	/// provided value to <see cref="CultureInfo(string)"/> and uses the resulting object
	/// with <see cref="CultureInfo.DefaultThreadCurrentCulture"/> and
	/// <see cref="CultureInfo.DefaultThreadCurrentUICulture"/>.
	/// </summary>
	public static string? GetCulture(this ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		return discoveryOptions.GetValue<string?>(TestOptionsNames.Discovery.Culture);
	}

	/// <summary>
	/// Gets a flag that determines whether diagnostic messages will be emitted.
	/// </summary>
	public static bool? GetDiagnosticMessages(this ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.DiagnosticMessages);
	}

	/// <summary>
	/// Gets a flag that determines whether diagnostic messages will be emitted. If the flag is not
	/// set, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetDiagnosticMessagesOrDefault(this ITestFrameworkDiscoveryOptions discoveryOptions) =>
		GetDiagnosticMessages(discoveryOptions) ?? false;

	/// <summary>
	/// Gets a flag that determines whether discovered test cases should include source information.
	/// Note that not all runners have access to source information, so this flag does not guarantee
	/// that source information will be provided.
	/// </summary>
	public static bool? GetIncludeSourceInformation(this ITestFrameworkDiscoveryOptions discoveryOptions)
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
	public static bool GetIncludeSourceInformationOrDefault(this ITestFrameworkDiscoveryOptions discoveryOptions) =>
		GetIncludeSourceInformation(discoveryOptions) ?? false;

	/// <summary>
	/// Gets a flag that determines whether internal diagnostic messages will be emitted.
	/// </summary>
	public static bool? GetInternalDiagnosticMessages(this ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.InternalDiagnosticMessages);
	}

	/// <summary>
	/// Gets a flag that determines whether internal diagnostic messages will be emitted. If the flag is not
	/// set, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetInternalDiagnosticMessagesOrDefault(this ITestFrameworkDiscoveryOptions discoveryOptions) =>
		GetInternalDiagnosticMessages(discoveryOptions) ?? false;

	/// <summary>
	/// Gets a flag that determines the default display name format for test methods.
	/// </summary>
	public static TestMethodDisplay? GetMethodDisplay(this ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		var methodDisplayString = discoveryOptions.GetValue<string>(TestOptionsNames.Discovery.MethodDisplay);
		return methodDisplayString is not null ? (TestMethodDisplay?)Enum.Parse(typeof(TestMethodDisplay), methodDisplayString) : null;
	}

	/// <summary>
	/// Gets a flag that determines the default display name format for test methods. If the flag is not present,
	/// returns the default value (<see cref="TestMethodDisplay.ClassAndMethod"/>).
	/// </summary>
	public static TestMethodDisplay GetMethodDisplayOrDefault(this ITestFrameworkDiscoveryOptions discoveryOptions) =>
		GetMethodDisplay(discoveryOptions) ?? TestMethodDisplay.ClassAndMethod;

	/// <summary>
	/// Gets a flag that determines the default display name format options for test methods.
	/// </summary>
	public static TestMethodDisplayOptions? GetMethodDisplayOptions(this ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		var methodDisplayOptionsString = discoveryOptions.GetValue<string>(TestOptionsNames.Discovery.MethodDisplayOptions);
		return methodDisplayOptionsString is not null ? (TestMethodDisplayOptions?)Enum.Parse(typeof(TestMethodDisplayOptions), methodDisplayOptionsString) : null;
	}

	/// <summary>
	/// Gets a flag that determines the default display name format options for test methods. If the flag is not present,
	/// returns the default value (<see cref="TestMethodDisplayOptions.None"/>).
	/// </summary>
	public static TestMethodDisplayOptions GetMethodDisplayOptionsOrDefault(this ITestFrameworkDiscoveryOptions discoveryOptions) =>
		GetMethodDisplayOptions(discoveryOptions) ?? TestMethodDisplayOptions.None;

	/// <summary>
	/// Gets a flag that determines whether theories are pre-enumerated. If they enabled, then the
	/// discovery system will return a test case for each row of test data; they are disabled, then the
	/// discovery system will return a single test case for the theory.
	/// </summary>
	public static bool? GetPreEnumerateTheories(this ITestFrameworkDiscoveryOptions discoveryOptions)
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
	public static bool GetPreEnumerateTheoriesOrDefault(this ITestFrameworkDiscoveryOptions discoveryOptions) =>
		GetPreEnumerateTheories(discoveryOptions) ?? false;

	/// <summary>
	/// Gets a flag that determines whether xUnit.net should report test results synchronously.
	/// </summary>
	public static bool? GetSynchronousMessageReporting(this ITestFrameworkDiscoveryOptions discoveryOptions)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.SynchronousMessageReporting);
	}

	/// <summary>
	/// Gets a flag that determines whether xUnit.net should report test results synchronously.
	/// If the flag is not set, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetSynchronousMessageReportingOrDefault(this ITestFrameworkDiscoveryOptions discoveryOptions) =>
		GetSynchronousMessageReporting(discoveryOptions) ?? false;

	// =======================================
	//   Write methods for discovery options
	// =======================================

	/// <summary>
	/// Sets the culture to use for discovering tests. <c>null</c> uses the default OS culture;
	/// <see cref="string.Empty"/> uses the invariant culture; any other value passes the
	/// provided value to <see cref="CultureInfo(string)"/> and uses the resulting object
	/// with <see cref="CultureInfo.DefaultThreadCurrentCulture"/> and
	/// <see cref="CultureInfo.DefaultThreadCurrentUICulture"/>.
	/// </summary>
	public static void SetCulture(
		this ITestFrameworkDiscoveryOptions discoveryOptions,
		string? culture)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.Culture, culture);
	}

	/// <summary>
	/// Sets a flag that determines whether diagnostic messages will be emitted.
	/// </summary>
	public static void SetDiagnosticMessages(
		this ITestFrameworkDiscoveryOptions discoveryOptions,
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
		this ITestFrameworkDiscoveryOptions discoveryOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.IncludeSourceInformation, value);
	}

	/// <summary>
	/// Sets a flag that determines whether internal diagnostic messages will be emitted.
	/// </summary>
	public static void SetInternalDiagnosticMessages(
		this ITestFrameworkDiscoveryOptions discoveryOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.InternalDiagnosticMessages, value);
	}

	/// <summary>
	/// Sets a flag that determines the default display name format for test methods.
	/// </summary>
	public static void SetMethodDisplay(
		this ITestFrameworkDiscoveryOptions discoveryOptions,
		TestMethodDisplay? value)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.MethodDisplay, value.HasValue ? value.GetValueOrDefault().ToString() : null);
	}

	/// <summary>
	/// Sets the flags that determine the default display options for test methods.
	/// </summary>
	public static void SetMethodDisplayOptions(
		this ITestFrameworkDiscoveryOptions discoveryOptions,
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
		this ITestFrameworkDiscoveryOptions discoveryOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.PreEnumerateTheories, value);
	}

	/// <summary>
	/// Sets a flag that determines whether xUnit.net should report test results synchronously.
	/// </summary>
	public static void SetSynchronousMessageReporting(
		this ITestFrameworkDiscoveryOptions discoveryOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(discoveryOptions);

		discoveryOptions.SetValue(TestOptionsNames.Discovery.SynchronousMessageReporting, value);
	}

	// ======================================
	//   Read methods for execution options
	// ======================================

	/// <summary>
	/// Gets the culture to use for running tests. <c>null</c> uses the default OS culture;
	/// <see cref="string.Empty"/> uses the invariant culture; any other value passes the
	/// provided value to <see cref="CultureInfo(string)"/> and uses the resulting object
	/// with <see cref="CultureInfo.DefaultThreadCurrentCulture"/> and
	/// <see cref="CultureInfo.DefaultThreadCurrentUICulture"/>.
	/// </summary>
	public static string? GetCulture(this ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<string?>(TestOptionsNames.Execution.Culture);
	}

	/// <summary>
	/// Gets a flag that determines whether diagnostic messages will be emitted.
	/// </summary>
	public static bool? GetDiagnosticMessages(this ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.DiagnosticMessages);
	}

	/// <summary>
	/// Gets a flag that determines whether diagnostic messages will be emitted. If the flag is not
	/// present, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetDiagnosticMessagesOrDefault(this ITestFrameworkExecutionOptions executionOptions) =>
		GetDiagnosticMessages(executionOptions) ?? false;

	/// <summary>
	/// Gets a flag to disable parallelization.
	/// </summary>
	public static bool? GetDisableParallelization(this ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.DisableParallelization);
	}

	/// <summary>
	/// Gets a flag to disable parallelization. If the flag is not present, returns the
	/// default value (<c>false</c>).
	/// </summary>
	public static bool GetDisableParallelizationOrDefault(this ITestFrameworkExecutionOptions executionOptions) =>
		GetDisableParallelization(executionOptions) ?? false;

	/// <summary>
	/// Gets a flag that indicates how explicit tests should be handled.
	/// </summary>
	public static ExplicitOption? GetExplicitOption(this ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		var explicitText = executionOptions.GetValue<string?>(TestOptionsNames.Execution.ExplicitOption);
		return Enum.TryParse<ExplicitOption>(explicitText, out var result) ? result : null;
	}

	/// <summary>
	/// Gets a flag that indicates how explicit tests should be handled. If the flag is not present,
	/// returns the default value (<see cref="ExplicitOption.Off"/>).
	/// </summary>
	public static ExplicitOption GetExplicitOptionOrDefault(this ITestFrameworkExecutionOptions executionOptions) =>
		GetExplicitOption(executionOptions) ?? ExplicitOption.Off;

	/// <summary>
	/// Gets a flag to fail skipped tests.
	/// </summary>
	public static bool? GetFailSkips(this ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.FailSkips);
	}

	/// <summary>
	/// Gets a flag to fail skipped tests. If the flag is not present, returns the default
	/// value (<c>false</c>).
	/// </summary>
	public static bool GetFailSkipsOrDefault(this ITestFrameworkExecutionOptions executionOptions) =>
		GetFailSkips(executionOptions) ?? false;

	/// <summary>
	/// Gets a flag to fail passing tests with warnings.
	/// </summary>
	public static bool? GetFailTestsWithWarnings(this ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.FailTestsWithWarnings);
	}

	/// <summary>
	/// Gets a flag to fail passing tests with warning. If the flag is not present, returns
	/// the default value (<c>false</c>).
	/// </summary>
	public static bool GetFailTestsWithWarningsOrDefault(this ITestFrameworkExecutionOptions executionOptions) =>
		GetFailTestsWithWarnings(executionOptions) ?? false;

	/// <summary>
	/// Gets a flag that determines whether internal diagnostic messages will be emitted.
	/// </summary>
	public static bool? GetInternalDiagnosticMessages(this ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.InternalDiagnosticMessages);
	}

	/// <summary>
	/// Gets a flag that determines whether internal diagnostic messages will be emitted. If the flag is not
	/// present, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetInternalDiagnosticMessagesOrDefault(this ITestFrameworkExecutionOptions executionOptions) =>
		GetInternalDiagnosticMessages(executionOptions) ?? false;

	/// <summary>
	/// Gets the maximum number of threads to use when running tests in parallel.
	/// </summary>
	public static int? GetMaxParallelThreads(this ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<int?>(TestOptionsNames.Execution.MaxParallelThreads);
	}

	/// <summary>
	/// Gets the maximum number of threads to use when running tests in parallel. If set to 0 (or not set),
	/// the value of <see cref="Environment.ProcessorCount"/> is used; if set to a value less
	/// than 0, does not limit the number of threads.
	/// </summary>
	public static int GetMaxParallelThreadsOrDefault(this ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		var result = executionOptions.GetMaxParallelThreads();

		return
			result is null or 0
				? Environment.ProcessorCount
				: result.Value;
	}

	/// <summary>
	/// Gets the parallel algorithm to be used.
	/// </summary>
	public static ParallelAlgorithm? GetParallelAlgorithm(this ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		var parallelAlgorithmString = executionOptions.GetValue<string>(TestOptionsNames.Execution.ParallelAlgorithm);
		return parallelAlgorithmString != null ? (ParallelAlgorithm?)Enum.Parse(typeof(ParallelAlgorithm), parallelAlgorithmString) : null;
	}

	/// <summary>
	/// Gets the parallel algorithm to be used. If the flag is not present, return the default
	/// value (<see cref="ParallelAlgorithm.Conservative"/>).
	/// </summary>
	public static ParallelAlgorithm GetParallelAlgorithmOrDefault(this ITestFrameworkExecutionOptions executionOptions) =>
		GetParallelAlgorithm(executionOptions) ?? ParallelAlgorithm.Conservative;

	/// <summary>
	/// Gets the value that should be used to seed randomness.
	/// </summary>
	public static int? GetSeed(this ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<int?>(TestOptionsNames.Execution.Seed);
	}

	/// <summary>
	/// Gets a flag which indicates if the developer wishes to see output from <see cref="T:Xunit.ITestOutputHelper"/>
	/// live while it's being reported (in addition to seeing it collected together when the test is finished).
	/// </summary>
	public static bool? GetShowLiveOutput(this ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.ShowLiveOutput);
	}

	/// <summary>
	/// Gets a flag which indicates if the developer wishes to see output from <see cref="T:Xunit.ITestOutputHelper"/>
	/// live while it's being reported (in addition to seeing it collected together when the test is finished).
	/// If the flag is not present, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetShowLiveOutputOrDefault(this ITestFrameworkExecutionOptions executionOptions) =>
		GetShowLiveOutput(executionOptions) ?? false;

	/// <summary>
	/// Gets a flag that determines whether xUnit.net stop testing when a test fails.
	/// </summary>
	public static bool? GetStopOnTestFail(this ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.StopOnFail);
	}

	/// <summary>
	/// Gets a flag that determines whether xUnit.net stop testing when a test fails. If the flag
	/// is not set, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetStopOnTestFailOrDefault(this ITestFrameworkExecutionOptions executionOptions) =>
		GetStopOnTestFail(executionOptions) ?? false;

	/// <summary>
	/// Gets a flag that determines whether xUnit.net should report test results synchronously.
	/// </summary>
	public static bool? GetSynchronousMessageReporting(this ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(executionOptions);

		return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.SynchronousMessageReporting);
	}

	/// <summary>
	/// Gets a flag that determines whether xUnit.net should report test results synchronously.
	/// If the flag is not set, returns the default value (<c>false</c>).
	/// </summary>
	public static bool GetSynchronousMessageReportingOrDefault(this ITestFrameworkExecutionOptions executionOptions) =>
		GetSynchronousMessageReporting(executionOptions) ?? false;

	// =======================================
	//   Write methods for execution options
	// =======================================

	/// <summary>
	/// Sets the culture to use for running tests. <c>null</c> uses the default OS culture;
	/// <see cref="string.Empty"/> uses the invariant culture; any other value passes the
	/// provided value to <see cref="CultureInfo(string)"/> and uses the resulting object
	/// with <see cref="CultureInfo.DefaultThreadCurrentCulture"/> and
	/// <see cref="CultureInfo.DefaultThreadCurrentUICulture"/>.
	/// </summary>
	public static void SetCulture(
		this ITestFrameworkExecutionOptions executionOptions,
		string? culture)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.Culture, culture);
	}

	/// <summary>
	/// Sets a flag that determines whether diagnostic messages will be emitted.
	/// </summary>
	public static void SetDiagnosticMessages(
		this ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.DiagnosticMessages, value);
	}

	/// <summary>
	/// Sets a flag to disable parallelization.
	/// </summary>
	public static void SetDisableParallelization(
		this ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.DisableParallelization, value);
	}

	/// <summary>
	/// Sets a flag to describe how explicit tests should be handled.
	/// </summary>
	public static void SetExplicitOption(
		this ITestFrameworkExecutionOptions executionOptions,
		ExplicitOption? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.ExplicitOption, value?.ToString());
	}

	/// <summary>
	/// Sets a flag to fail skipped tests.
	/// </summary>
	public static void SetFailSkips(
		this ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.FailSkips, value);
	}

	/// <summary>
	/// Sets a flag to fail passing tests with warnings.
	/// </summary>
	public static void SetFailTestsWithWarnings(
		this ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.FailTestsWithWarnings, value);
	}

	/// <summary>
	/// Sets a flag that determines whether internal diagnostic messages will be emitted.
	/// </summary>
	public static void SetInternalDiagnosticMessages(
		this ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.InternalDiagnosticMessages, value);
	}

	/// <summary>
	/// Sets the maximum number of threads to use when running tests in parallel.
	/// If set to 0 (the default value), does not limit the number of threads.
	/// </summary>
	public static void SetMaxParallelThreads(
		this ITestFrameworkExecutionOptions executionOptions,
		int? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.MaxParallelThreads, value);
	}

	/// <summary>
	/// Sets the parallel algorithm to be used.
	/// </summary>
	public static void SetParallelAlgorithm(
		this ITestFrameworkExecutionOptions executionOptions,
		ParallelAlgorithm? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.ParallelAlgorithm, value.HasValue ? value.GetValueOrDefault().ToString() : null);
	}

	/// <summary>
	/// Sets the value that should be used to seed randomness.
	/// </summary>
	public static void SetSeed(
		this ITestFrameworkExecutionOptions executionOptions,
		int? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.Seed, value);
	}

	/// <summary>
	/// Sets a flag which indicates if the developer wishes to see output from <see cref="T:Xunit.ITestOutputHelper"/>
	/// live while it's being reported (in addition to seeing it collected together when the test is finished).
	/// </summary>
	public static void SetShowLiveOutput(
		this ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.ShowLiveOutput, value);
	}

	/// <summary>
	/// Sets a flag that determines whether xUnit.net stop testing when a test fails.
	/// </summary>
	public static void SetStopOnTestFail(
		this ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.StopOnFail, value);
	}

	/// <summary>
	/// Sets a flag that determines whether xUnit.net should report test results synchronously.
	/// </summary>
	public static void SetSynchronousMessageReporting(
		this ITestFrameworkExecutionOptions executionOptions,
		bool? value)
	{
		Guard.ArgumentNotNull(executionOptions);

		executionOptions.SetValue(TestOptionsNames.Execution.SynchronousMessageReporting, value);
	}
}
