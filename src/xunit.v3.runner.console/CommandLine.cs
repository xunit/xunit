using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Internal;
using Xunit.Runner.Common;

namespace Xunit.Runner.SystemConsole;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public class CommandLine : CommandLineParserBase
{
	/// <summary/>
	protected CommandLine(
		IReadOnlyList<IRunnerReporter> reporters,
		string[] args)
			: base(reporters, null, args)
	{
		AddParsers();
	}

	/// <summary/>
	public CommandLine(
		string? reporterFolder,
		string[] args)
			: base(null, reporterFolder, args)
	{
		AddParsers();
	}

	private void AddParsers()
	{
		// General options
		AddParser(
			"parallel", OnParallel, CommandLineGroup.General, "<option>",
			"set parallelization based on option",
			"  none        - turn off all parallelization",
			"  collections - only parallelize collections",
			"  assemblies  - only parallelize assemblies",
			"  all         - parallelize assemblies & collections"
		);

		// .NET Framework options
		AddParser(
			"appdomains", OnAppDomains, CommandLineGroup.NetFramework, "<option>",
			"choose an app domain mode",
			"  required    - force app domains on",
			"  denied      - force app domains off",
			"  ifavailable - use app domains if they're available"
		);
		AddParser("noshadow", OnNoShadow, CommandLineGroup.NetFramework, null, "do not shadow copy assemblies");

		// Deprecated options
		AddHiddenParser("noappdomain", OnNoAppDomain);
	}

	void AddAssembly(
		string assemblyFileName,
		string? configFileName)
	{
		if (!FileExists(assemblyFileName))
			throw new ArgumentException($"assembly not found: {assemblyFileName}");
		if (configFileName != null && !FileExists(configFileName))
			throw new ArgumentException($"config file not found: {configFileName}");

		var targetFramework = AssemblyUtility.GetTargetFramework(assemblyFileName);
		var projectAssembly = new XunitProjectAssembly(Project)
		{
			AssemblyFileName = GetFullPath(assemblyFileName),
			ConfigFileName = GetFullPath(configFileName),
			TargetFramework = targetFramework
		};

		ConfigReader.Load(projectAssembly.Configuration, projectAssembly.AssemblyFileName, projectAssembly.ConfigFileName);

		Project.Add(projectAssembly);
	}

	/// <summary/>
	protected override Assembly LoadAssembly(string dllFile) =>
#if NETFRAMEWORK
		Assembly.LoadFile(dllFile);
#else
		Assembly.Load(new AssemblyName(Path.GetFileNameWithoutExtension(dllFile)));
#endif

	void OnAppDomains(KeyValuePair<string, string?> option)
	{
		if (option.Value == null)
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

	/// <summary/>
	public XunitProject Parse()
	{
		if (Project.Assemblies.Count > 0)
			throw new InvalidOperationException("Parse may only be called once");

		var argsStartIndex = 0;

		while (argsStartIndex < Args.Length)
		{
			if (Args[argsStartIndex].StartsWith("-", StringComparison.Ordinal))
				break;

			var assemblyFileName = Args[argsStartIndex++];
			if (IsConfigFile(assemblyFileName))
				throw new ArgumentException($"expecting assembly, got config file: {assemblyFileName}");

			string? configFileName = null;
			if (argsStartIndex < Args.Length)
			{
				var value = Args[argsStartIndex];
				if (!value.StartsWith("-", StringComparison.Ordinal) && IsConfigFile(value))
				{
					configFileName = value;
					++argsStartIndex;
				}
			}

			AddAssembly(assemblyFileName, configFileName);
		}

		return ParseInternal(argsStartIndex);
	}
}
