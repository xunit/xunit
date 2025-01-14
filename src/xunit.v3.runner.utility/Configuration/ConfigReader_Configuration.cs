#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit.Internal;
using Xunit.Sdk;

// This is in this namespace so that it aligns with ConfigReader_Json, which comes from xunit.v3.runner.common
namespace Xunit.Runner.Common;

/// <summary>
/// This class is used to read configuration information for a test assembly.
/// </summary>
public static class ConfigReader_Configuration
{
	/// <summary>
	/// Loads the test assembly configuration for the given test assembly.
	/// </summary>
	/// <param name="configuration">The configuration object to write the values to.</param>
	/// <param name="assemblyFileName">The test assembly file name.</param>
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
			if (!configFileName.EndsWith(".config", StringComparison.Ordinal))
				return false;

			if (!File.Exists(configFileName))
			{
				warnings?.Add(string.Format(CultureInfo.CurrentCulture, "Couldn't load config file '{0}': file not found", configFileName));
				return false;
			}
		}
		else
		{
			// If there's no assembly file, then we can't find co-located .config files
			if (string.IsNullOrWhiteSpace(assemblyFileName))
				return false;

			configFileName = assemblyFileName + ".config";
			if (!File.Exists(configFileName))
				return false;
		}

		try
		{
			var map = new ExeConfigurationFileMap { ExeConfigFilename = configFileName };
			var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
			if (config.AppSettings is not null)
			{
				var settings = config.AppSettings.Settings;

				configuration.AppDomain = GetEnum<AppDomainSupport>(settings, Configuration.AppDomain) ?? configuration.AppDomain;
				configuration.DiagnosticMessages = GetBoolean(settings, Configuration.DiagnosticMessages) ?? configuration.DiagnosticMessages;
				configuration.InternalDiagnosticMessages = GetBoolean(settings, Configuration.InternalDiagnosticMessages) ?? configuration.InternalDiagnosticMessages;
				configuration.MaxParallelThreads = GetInt(settings, Configuration.MaxParallelThreads) ?? configuration.MaxParallelThreads;
				configuration.MethodDisplay = GetEnum<TestMethodDisplay>(settings, Configuration.MethodDisplay) ?? configuration.MethodDisplay;
				configuration.MethodDisplayOptions = GetEnum<TestMethodDisplayOptions>(settings, Configuration.MethodDisplayOptions) ?? configuration.MethodDisplayOptions;
				configuration.ParallelAlgorithm = GetEnum<ParallelAlgorithm>(settings, Configuration.ParallelAlgorithm) ?? configuration.ParallelAlgorithm;
				configuration.ParallelizeAssembly = GetBoolean(settings, Configuration.ParallelizeAssembly) ?? configuration.ParallelizeAssembly;
				configuration.ParallelizeTestCollections = GetBoolean(settings, Configuration.ParallelizeTestCollections) ?? configuration.ParallelizeTestCollections;
				configuration.PreEnumerateTheories = GetBoolean(settings, Configuration.PreEnumerateTheories) ?? configuration.PreEnumerateTheories;
				configuration.ShadowCopy = GetBoolean(settings, Configuration.ShadowCopy) ?? configuration.ShadowCopy;
				configuration.StopOnFail = GetBoolean(settings, Configuration.StopOnFail) ?? configuration.StopOnFail;
				configuration.LongRunningTestSeconds = GetInt(settings, Configuration.LongRunningTestSeconds) ?? configuration.LongRunningTestSeconds;

				return true;
			}
		}
		catch (Exception ex)
		{
			warnings?.Add(string.Format(CultureInfo.CurrentCulture, "Exception loading config file '{0}': {1}", configFileName, ex.Message));
		}

		return false;
	}

	static bool? GetBoolean(
		KeyValueConfigurationCollection settings,
		string key) =>
			GetValue<bool>(
				settings,
				key,
				value =>
				{
					return value.ToUpperInvariant() switch
					{
						"TRUE" => true,
						"FALSE" => false,
						_ => null,
					};
				}
			);

	static TValue? GetEnum<TValue>(
		KeyValueConfigurationCollection settings,
		string key)
			where TValue : struct =>
				GetValue<TValue>(
					settings,
					key,
					value =>
					{
						try { return (TValue)Enum.Parse(typeof(TValue), value, true); }
						catch { return null; }
					}
				);

	static int? GetInt(
		KeyValueConfigurationCollection settings,
		string key) =>
			GetValue<int>(
				settings,
				key,
				ValueType => int.TryParse(ValueType, out var result) ? result : null
			);

	static T? GetValue<T>(
		KeyValueConfigurationCollection settings,
		string key,
		Func<string, T?> converter)
			where T : struct
	{
		var settingsKey = settings.AllKeys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));

		return
			settingsKey is not null
				? converter(settings[settingsKey].Value)
				: default;
	}

	static class Configuration
	{
		public const string AppDomain = "xunit.appDomain";
		public const string DiagnosticMessages = "xunit.diagnosticMessages";
		public const string InternalDiagnosticMessages = "xunit.internalDiagnosticMessages";
		public const string LongRunningTestSeconds = "xunit.longRunningTestSeconds";
		public const string MaxParallelThreads = "xunit.maxParallelThreads";
		public const string MethodDisplay = "xunit.methodDisplay";
		public const string MethodDisplayOptions = "xunit.methodDisplayOptions";
		public const string ParallelAlgorithm = "xunit.parallelAlgorithm";
		public const string ParallelizeAssembly = "xunit.parallelizeAssembly";
		public const string ParallelizeTestCollections = "xunit.parallelizeTestCollections";
		public const string PreEnumerateTheories = "xunit.preEnumerateTheories";
		public const string ShadowCopy = "xunit.shadowCopy";
		public const string StopOnFail = "xunit.stopOnFail";
	}
}

#endif
