using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit.Internal;
using Xunit.Runner.Common;

namespace Xunit.Runner.InProc.SystemConsole;

/// <summary/>
public class CommandLine : CommandLineParserBase
{
	readonly Assembly assembly;
	readonly string? assemblyFileName;

	/// <summary/>
	public CommandLine(
		Assembly assembly,
		string[] args,
		IReadOnlyList<IRunnerReporter>? runnerReporters = null,
		string? reporterFolder = null)
			: base(runnerReporters, reporterFolder ?? Path.GetDirectoryName(assembly.GetSafeLocation()), args)
	{
		this.assembly = assembly;
		assemblyFileName = assembly.GetSafeLocation();

		// General options
		AddParser(
			"parallel", OnParallel, CommandLineGroup.General, "<option>",
			"set parallelization based on option",
			"  none        - turn off all parallelization",
			"  collections - only parallelize collections"
		);
	}

	void AddAssembly(
		Assembly assembly,
		string? assemblyFileName,
		string? configFileName)
	{
		if (assemblyFileName != null && !FileExists(assemblyFileName))
			throw new ArgumentException($"assembly not found: {assemblyFileName}");
		if (configFileName != null && !FileExists(configFileName))
			throw new ArgumentException($"config file not found: {configFileName}");

		var targetFramework = assembly.GetTargetFramework();
		var projectAssembly = new XunitProjectAssembly(Project)
		{
			Assembly = assembly,
			AssemblyFileName = GetFullPath(assemblyFileName),
			ConfigFileName = GetFullPath(configFileName),
			TargetFramework = targetFramework
		};

		ConfigReader_Json.Load(projectAssembly.Configuration, projectAssembly.AssemblyFileName, projectAssembly.ConfigFileName);

		Project.Add(projectAssembly);
	}

	/// <summary/>
	protected override Assembly LoadAssembly(string dllFile) =>
#if NETFRAMEWORK
		Assembly.LoadFile(dllFile);
#else
		Assembly.Load(new AssemblyName(Path.GetFileNameWithoutExtension(dllFile)));
#endif

	/// <summary/>
	public XunitProject Parse()
	{
		if (Project.Assemblies.Count > 0)
			throw new InvalidOperationException("Parse may only be called once");

		var argsStartIndex = 0;

		string? configFileName = null;
		if (Args.Length > 0 && !Args[0].StartsWith("-") && Args[0].EndsWith(".json", StringComparison.OrdinalIgnoreCase))
		{
			configFileName = Args[0];
			argsStartIndex = 1;
		}

		AddAssembly(assembly, assemblyFileName, configFileName);

		return ParseInternal(argsStartIndex);
	}
}
