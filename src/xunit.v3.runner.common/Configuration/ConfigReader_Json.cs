using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Xunit.Internal;
using Xunit.v3;

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

			using var jsonStream = File.OpenRead(configFileName);
			var root = JsonSerializer.Deserialize<JsonElement>(jsonStream);
			if (root.ValueKind != JsonValueKind.Object)
			{
				warnings?.Add(string.Format(CultureInfo.CurrentCulture, "Couldn't parse config file '{0}': the root must be a JSON object", configFileName));
				return false;
			}

			foreach (var property in root.EnumerateObject())
			{
				if (property.Value.ValueKind == JsonValueKind.True || property.Value.ValueKind == JsonValueKind.False)
				{
					var booleanValue = property.Value.GetBoolean();

					if (string.Equals(property.Name, Configuration.DiagnosticMessages, StringComparison.OrdinalIgnoreCase))
						configuration.DiagnosticMessages = booleanValue;
					else if (string.Equals(property.Name, Configuration.FailSkips, StringComparison.OrdinalIgnoreCase))
						configuration.FailSkips = booleanValue;
					else if (string.Equals(property.Name, Configuration.FailWarns, StringComparison.OrdinalIgnoreCase))
						configuration.FailWarns = booleanValue;
					else if (string.Equals(property.Name, Configuration.InternalDiagnosticMessages, StringComparison.OrdinalIgnoreCase))
						configuration.InternalDiagnosticMessages = booleanValue;
					else if (string.Equals(property.Name, Configuration.ParallelizeAssembly, StringComparison.OrdinalIgnoreCase))
						configuration.ParallelizeAssembly = booleanValue;
					else if (string.Equals(property.Name, Configuration.ParallelizeTestCollections, StringComparison.OrdinalIgnoreCase))
						configuration.ParallelizeTestCollections = booleanValue;
					else if (string.Equals(property.Name, Configuration.PreEnumerateTheories, StringComparison.OrdinalIgnoreCase))
						configuration.PreEnumerateTheories = booleanValue;
					else if (string.Equals(property.Name, Configuration.ShadowCopy, StringComparison.OrdinalIgnoreCase))
						configuration.ShadowCopy = booleanValue;
					else if (string.Equals(property.Name, Configuration.StopOnFail, StringComparison.OrdinalIgnoreCase))
						configuration.StopOnFail = booleanValue;
				}
				else if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out var intValue))
				{
					if (string.Equals(property.Name, Configuration.MaxParallelThreads, StringComparison.OrdinalIgnoreCase))
					{
						if (intValue >= -1)
							configuration.MaxParallelThreads = intValue;
					}
					else if (string.Equals(property.Name, Configuration.LongRunningTestSeconds, StringComparison.OrdinalIgnoreCase))
					{
						if (intValue > 0)
							configuration.LongRunningTestSeconds = intValue;
					}
					else if (string.Equals(property.Name, Configuration.Seed, StringComparison.OrdinalIgnoreCase))
					{
						if (intValue >= 0)
							configuration.Seed = intValue;
					}
				}
				else if (property.Value.ValueKind == JsonValueKind.String)
				{
					var stringValue = property.Value.GetString();
					if (stringValue is not null)
					{
						if (string.Equals(property.Name, Configuration.MaxParallelThreads, StringComparison.OrdinalIgnoreCase))
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
						else if (string.Equals(property.Name, Configuration.MethodDisplay, StringComparison.OrdinalIgnoreCase))
						{
							if (Enum.TryParse<TestMethodDisplay>(stringValue, true, out var methodDisplay))
								configuration.MethodDisplay = methodDisplay;
						}
						else if (string.Equals(property.Name, Configuration.MethodDisplayOptions, StringComparison.OrdinalIgnoreCase))
						{
							if (Enum.TryParse<TestMethodDisplayOptions>(stringValue, true, out var methodDisplayOptions))
								configuration.MethodDisplayOptions = methodDisplayOptions;
						}
						else if (string.Equals(property.Name, Configuration.AppDomain, StringComparison.OrdinalIgnoreCase))
						{
							if (Enum.TryParse<AppDomainSupport>(stringValue, true, out var appDomain))
								configuration.AppDomain = appDomain;
						}
						else if (string.Equals(property.Name, Configuration.Culture, StringComparison.OrdinalIgnoreCase))
						{
							if (string.Equals("default", stringValue, StringComparison.OrdinalIgnoreCase))
								configuration.Culture = null;
							else if (string.Equals("invariant", stringValue, StringComparison.OrdinalIgnoreCase))
								configuration.Culture = string.Empty;
							else
								configuration.Culture = stringValue;
						}
					}
				}
			}

			return true;
		}
		catch (Exception ex)
		{
			warnings?.Add(string.Format(CultureInfo.CurrentCulture, "Exception loading config file '{0}': {1}", configFileName, ex.Message));
		}

		return false;
	}

	static class Configuration
	{
		public const string AppDomain = "appDomain";
		public const string Culture = "culture";
		public const string DiagnosticMessages = "diagnosticMessages";
		public const string FailSkips = "failSkips";
		public const string FailWarns = "failWarns";
		public const string InternalDiagnosticMessages = "internalDiagnosticMessages";
		public const string LongRunningTestSeconds = "longRunningTestSeconds";
		public const string MaxParallelThreads = "maxParallelThreads";
		public const string MethodDisplay = "methodDisplay";
		public const string MethodDisplayOptions = "methodDisplayOptions";
		public const string ParallelizeAssembly = "parallelizeAssembly";
		public const string ParallelizeTestCollections = "parallelizeTestCollections";
		public const string PreEnumerateTheories = "preEnumerateTheories";
		public const string Seed = "seed";
		public const string ShadowCopy = "shadowCopy";
		public const string StopOnFail = "stopOnFail";
	}
}
