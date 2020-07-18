using System;
using System.IO;
using System.Text.Json;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// This class is used to read configuration information for a test assembly.
	/// </summary>
	public static class ConfigReader_Json
	{
		/// <summary>
		/// Loads the test assembly configuration for the given test assembly from JSON.
		/// </summary>
		/// <param name="json">The JSON text to read</param>
		/// <returns>The test assembly configuration.</returns>
		public static TestAssemblyConfiguration? Load(string json)
		{
			try
			{
				var result = new TestAssemblyConfiguration();
				var root = JsonSerializer.Deserialize<JsonElement>(json);
				if (root.ValueKind != JsonValueKind.Object)
					return null;

				foreach (var property in root.EnumerateObject())
				{
					if (property.Value.ValueKind == JsonValueKind.True || property.Value.ValueKind == JsonValueKind.False)
					{
						var booleanValue = property.Value.GetBoolean();

						if (string.Equals(property.Name, Configuration.DiagnosticMessages, StringComparison.OrdinalIgnoreCase))
							result.DiagnosticMessages = booleanValue;
						else if (string.Equals(property.Name, Configuration.InternalDiagnosticMessages, StringComparison.OrdinalIgnoreCase))
							result.InternalDiagnosticMessages = booleanValue;
						else if (string.Equals(property.Name, Configuration.ParallelizeAssembly, StringComparison.OrdinalIgnoreCase))
							result.ParallelizeAssembly = booleanValue;
						else if (string.Equals(property.Name, Configuration.ParallelizeTestCollections, StringComparison.OrdinalIgnoreCase))
							result.ParallelizeTestCollections = booleanValue;
						else if (string.Equals(property.Name, Configuration.PreEnumerateTheories, StringComparison.OrdinalIgnoreCase))
							result.PreEnumerateTheories = booleanValue;
						else if (string.Equals(property.Name, Configuration.ShadowCopy, StringComparison.OrdinalIgnoreCase))
							result.ShadowCopy = booleanValue;
						else if (string.Equals(property.Name, Configuration.StopOnFail, StringComparison.OrdinalIgnoreCase))
							result.StopOnFail = booleanValue;
					}
					else if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out var intValue))
					{
						if (string.Equals(property.Name, Configuration.MaxParallelThreads, StringComparison.OrdinalIgnoreCase))
						{
							if (intValue >= -1)
								result.MaxParallelThreads = intValue;
						}
						else if (string.Equals(property.Name, Configuration.LongRunningTestSeconds, StringComparison.OrdinalIgnoreCase))
						{
							if (intValue > 0)
								result.LongRunningTestSeconds = intValue;
						}
					}
					else if (property.Value.ValueKind == JsonValueKind.String)
					{
						var stringValue = property.Value.GetString();

						if (string.Equals(property.Name, Configuration.MethodDisplay, StringComparison.OrdinalIgnoreCase))
						{
							if (Enum.TryParse<TestMethodDisplay>(stringValue, true, out var methodDisplay))
								result.MethodDisplay = methodDisplay;
						}
						else if (string.Equals(property.Name, Configuration.MethodDisplayOptions, StringComparison.OrdinalIgnoreCase))
						{
							if (Enum.TryParse<TestMethodDisplayOptions>(stringValue, true, out var methodDisplayOptions))
								result.MethodDisplayOptions = methodDisplayOptions;
						}
						else if (string.Equals(property.Name, Configuration.AppDomain, StringComparison.OrdinalIgnoreCase))
						{
							if (Enum.TryParse<AppDomainSupport>(stringValue, true, out var appDomain))
								result.AppDomain = appDomain;
						}
					}
				}

				return result;
			}
			catch { }

			return null;
		}

		/// <summary>
		/// Loads the test assembly configuration for the given test assembly.
		/// </summary>
		/// <param name="assemblyFileName">The test assembly.</param>
		/// <param name="configFileName">The test assembly configuration file.</param>
		/// <returns>The test assembly configuration.</returns>
		public static TestAssemblyConfiguration? Load(string assemblyFileName, string? configFileName = null)
		{
			Guard.ArgumentNotNull(nameof(assemblyFileName), assemblyFileName);

			if (configFileName != null)
				return configFileName.EndsWith(".json", StringComparison.Ordinal) ? LoadFile(configFileName) : null;

			var assemblyName = Path.GetFileNameWithoutExtension(assemblyFileName);
			var directoryName = Path.GetDirectoryName(assemblyFileName)!;

			return LoadFile(Path.Combine(directoryName, $"{assemblyName}.xunit.runner.json"))
				?? LoadFile(Path.Combine(directoryName, "xunit.runner.json"));
		}

		static TestAssemblyConfiguration? LoadFile(string configFileName)
		{
			try
			{
				if (!File.Exists(configFileName))
					return null;

				var json = File.ReadAllText(configFileName);
				return Load(json);
			}
			catch { }

			return null;
		}

		static class Configuration
		{
			public const string AppDomain = "appDomain";
			public const string DiagnosticMessages = "diagnosticMessages";
			public const string InternalDiagnosticMessages = "internalDiagnosticMessages";
			public const string MaxParallelThreads = "maxParallelThreads";
			public const string MethodDisplay = "methodDisplay";
			public const string MethodDisplayOptions = "methodDisplayOptions";
			public const string ParallelizeAssembly = "parallelizeAssembly";
			public const string ParallelizeTestCollections = "parallelizeTestCollections";
			public const string PreEnumerateTheories = "preEnumerateTheories";
			public const string ShadowCopy = "shadowCopy";
			public const string StopOnFail = "stopOnFail";
			public const string LongRunningTestSeconds = "longRunningTestSeconds";
		}
	}
}
