#nullable enable

using System;
using System.Collections.Generic;

static class EnvironmentHelper
{
	static readonly Lazy<bool> isMono = new Lazy<bool>(() => Type.GetType("Mono.Runtime") is not null);
	static readonly string[] reporterEnvironmentVariables =
	{
		// AppVeyorReporter
		"APPVEYOR_API_URL",
		// TeamCityReporter
		"TEAMCITY_PROJECT_NAME",
		"TEAMCITY_PROCESS_FLOW_ID",
		// VstsReporter
		"VSTS_ACCESS_TOKEN",
		"SYSTEM_TEAMFOUNDATIONCOLLECTIONURI",
		"SYSTEM_TEAMPROJECT",
		"BUILD_BUILDID",
	};

	/// <summary>
	/// Returns <c>true</c> if you're currently running in Mono; <c>false</c> if you're running in .NET Framework.
	/// </summary>
	public static bool IsMono => isMono.Value;

	public static IDisposable NullifyEnvironmentalReporters()
	{
		var result = new EnvironmentRestorer(reporterEnvironmentVariables);

		foreach (var variable in reporterEnvironmentVariables)
			Environment.SetEnvironmentVariable(variable, null);

		return result;
	}

	public static IDisposable RestoreEnvironment(params string[] variables) =>
		new EnvironmentRestorer(variables);

	class EnvironmentRestorer : IDisposable
	{
		Dictionary<string, string?> savedVariables = new();

		public EnvironmentRestorer(string[] variables)
		{
			foreach (var variable in variables)
				savedVariables[variable] = Environment.GetEnvironmentVariable(variable);
		}

		public void Dispose()
		{
			foreach (var kvp in savedVariables)
				Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
		}
	}
}
