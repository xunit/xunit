using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runner.v3;

/// <summary>
/// This class is responsible for generating arguments used for calling xUnit.net v3
/// command line arguments.
/// </summary>
public static class Xunit3ArgumentFactory
{
	static readonly Version Version_0_3_0 = new(0, 3, 0);

	/// <summary>
	/// Gets command line switches based on a call to <see cref="Xunit3.Find"/>.
	/// </summary>
	public static List<string> ForFind(
		Version coreFrameworkVersion,
		ITestFrameworkDiscoveryOptions options,
		XunitFilters? filters = null,
		string? configFileName = null,
		ListOption? listOption = null,
		bool waitForDebugger = false)
	{
		Guard.ArgumentNotNull(options);

		return ToArguments(
			coreFrameworkVersion,
			configFileName,
			options.GetCulture(),
			options.GetDiagnosticMessages(),
			disableParallelization: null,
			explicitOption: null,
			failSkips: null,
			failTestsWithWarnings: null,
			filters,
			options.GetInternalDiagnosticMessages(),
			listOption,
			maxParallelThreads: null,
			options.GetMethodDisplay(),
			options.GetMethodDisplayOptions(),
			parallelAlgorithm: null,
			options.GetPreEnumerateTheories(),
			seed: null,
			serializedTestCases: null,
			stopOnTestFail: null,
			options.GetSynchronousMessageReporting(),
			waitForDebugger
		);
	}

	/// <summary>
	/// Gets command line switches based on a call to <see cref="Xunit3.FindAndRun"/>.
	/// </summary>
	public static List<string> ForFindAndRun(
		Version coreFrameworkVersion,
		ITestFrameworkDiscoveryOptions discoveryOptions,
		ITestFrameworkExecutionOptions executionOptions,
		XunitFilters? filters = null,
		string? configFileName = null,
		bool waitForDebugger = false)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(executionOptions);

