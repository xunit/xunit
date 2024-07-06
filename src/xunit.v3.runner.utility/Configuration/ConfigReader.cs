using System.Collections.Generic;
using System.Globalization;

// This is in this namespace so that it aligns with ConfigReader_Json, which comes from xunit.v3.runner.common
namespace Xunit.Runner.Common;

/// <summary>
/// This class is used to read configuration information for a test assembly.
/// </summary>
public static class ConfigReader
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
		// JSON configuration takes priority over XML configuration
		if (ConfigReader_Json.Load(configuration, assemblyFileName, configFileName, warnings))
			return true;

#if NETFRAMEWORK
		if (ConfigReader_Configuration.Load(configuration, assemblyFileName, configFileName, warnings))
			return true;
#endif

		// If we end up here with a config file and no warnings, we have an unsupported file type
		if (configFileName is not null && warnings is not null && warnings.Count == 0)
			warnings.Add(string.Format(CultureInfo.CurrentCulture, "Couldn't load config file '{0}': unknown file type", configFileName));

		return false;
	}
}
