using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents options passed to a test framework for discovery or execution.
/// </summary>
[DebuggerDisplay("{ToDebuggerDisplay(),nq}")]
public class _TestFrameworkOptions : _ITestFrameworkDiscoveryOptions, _ITestFrameworkExecutionOptions
{
	readonly Dictionary<string, string> properties = new();

	// Force users to use one of the factory methods
	_TestFrameworkOptions(string? optionsJson = null)
	{
		if (optionsJson is not null)
			properties = JsonSerializer.Deserialize<Dictionary<string, string>>(optionsJson) ?? throw new ArgumentException("Invalid JSON", nameof(optionsJson));
	}

	/// <summary>
	/// Creates an instance of <see cref="_TestFrameworkOptions"/> for discovery purposes.
	/// </summary>
	/// <param name="culture">Optional value to indicate the culture used for test discovery</param>
	/// <param name="diagnosticMessages">Optional flag to enable diagnostic messages</param>
	/// <param name="includeSourceInformation">Optional flag to include source information</param>
	/// <param name="internalDiagnosticMessages">Optional flag to enable internal diagnostic messages</param>
	/// <param name="methodDisplay">Optional flags for creating the display name of test methods</param>
	/// <param name="methodDisplayOptions">Optional flags for formatting the display name of test methods</param>
	/// <param name="preEnumerateTheories">Optional flag to enable pre-enumerating theories</param>
	public static _ITestFrameworkDiscoveryOptions ForDiscovery(
		string? culture = null,
		bool? diagnosticMessages = null,
		bool? includeSourceInformation = null,
		bool? internalDiagnosticMessages = null,
		TestMethodDisplay? methodDisplay = null,
		TestMethodDisplayOptions? methodDisplayOptions = null,
		bool? preEnumerateTheories = null)
	{
		_ITestFrameworkDiscoveryOptions result = new _TestFrameworkOptions();

		result.SetCulture(culture);
		result.SetDiagnosticMessages(diagnosticMessages);
		result.SetIncludeSourceInformation(includeSourceInformation);
		result.SetInternalDiagnosticMessages(internalDiagnosticMessages);
		result.SetMethodDisplay(methodDisplay);
		result.SetMethodDisplayOptions(methodDisplayOptions);
		result.SetPreEnumerateTheories(preEnumerateTheories);

		return result;
	}

	/// <summary>
	/// Creates an instance of <see cref="_TestFrameworkOptions"/> for discovery purposes.
	/// </summary>
	/// <param name="configuration">The configuration to copy values from.</param>
	public static _ITestFrameworkDiscoveryOptions ForDiscovery(TestAssemblyConfiguration configuration)
	{
		Guard.ArgumentNotNull(configuration);

		return ForDiscovery(
			configuration.Culture,
			configuration.DiagnosticMessages,
			configuration.IncludeSourceInformation,
			configuration.InternalDiagnosticMessages,
			configuration.MethodDisplay,
			configuration.MethodDisplayOptions,
			configuration.PreEnumerateTheories
		);
	}

	/// <summary>
	/// Creates an instance of <see cref="_TestFrameworkOptions"/> for discovery purposes.
	/// </summary>
	/// <param name="optionsJson">The serialized discovery options.</param>
	public static _ITestFrameworkDiscoveryOptions ForDiscoveryFromSerialization(string optionsJson) =>
		new _TestFrameworkOptions(optionsJson);

	/// <summary>
	/// Creates an instance of <see cref="_TestFrameworkOptions"/> for execution purposes.
	/// </summary>
	/// <param name="culture">Optional value to indicate the culture used for test execution</param>
	/// <param name="diagnosticMessages">Optional flag to enable diagnostic messages</param>
	/// <param name="disableParallelization">Optional flag to disable test parallelization</param>
	/// <param name="explicitOption">Optional flag to indicate how explicit tests should be handled</param>
	/// <param name="internalDiagnosticMessages">Optional flag to enable internal diagnostic messages</param>
	/// <param name="maxParallelThreads">Optional value for maximum threads when running tests in parallel</param>
	/// <param name="seed">Optional override value to seed randomization</param>
	/// <param name="stopOnFail">Optional flag to indicate that tests should stop running once one test has failed</param>
	/// <returns></returns>
	public static _ITestFrameworkExecutionOptions ForExecution(
		string? culture = null,
		bool? diagnosticMessages = null,
		bool? disableParallelization = null,
		ExplicitOption? explicitOption = null,
		bool? internalDiagnosticMessages = null,
		int? maxParallelThreads = null,
		int? seed = null,
		bool? stopOnFail = null)
	{
		_ITestFrameworkExecutionOptions result = new _TestFrameworkOptions();

		result.SetCulture(culture);
		result.SetDiagnosticMessages(diagnosticMessages);
		result.SetDisableParallelization(disableParallelization);
		result.SetExplicitOption(explicitOption);
		result.SetInternalDiagnosticMessages(internalDiagnosticMessages);
		result.SetMaxParallelThreads(maxParallelThreads);
		result.SetSeed(seed);
		result.SetStopOnTestFail(stopOnFail);

		return result;
	}

	/// <summary>
	/// Creates an instance of <see cref="_TestFrameworkOptions"/> for execution purposes.
	/// </summary>
	/// <param name="configuration">The configuration to copy values from.</param>
	public static _ITestFrameworkExecutionOptions ForExecution(TestAssemblyConfiguration configuration)
	{
		Guard.ArgumentNotNull(configuration);

		return ForExecution(
			configuration.Culture,
			configuration.DiagnosticMessages,
			!configuration.ParallelizeTestCollections,
			configuration.ExplicitOption,
			configuration.InternalDiagnosticMessages,
			configuration.MaxParallelThreads,
			configuration.Seed,
			configuration.StopOnFail
		);
	}

	/// <summary>
	/// Creates an instance of <see cref="_TestFrameworkOptions"/> for execution purposes.
	/// </summary>
	/// <param name="optionsJson">The serialized execution options.</param>
	public static _ITestFrameworkExecutionOptions ForExecutionFromSerialization(string optionsJson) =>
		new _TestFrameworkOptions(optionsJson);

	/// <summary>
	/// Gets a value from the options collection.
	/// </summary>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <param name="name">The name of the value.</param>
	/// <returns>Returns the value.</returns>
	public TValue? GetValue<TValue>(string name)
	{
		Guard.ArgumentNotNullOrEmpty(name);

		if (properties.TryGetValue(name, out var result))
		{
			if (result is null)
				return default;

			if (typeof(TValue) == typeof(string))
				return (TValue)(object)result;

			var targetType = typeof(TValue).UnwrapNullable();
			return (TValue)Convert.ChangeType(result, targetType, CultureInfo.InvariantCulture);
		}

		return default;
	}

	/// <summary>
	/// Sets a value into the options collection.
	/// </summary>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <param name="name">The name of the value.</param>
	/// <param name="value">The value.</param>
	public void SetValue<TValue>(
		string name,
		TValue value)
	{
		if (value is null)
			properties.Remove(name);
		else
		{
			if (typeof(TValue) == typeof(string))
				properties[name] = (string)(object)value;
			else
				properties[name] = (string)Convert.ChangeType(value, typeof(string), CultureInfo.InvariantCulture);
		}
	}

	string ToDebuggerDisplay()
		=> string.Format(CultureInfo.CurrentCulture, "{{ {0} }}", string.Join(", ", properties.Select(p => string.Format(CultureInfo.CurrentCulture, "{{ {0} = {1} }}", p.Key, p.Value)).ToArray()));

	/// <inheritdoc/>
	public string ToJson() =>
		JsonSerializer.Serialize(properties);
}
