using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// This class is used to read JSON-based configuration information for a test assembly.
/// </summary>
public static class ConfigReader_Json
{
	/// <summary>
	/// Loads the test assembly configuration for the given test assembly.
	/// </summary>
	/// <param name="configuration">The configuration object to write the values to.</param>
	/// <param name="assemblyFileName">The test assembly.</param>
	/// <param name="configFileName">The test assembly configuration file.</param>
	/// <param name="warnings">A container to receive loading warnings, if desired.</param>
	/// <returns>A flag which indicates whether configuration values were read.</returns>
	public static bool Load(
		TestAssemblyConfiguration configuration,
		string? assemblyFileName,
		string? configFileName = null,
		List<string>? warnings = null)
	{
		Guard.ArgumentNotNull(configuration);

		// If they provide a configuration file, we only read that, success or failure
		if (configFileName is not null)
		{
			if (!configFileName.EndsWith(".json", StringComparison.Ordinal))
				return false;

			if (!File.Exists(configFileName))
			{
				warnings?.Add(string.Format(CultureInfo.CurrentCulture, "Couldn't load config file '{0}': file not found", configFileName));
				return false;
			}

			return LoadFile(configuration, configFileName, warnings);
		}

		// If there's no assembly file, then we can't find co-located xunit.runner.json files
		if (string.IsNullOrWhiteSpace(assemblyFileName))
			return false;

		var assemblyName = Path.GetFileNameWithoutExtension(assemblyFileName);
		var directoryName = Path.GetDirectoryName(assemblyFileName)!;

		// {assembly}.xunit.runner.json takes priority over xunit.runner.json
		return
			LoadFile(configuration, Path.Combine(directoryName, string.Format(CultureInfo.CurrentCulture, "{0}.xunit.runner.json", assemblyName)), warnings) ||
			LoadFile(configuration, Path.Combine(directoryName, "xunit.runner.json"), warnings);
	}

	static bool LoadFile(
		TestAssemblyConfiguration configuration,
		string configFileName,
		List<string>? warnings)
	{
		try
		{
			if (!File.Exists(configFileName))
				return false;

			var jsonText = File.ReadAllText(configFileName);
			return LoadFromJson(configuration, jsonText, string.Format(CultureInfo.CurrentCulture, "config file '{0}'", configFileName), warnings);
		}
		catch (Exception ex)
		{
			warnings?.Add(string.Format(CultureInfo.CurrentCulture, "Exception loading config file '{0}': {1}", configFileName, ex.Message));
			return false;
		}
	}

	/// <summary>
	/// Loads the test assembly configuration for the given test assembly.
	/// </summary>
	/// <param name="configuration">The configuration object to write the values to.</param>
	/// <param name="jsonText">The configuration JSON, as a string</param>
	/// <param name="warnings">A container to receive loading warnings, if desired.</param>
	/// <returns>A flag which indicates whether configuration values were read.</returns>
	/// <returns>A flag which indicates whether configuration values were read.</returns>
	public static bool LoadFromJson(
		TestAssemblyConfiguration configuration,
		string jsonText,
		List<string>? warnings = null) =>
			LoadFromJson(configuration, jsonText, "JSON text", warnings);

