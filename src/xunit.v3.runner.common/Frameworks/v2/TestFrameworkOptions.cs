using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Represents options passed to a test framework for discovery or execution.
	/// </summary>
	[DebuggerDisplay("{ToDebuggerDisplay(),nq}")]
#if NETFRAMEWORK
	[System.Serializable]
#endif
	public class TestFrameworkOptions : ITestFrameworkDiscoveryOptions, ITestFrameworkExecutionOptions
	{
		readonly Dictionary<string, object> properties = new Dictionary<string, object>();

		// Force users to use one of the factory methods
		TestFrameworkOptions()
		{ }

		/// <summary>
		/// Creates an instance of <see cref="TestFrameworkOptions"/>
		/// </summary>
		/// <param name="configuration">The optional configuration to copy values from.</param>
		public static ITestFrameworkDiscoveryOptions ForDiscovery(TestAssemblyConfiguration? configuration = null)
		{
			ITestFrameworkDiscoveryOptions result = new TestFrameworkOptions();

			if (configuration != null)
			{
				result.SetDiagnosticMessages(configuration.DiagnosticMessages);
				result.SetInternalDiagnosticMessages(configuration.InternalDiagnosticMessages);
				result.SetMethodDisplay(configuration.MethodDisplay);
				result.SetMethodDisplayOptions(configuration.MethodDisplayOptions);
				result.SetPreEnumerateTheories(configuration.PreEnumerateTheories);
			}

			return result;
		}

		/// <summary>
		/// Creates an instance of <see cref="TestFrameworkOptions"/>
		/// </summary>
		/// <param name="configuration">The optional configuration to copy values from.</param>
		public static ITestFrameworkExecutionOptions ForExecution(TestAssemblyConfiguration? configuration = null)
		{
			ITestFrameworkExecutionOptions result = new TestFrameworkOptions();

			if (configuration != null)
			{
				result.SetDiagnosticMessages(configuration.DiagnosticMessages);
				result.SetInternalDiagnosticMessages(configuration.InternalDiagnosticMessages);
				result.SetDisableParallelization(!configuration.ParallelizeTestCollections);
				result.SetMaxParallelThreads(configuration.MaxParallelThreads);
			}

			return result;
		}

		/// <summary>
		/// Gets a value from the options collection.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="name">The name of the value.</param>
		/// <returns>Returns the value.</returns>
		public TValue? GetValue<TValue>(string name)
			where TValue : class
		{
			Guard.ArgumentNotNullOrEmpty(nameof(name), name);

			if (properties.TryGetValue(name, out var result))
				return (TValue)result;

			return default;
		}

#nullable disable  // The original signature is not compatibility with nullable reference type support
		TValue ITestFrameworkOptions.GetValue<TValue>(string name)
		{
			Guard.ArgumentNotNullOrEmpty(nameof(name), name);

			if (properties.TryGetValue(name, out var result))
				return (TValue)result;

			return default;
		}
#nullable enable

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
				properties[name] = value;
		}

		string ToDebuggerDisplay()
			=> $"{{ {string.Join(", ", properties.Select(p => string.Format("{{ {0} = {1} }}", new object[] { p.Key, ToDebuggerDisplay(p.Value) })).ToArray())} }}";

		string ToDebuggerDisplay(object value)
		{
			if (value == null)
				return "null";

			if (value is string stringValue)
				return $"\"{stringValue}\"";

			return value.ToString()!;
		}

		/// <summary>
		/// TEMPORARY METHOD. DO NOT USE.
		/// </summary>
		public static ITestFrameworkDiscoveryOptions Wrap(_ITestFrameworkDiscoveryOptions options) =>
			new OptionsWrapper(options);

		/// <summary>
		/// TEMPORARY METHOD. DO NOT USE.
		/// </summary>
		public static ITestFrameworkExecutionOptions Wrap(_ITestFrameworkExecutionOptions options) =>
			new OptionsWrapper(options);

		class OptionsWrapper : ITestFrameworkDiscoveryOptions, ITestFrameworkExecutionOptions
		{
			readonly _ITestFrameworkOptions innerOptions;

			public OptionsWrapper(_ITestFrameworkOptions innerOptions)
			{
				this.innerOptions = innerOptions;
			}

			public TValue GetValue<TValue>(string name)
				=> innerOptions.GetValue<TValue>(name);

			public void SetValue<TValue>(string name, TValue value)
				=> innerOptions.SetValue<TValue>(name, value);
		}
	}
}
