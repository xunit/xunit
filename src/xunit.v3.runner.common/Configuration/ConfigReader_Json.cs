using System;
using System.IO;
using System.Text.Json;
using Xunit.v3;

namespace Xunit.Runner.Common
{
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
		/// <returns>A flag which indicates whether configuration values were read.</returns>
		public static bool Load(
			TestAssemblyConfiguration configuration,
			string? assemblyFileName,
			string? configFileName = null)
		{
			// If they provide a configuration file, we only read that, success or failure
			if (configFileName != null)
				return configFileName.EndsWith(".json", StringComparison.Ordinal) && LoadFile(configuration, configFileName);

			// If there's no assembly file, then we can't find co-located xunit.runner.json files
			if (string.IsNullOrWhiteSpace(assemblyFileName))
				return false;

			var assemblyName = Path.GetFileNameWithoutExtension(assemblyFileName);
			var directoryName = Path.GetDirectoryName(assemblyFileName)!;

			// {assembly}.xunit.runner.json takes priority over xunit.runner.json
			return
				LoadFile(configuration, Path.Combine(directoryName, $"{assemblyName}.xunit.runner.json")) ||
				LoadFile(configuration, Path.Combine(directoryName, "xunit.runner.json"));
		}

		static bool LoadFile(
			TestAssemblyConfiguration configuration,
			string configFileName)
		{
			try
			{
				if (!File.Exists(configFileName))
					return false;

				var json = File.ReadAllText(configFileName);
				return LoadJson(configuration, json);
			}
			catch { }

			return false;
		}

		static bool LoadJson(
			TestAssemblyConfiguration configuration,
			string json)
		{
			try
			{
				var root = JsonSerializer.Deserialize<JsonElement>(json);
				if (root.ValueKind != JsonValueKind.Object)
					return false;

				foreach (var property in root.EnumerateObject())
				{
					if (property.Value.ValueKind == JsonValueKind.True || property.Value.ValueKind == JsonValueKind.False)
					{
						var booleanValue = property.Value.GetBoolean();

						if (string.Equals(property.Name, Configuration.DiagnosticMessages, StringComparison.OrdinalIgnoreCase))
							configuration.DiagnosticMessages = booleanValue;
						else if (string.Equals(property.Name, Configuration.FailSkips, StringComparison.OrdinalIgnoreCase))
							configuration.FailSkips = booleanValue;
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
					}
					else if (property.Value.ValueKind == JsonValueKind.String)
					{
						var stringValue = property.Value.GetString();
						if (stringValue != null)
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
									if (match.Success && decimal.TryParse(match.Groups[1].Value, out var maxThreadMultiplier))
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
						}
					}
				}

				return true;
			}
			catch { }

			return false;
		}

		static class Configuration
		{
			public const string AppDomain = "appDomain";
			public const string DiagnosticMessages = "diagnosticMessages";
			public const string FailSkips = "failSkips";
			public const string InternalDiagnosticMessages = "internalDiagnosticMessages";
			public const string LongRunningTestSeconds = "longRunningTestSeconds";
			public const string MaxParallelThreads = "maxParallelThreads";
			public const string MethodDisplay = "methodDisplay";
			public const string MethodDisplayOptions = "methodDisplayOptions";
			public const string ParallelizeAssembly = "parallelizeAssembly";
			public const string ParallelizeTestCollections = "parallelizeTestCollections";
			public const string PreEnumerateTheories = "preEnumerateTheories";
			public const string ShadowCopy = "shadowCopy";
			public const string StopOnFail = "stopOnFail";
		}
	}
}
