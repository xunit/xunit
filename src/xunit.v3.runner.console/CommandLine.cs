#pragma warning disable CA1852  // This type is not sealed because it's derived from in unit tests

using Xunit.Runner.Common;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public class CommandLine : CommandLineParserBase
{
	/// <summary/>
	public CommandLine(
		ConsoleHelper consoleHelper,
		string[] args,
		IReadOnlyList<IRunnerReporter> runnerReporters)
			: base(consoleHelper, runnerReporters, args) =>
				AddParsers();

	void AddParsers()
	{
		// General options
		AddParser("assertEquivalentMaxDepth", OnAssertEquivalentMaxDepth, CommandLineGroup.General, "<option>",
			"override the maximum recursive depth when comparing objects with Assert.Equivalent",
			$"  any integer value >= 1 is valid (default value is {EnvironmentVariables.Defaults.AssertEquivalentMaxDepth})"
		);
		AddParser(
			"parallelAssemblies", OnParallelAssemblies, CommandLineGroup.General, "<option>",
			"run assemblies in parallel to each other",
			"  on  - run assemblies in parallel",
			"  off - do not run assemblies in parallel [default]"
		);
		AddParser("pause", OnPause, CommandLineGroup.General, null, "wait for input before running tests");
		AddParser("wait", OnWait, CommandLineGroup.General, null, "wait for input after completion");

		// Simple filtering
		AddParser(
			"displayName", OnDisplayName, CommandLineGroup.FilterSimple, "\"name\"",
			"run all tests with a matching test case display name (wildcard '*' is supported",
			"at the beginning and/or end of the filter)",
			"  if specified more than once, acts as an OR operation",
			"    v1 and v2 projects are supported; v3 projects >= 4.0.0 are also supported"
		);
		AddParser(
			"displayName-", OnDisplayNameMinus, CommandLineGroup.FilterSimple, "\"name\"",
			"do not run tests with a matching test case display name (wildcard '*' is supported",
			"at the beginning and/or end of the filter)",
			"  if specified more than once, acts as an AND operation",
			"    v1 and v2 projects are supported; v3 projects >= 4.0.0 are also supported"
		);

		// .NET Framework options
		AddParser(
			"appDomains", OnAppDomains, CommandLineGroup.NetFramework, "<option>",
			"choose an app domain mode",
			"  required    - force app domains on",
			"  denied      - force app domains off",
			"  ifavailable - use app domains if they're available [default]"
		);
		AddParser("noShadow", OnNoShadow, CommandLineGroup.NetFramework, null, "do not shadow copy assemblies");

		// Deprecated options
		AddHiddenParser("noappdomain", kvp =>
		{
			ParseWarnings.Add($"The '-noAppDomain' switch has been deprecated in favor of '-appDomains denied' and will be removed in the next major version");
			OnNoAppDomain(kvp);
		});
	}

	void AddAssembly(
		string assemblyFileName,
		string? configFileName,
		int? seed)
	{
		if (!FileExists(assemblyFileName))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "assembly not found: {0}", assemblyFileName));
		if (configFileName is not null && !FileExists(configFileName))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "config file not found: {0}", configFileName));

		var metadata =
			AssemblyUtility.GetAssemblyMetadata(assemblyFileName)
				?? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "not a valid .NET assembly: {0}", assemblyFileName));
		if (metadata.XunitVersion == 0)
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "not an xUnit.net test assembly: {0}", assemblyFileName));

		var projectAssembly = new XunitProjectAssembly(Project, GetFullPath(assemblyFileName), metadata) { ConfigFileName = GetFullPath(configFileName) };

		ConfigReader.Load(projectAssembly.Configuration, projectAssembly.AssemblyFileName, projectAssembly.ConfigFileName, ParseWarnings);
		projectAssembly.Configuration.Seed = seed ?? projectAssembly.Configuration.Seed;

		Project.Add(projectAssembly);
	}

	void OnAppDomains(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -appdomains");

		var appDomainSupport = option.Value switch
		{
			"denied" => AppDomainSupport.Denied,
			"ifavailable" => AppDomainSupport.IfAvailable,
			"required" => AppDomainSupport.Required,
			_ => throw new ArgumentException("incorrect argument value for -appdomains (must be 'denied', 'required', or 'ifavailable')"),
		};

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.AppDomain = appDomainSupport;
	}

	void OnNoAppDomain(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.AppDomain = AppDomainSupport.Denied;
	}

	void OnNoShadow(KeyValuePair<string, string?> option)
	{
		GuardNoOptionValue(option);
		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.ShadowCopy = false;
	}

	void OnParallelAssemblies(KeyValuePair<string, string?> option)
	{
		if (option.Value is null)
			throw new ArgumentException("missing argument for -parallelAssemblies");

		var parallelAssemblies = option.Value.ToUpperInvariant() switch
		{
			"ON" or "TRUE" or "1" => true,
			"OFF" or "FALSE" or "0" => false,
			_ => throw new ArgumentException("incorrect argument value for -parallelAssemblies")
		};

		foreach (var projectAssembly in Project.Assemblies)
			projectAssembly.Configuration.ParallelizeAssembly = parallelAssemblies;
	}

	/// <summary/>
	public XunitProject Parse()
	{
		if (Project.Assemblies.Count > 0)
			throw new InvalidOperationException("Parse may only be called once");

		var argsStartIndex = 0;

		while (argsStartIndex < Args.Count)
		{
			if (Args[argsStartIndex].StartsWith('-'))
				break;

			var assemblyFileName = Args[argsStartIndex++];

			int? seed = null;
			var seedIndex = assemblyFileName.LastIndexOf(':');
			if (seedIndex > 1)  // Skip colon from drive letter
			{
				var seedValueText = assemblyFileName.Substring(seedIndex + 1);
				if (!int.TryParse(seedValueText, NumberStyles.None, NumberFormatInfo.CurrentInfo, out var parsedValue) || parsedValue < 0)
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "invalid seed value '{0}' (must be an integer in the range of 0 - 2147483647)", seedValueText));

				seed = parsedValue;
				assemblyFileName = assemblyFileName.Substring(0, seedIndex);
			}

			if (IsConfigFile(assemblyFileName))
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "expecting assembly, got config file: {0}", assemblyFileName));

			string? configFileName = null;
			if (argsStartIndex < Args.Count)
			{
				var value = Args[argsStartIndex];
				if (!value.StartsWith('-') && IsConfigFile(value))
				{
					configFileName = value;
					++argsStartIndex;
				}
			}

			AddAssembly(assemblyFileName, configFileName, seed);
		}

		return ParseInternal(argsStartIndex);
	}
}