	static bool LoadFromJson(
		TestAssemblyConfiguration configuration,
		string jsonText,
		string sourceName,
		List<string>? warnings)
	{
		Guard.ArgumentNotNull(configuration);
		Guard.ArgumentNotNull(jsonText);

		try
		{
			if (!JsonDeserializer.TryDeserialize(jsonText, out var json))
			{
				warnings?.Add(string.Format(CultureInfo.CurrentCulture, "Couldn't parse {0}: the JSON appears to be malformed", sourceName));
				return false;
			}

			if (json is not IReadOnlyDictionary<string, object> root)
			{
				warnings?.Add(string.Format(CultureInfo.CurrentCulture, "Couldn't parse {0}: the root must be a JSON object", sourceName));
				return false;
			}

			foreach (var kvp in root)
			{
				if (kvp.Value is bool booleanValue)
				{
					if (string.Equals(kvp.Key, Configuration.DiagnosticMessages, StringComparison.OrdinalIgnoreCase))
						configuration.DiagnosticMessages = booleanValue;
					else if (string.Equals(kvp.Key, Configuration.FailSkips, StringComparison.OrdinalIgnoreCase))
						configuration.FailSkips = booleanValue;
					else if (string.Equals(kvp.Key, Configuration.FailWarns, StringComparison.OrdinalIgnoreCase))
						configuration.FailTestsWithWarnings = booleanValue;
					else if (string.Equals(kvp.Key, Configuration.InternalDiagnosticMessages, StringComparison.OrdinalIgnoreCase))
						configuration.InternalDiagnosticMessages = booleanValue;
					else if (string.Equals(kvp.Key, Configuration.ParallelizeAssembly, StringComparison.OrdinalIgnoreCase))
						configuration.ParallelizeAssembly = booleanValue;
					else if (string.Equals(kvp.Key, Configuration.ParallelizeTestCollections, StringComparison.OrdinalIgnoreCase))
						configuration.ParallelizeTestCollections = booleanValue;
					else if (string.Equals(kvp.Key, Configuration.PreEnumerateTheories, StringComparison.OrdinalIgnoreCase))
						configuration.PreEnumerateTheories = booleanValue;
					else if (string.Equals(kvp.Key, Configuration.ShadowCopy, StringComparison.OrdinalIgnoreCase))
						configuration.ShadowCopy = booleanValue;
					else if (string.Equals(kvp.Key, Configuration.ShowLiveOutput, StringComparison.OrdinalIgnoreCase))
						configuration.ShowLiveOutput = booleanValue;
					else if (string.Equals(kvp.Key, Configuration.StopOnFail, StringComparison.OrdinalIgnoreCase))
						configuration.StopOnFail = booleanValue;
					else if (string.Equals(kvp.Key, Configuration.SynchronousReporting, StringComparison.OrdinalIgnoreCase))
						configuration.SynchronousMessageReporting = booleanValue;
				}
				else if (kvp.Value is decimal decimalValue && (decimalValue % 1m) == 0m)
				{
					var intValue = (int)decimalValue;

					if (string.Equals(kvp.Key, Configuration.AssertEquivalentMaxDepth, StringComparison.OrdinalIgnoreCase))
					{
						if (intValue >= 1)
							configuration.AssertEquivalentMaxDepth = intValue;
					}
					if (string.Equals(kvp.Key, Configuration.MaxParallelThreads, StringComparison.OrdinalIgnoreCase))
					{
						if (intValue >= -1)
							configuration.MaxParallelThreads = intValue;
					}
					else if (string.Equals(kvp.Key, Configuration.LongRunningTestSeconds, StringComparison.OrdinalIgnoreCase))
					{
						if (intValue > 0)
							configuration.LongRunningTestSeconds = intValue;
					}
					if (string.Equals(kvp.Key, Configuration.PrintMaxEnumerableLength, StringComparison.OrdinalIgnoreCase))
					{
						if (intValue >= 0)
							configuration.PrintMaxEnumerableLength = intValue;
					}
					if (string.Equals(kvp.Key, Configuration.PrintMaxObjectDepth, StringComparison.OrdinalIgnoreCase))
					{
						if (intValue >= 0)
							configuration.PrintMaxObjectDepth = intValue;
					}
					if (string.Equals(kvp.Key, Configuration.PrintMaxObjectMemberCount, StringComparison.OrdinalIgnoreCase))
					{
						if (intValue >= 0)
							configuration.PrintMaxObjectMemberCount = intValue;
					}
					if (string.Equals(kvp.Key, Configuration.PrintMaxStringLength, StringComparison.OrdinalIgnoreCase))
					{
						if (intValue >= 0)
							configuration.PrintMaxStringLength = intValue;
					}
					else if (string.Equals(kvp.Key, Configuration.Seed, StringComparison.OrdinalIgnoreCase))
					{
						if (intValue >= 0)
							configuration.Seed = intValue;
					}
				}
				else if (kvp.Value is string stringValue)
				{
					if (string.Equals(kvp.Key, Configuration.MaxParallelThreads, StringComparison.OrdinalIgnoreCase))
					{
						if (string.Equals("default", stringValue, StringComparison.OrdinalIgnoreCase))
							configuration.MaxParallelThreads = null;
						else if (string.Equals("unlimited", stringValue, StringComparison.OrdinalIgnoreCase))
							configuration.MaxParallelThreads = -1;
						else
						{
							var match = ConfigUtility.MultiplierStyleMaxParallelThreadsRegex.Match(stringValue);
							// Use invariant format and convert ',' to '.' so we can always support both formats, regardless of locale
							// If we stick to locale-only parsing, we could break people when moving from one locale to another (for example,
							// from people running tests on their desktop in a comma locale vs. running them in CI with a decimal locale).
							if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var maxThreadMultiplier))
								configuration.MaxParallelThreads = (int)(maxThreadMultiplier * Environment.ProcessorCount);
						}
					}
					else if (string.Equals(kvp.Key, Configuration.MethodDisplay, StringComparison.OrdinalIgnoreCase))
					{
						if (Enum.TryParse<TestMethodDisplay>(stringValue, true, out var methodDisplay))
							configuration.MethodDisplay = methodDisplay;
					}
					else if (string.Equals(kvp.Key, Configuration.MethodDisplayOptions, StringComparison.OrdinalIgnoreCase))
					{
						if (Enum.TryParse<TestMethodDisplayOptions>(stringValue, true, out var methodDisplayOptions))
							configuration.MethodDisplayOptions = methodDisplayOptions;
					}
					else if (string.Equals(kvp.Key, Configuration.AppDomain, StringComparison.OrdinalIgnoreCase))
					{
						if (Enum.TryParse<AppDomainSupport>(stringValue, true, out var appDomain))
							configuration.AppDomain = appDomain;
					}
					else if (string.Equals(kvp.Key, Configuration.Culture, StringComparison.OrdinalIgnoreCase))
					{
						configuration.Culture =
							string.Equals("default", stringValue, StringComparison.OrdinalIgnoreCase)
								? null
								: string.Equals("invariant", stringValue, StringComparison.OrdinalIgnoreCase)
									? string.Empty
									: stringValue;
					}
					else if (string.Equals(kvp.Key, Configuration.ParallelAlgorithm, StringComparison.OrdinalIgnoreCase))
					{
						if (Enum.TryParse<ParallelAlgorithm>(stringValue, true, out var parallelAlgorithm))
							configuration.ParallelAlgorithm = parallelAlgorithm;
					}
				}
			}

