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

	/// <summary>
	/// Returns <c>true</c> if you're currently running on Windows; <c>false</c> if you're running on
	/// non-Windows (like Linux or macOS). (Note: we do this by detecting Mono; this is not normally a good
	/// verification strategy, since you can run Mono on Windows, but in our case we know that we only use
	/// Mono with our unit tests on non-Windows machines. This would be a bad assumption for production code.)
	/// </summary>
	public static bool IsWindows =>
#if NETFRAMEWORK
		!IsMono;
#else
		OperatingSystem.IsWindows();
#endif

	/// <summary>
	/// Nullifies environment variables that are depended on by environmental-enabled runner reporters.
	/// This is useful when running unit tests for an environmentally-enabled runner reporter when running
	/// inside the test environment.
	/// </summary>
	/// <returns>A disposable object that restores the environment variables.</returns>
	public static IDisposable NullifyEnvironmentalReporters()
	{
		var result = new EnvironmentRestorer(reporterEnvironmentVariables);

		foreach (var variable in reporterEnvironmentVariables)
			Environment.SetEnvironmentVariable(variable, null);

		return result;
	}

	/// <summary>
	/// Stashes the values of one or more environment variables, and restores them to the original value
	/// when the returned object is disposed. Can be used to set/clear environment variables that are
	/// reacted to by code under test.
	/// </summary>
	/// <param name="variables">The variables to save and restore.</param>
	/// <returns>A disposable object that restores the environment variables.</returns>
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
