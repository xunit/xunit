#nullable enable

using System;
using System.Collections.Generic;

static class EnvironmentHelper
{
	static readonly string[] reporterEnvironmentVariables =
	[
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
	];

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
		readonly Dictionary<string, string?> savedVariables = [];

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