		return ToArguments(
			coreFrameworkVersion,
			configFileName,
			executionOptions.GetCulture() ?? discoveryOptions.GetCulture(),
			executionOptions.GetDiagnosticMessages() ?? discoveryOptions.GetDiagnosticMessages(),
			executionOptions.GetDisableParallelization(),
			executionOptions.GetExplicitOption(),
			executionOptions.GetFailSkips(),
			executionOptions.GetFailTestsWithWarnings(),
			filters,
			executionOptions.GetInternalDiagnosticMessages() ?? discoveryOptions.GetInternalDiagnosticMessages(),
			listOption: null,
			executionOptions.GetMaxParallelThreads(),
			discoveryOptions.GetMethodDisplay(),
			discoveryOptions.GetMethodDisplayOptions(),
			executionOptions.GetParallelAlgorithm(),
			discoveryOptions.GetPreEnumerateTheories(),
			executionOptions.GetSeed(),
			serializedTestCases: null,
			executionOptions.GetStopOnTestFail(),
			executionOptions.GetSynchronousMessageReporting() ?? discoveryOptions.GetSynchronousMessageReporting(),
			waitForDebugger
		);
	}


	/// <summary>
	/// Gets command line switches based on a call to <see cref="Xunit3.Run"/>.
	/// </summary>
	public static List<string> ForRun(
		Version coreFrameworkVersion,
		ITestFrameworkExecutionOptions options,
		IReadOnlyCollection<string> serializedTestCases,
		string? configFileName = null,
		bool waitForDebugger = false)
	{
		Guard.ArgumentNotNull(options);
		Guard.ArgumentNotNullOrEmpty(serializedTestCases);

		return ToArguments(
			coreFrameworkVersion,
			configFileName,
			options.GetCulture(),
			options.GetDiagnosticMessages(),
			options.GetDisableParallelization(),
			options.GetExplicitOption(),
			options.GetFailSkips(),
			options.GetFailTestsWithWarnings(),
			filters: null,
			options.GetInternalDiagnosticMessages(),
			listOption: null,
			options.GetMaxParallelThreads(),
			methodDisplay: null,
			methodDisplayOptions: null,
			options.GetParallelAlgorithm(),
			preEnumerateTheories: null,
			options.GetSeed(),
			serializedTestCases,
			options.GetStopOnTestFail(),
			options.GetSynchronousMessageReporting(),
			waitForDebugger
		);
	}

	static List<string> ToArguments(
		Version coreFrameworkVersion,
		string? configFileName,
		string? culture,
		bool? diagnosicMessages,
		bool? disableParallelization,
		ExplicitOption? explicitOption,
		bool? failSkips,
		bool? failTestsWithWarnings,
		XunitFilters? filters,
		bool? internalDiagnosticMessages,
		ListOption? listOption,
		int? maxParallelThreads,
		TestMethodDisplay? methodDisplay,
		TestMethodDisplayOptions? methodDisplayOptions,
		ParallelAlgorithm? parallelAlgorithm,
		bool? preEnumerateTheories,
		int? seed,
		IReadOnlyCollection<string>? serializedTestCases,
		bool? stopOnTestFail,
		bool? synchronousMessages,
		bool waitForDebugger)
	{
		var result = new List<string>();

		// POSITIONAL VALUES

		if (seed is not null)
			result.AddRange([string.Format(CultureInfo.InvariantCulture, ":{0}", seed)]);

		if (configFileName is not null)
			result.Add(configFileName);

		// SWITCH VALUES

		result.Add("-automated");

		if (coreFrameworkVersion >= Version_0_3_0 && synchronousMessages == true)
			result.Add("sync");

		result.AddRange(culture switch
		{
			null => [],
			"" => ["-culture", "invariant"],
			_ => ["-culture", culture],
		});

		if (diagnosicMessages == true)
			result.Add("-diagnostics");

		if (explicitOption.HasValue)
			result.AddRange(["-explicit", explicitOption.Value.ToString()]);

		result.AddRange(failSkips switch
		{
			true => ["-failSkips"],
			false => ["-failSkips-"],
			_ => [],
		});

		result.AddRange(failTestsWithWarnings switch
		{
			true => ["-failWarns"],
			false => ["-failWarns-"],
			_ => [],
		});

		if (internalDiagnosticMessages == true)
			result.Add("-internalDiagnostics");

		if (listOption.HasValue)
			result.AddRange(["-list", listOption.Value.ToString()]);

		if (maxParallelThreads.HasValue)
			result.AddRange(["-maxThreads", maxParallelThreads.Value.ToString(CultureInfo.InvariantCulture)]);

		if (methodDisplay.HasValue)
			result.AddRange(["-methodDisplay", methodDisplay.Value.ToString()]);

		if (methodDisplayOptions.HasValue)
#if NETFRAMEWORK
			result.AddRange(["-methodDisplayOptions", methodDisplayOptions.Value.ToString().Replace(" ", "")]);
#else
			result.AddRange(["-methodDisplayOptions", methodDisplayOptions.Value.ToString().Replace(" ", "", StringComparison.Ordinal)]);
#endif

		result.AddRange(disableParallelization switch
		{
			true => ["-parallel", "none"],
			false => ["-parallel", "collections"],
			_ => [],
		});

		if (parallelAlgorithm.HasValue)
			result.AddRange(["-parallelAlgorithm", parallelAlgorithm.Value.ToString()]);

		if (preEnumerateTheories == true)
			result.Add("-preEnumerateTheories");

		if (serializedTestCases?.Count > 0)
			foreach (var testCase in serializedTestCases)
				result.AddRange(["-run", testCase]);

		if (stopOnTestFail == true)
			result.Add("-stopOnFail");

		if (waitForDebugger)
			result.Add("-waitForDebugger");

		// FILTERS

		if (filters is not null)
			result.AddRange(filters.ToXunit3Arguments());

		return result;
	}
}