			return true;
		}
		catch (Exception ex)
		{
			warnings?.Add(string.Format(CultureInfo.CurrentCulture, "Exception parsing {0}: {1}", sourceName, ex.Message));
		}

		return false;
	}

	static class Configuration
	{
		public const string AppDomain = "appDomain";
		public const string AssertEquivalentMaxDepth = "assertEquivalentMaxDepth";
		public const string Culture = "culture";
		public const string DiagnosticMessages = "diagnosticMessages";
		public const string FailSkips = "failSkips";
		public const string FailWarns = "failWarns";
		public const string InternalDiagnosticMessages = "internalDiagnosticMessages";
		public const string LongRunningTestSeconds = "longRunningTestSeconds";
		public const string MaxParallelThreads = "maxParallelThreads";
		public const string MethodDisplay = "methodDisplay";
		public const string MethodDisplayOptions = "methodDisplayOptions";
		public const string ParallelAlgorithm = "parallelAlgorithm";
		public const string ParallelizeAssembly = "parallelizeAssembly";
		public const string ParallelizeTestCollections = "parallelizeTestCollections";
		public const string PreEnumerateTheories = "preEnumerateTheories";
		public const string PrintMaxEnumerableLength = "printMaxEnumerableLength";
		public const string PrintMaxObjectDepth = "printMaxObjectDepth";
		public const string PrintMaxObjectMemberCount = "printMaxObjectMemberCount";
		public const string PrintMaxStringLength = "printMaxStringLength";
		public const string Seed = "seed";
		public const string ShadowCopy = "shadowCopy";
		public const string ShowLiveOutput = "showLiveOutput";
		public const string StopOnFail = "stopOnFail";
		// This is undocumented but available for our own internal testing
		public const string SynchronousReporting = "synchronousReporting";
	}
}
