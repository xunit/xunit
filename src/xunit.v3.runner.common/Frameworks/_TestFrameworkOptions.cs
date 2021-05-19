using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Represents options passed to a test framework for discovery or execution.
	/// </summary>
	[DebuggerDisplay("{ToDebuggerDisplay(),nq}")]
	[Serializable]
	public class _TestFrameworkOptions : _ITestFrameworkDiscoveryOptions, _ITestFrameworkExecutionOptions
	{
		readonly Dictionary<string, string> properties = new Dictionary<string, string>();

		// Force users to use one of the factory methods
		_TestFrameworkOptions(string? optionsJson = null)
		{
			if (optionsJson != null)
				properties = JsonSerializer.Deserialize<Dictionary<string, string>>(optionsJson) ?? throw new ArgumentException("Invalid JSON", nameof(optionsJson));
		}

		/// <summary>
		/// Creates an instance of <see cref="_TestFrameworkOptions"/> for discovery purposes.
		/// </summary>
		/// <param name="configuration">The optional configuration to copy values from.</param>
		public static _ITestFrameworkDiscoveryOptions ForDiscovery(TestAssemblyConfiguration? configuration = null)
		{
			_ITestFrameworkDiscoveryOptions result = new _TestFrameworkOptions();

			if (configuration != null)
			{
				result.SetDiagnosticMessages(configuration.DiagnosticMessages);
				result.SetIncludeSourceInformation(configuration.IncludeSourceInformation);
				result.SetInternalDiagnosticMessages(configuration.InternalDiagnosticMessages);
				result.SetMethodDisplay(configuration.MethodDisplay);
				result.SetMethodDisplayOptions(configuration.MethodDisplayOptions);
				result.SetPreEnumerateTheories(configuration.PreEnumerateTheories);
			}

			return result;
		}

		/// <summary>
		/// Creates an instance of <see cref="_TestFrameworkOptions"/> for discovery purposes.
		/// </summary>
		/// <param name="optionsJson">The serialized discovery options.</param>
		public static _ITestFrameworkDiscoveryOptions ForDiscovery(string optionsJson) =>
			new _TestFrameworkOptions(optionsJson);

		/// <summary>
		/// Creates an instance of <see cref="_TestFrameworkOptions"/> for execution purposes.
		/// </summary>
		/// <param name="configuration">The optional configuration to copy values from.</param>
		public static _ITestFrameworkExecutionOptions ForExecution(TestAssemblyConfiguration? configuration = null)
		{
			_ITestFrameworkExecutionOptions result = new _TestFrameworkOptions();

			if (configuration != null)
			{
				result.SetDiagnosticMessages(configuration.DiagnosticMessages);
				result.SetDisableParallelization(!configuration.ParallelizeTestCollections);
				result.SetInternalDiagnosticMessages(configuration.InternalDiagnosticMessages);
				result.SetMaxParallelThreads(configuration.MaxParallelThreads);
				result.SetStopOnTestFail(configuration.StopOnFail);
			}

			return result;
		}

		/// <summary>
		/// Creates an instance of <see cref="_TestFrameworkOptions"/> for execution purposes.
		/// </summary>
		/// <param name="optionsJson">The serialized execution options.</param>
		public static _ITestFrameworkExecutionOptions ForExecution(string optionsJson) =>
			new _TestFrameworkOptions(optionsJson);

		/// <summary>
		/// Gets a value from the options collection.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="name">The name of the value.</param>
		/// <returns>Returns the value.</returns>
		public TValue? GetValue<TValue>(string name)
		{
			Guard.ArgumentNotNullOrEmpty(nameof(name), name);

			if (properties.TryGetValue(name, out var result))
			{
				if (result == null)
					return default;

				if (typeof(TValue) == typeof(string))
					return (TValue)(object)result;

				var targetType = typeof(TValue).UnwrapNullable();
				return (TValue)Convert.ChangeType(result, targetType);
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
			if (value == null)
				properties.Remove(name);
			else
			{
				if (typeof(TValue) == typeof(string))
					properties[name] = (string)(object)value;
				else
					properties[name] = (string)Convert.ChangeType(value, typeof(string));
			}
		}

		string ToDebuggerDisplay()
			=> $"{{ {string.Join(", ", properties.Select(p => $"{{ {p.Key} = {p.Value} }}").ToArray())} }}";

		/// <inheritdoc/>
		public string ToJson() =>
			JsonSerializer.Serialize(properties);
	}
}
