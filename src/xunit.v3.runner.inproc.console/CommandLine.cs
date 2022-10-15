using System;
using System.Collections.Generic;
using System.Globalization;
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
			"  none        - turn off parallelization",
			"  collections - parallelize by collections [default]"
		);
	}

	void AddAssembly(
		Assembly assembly,
		string? assemblyFileName,
		string? configFileName,
		int? seed)
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
		projectAssembly.Configuration.Seed = seed ?? projectAssembly.Configuration.Seed;

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

		int? seed = null;
		if (Args.Length > argsStartIndex && Args[argsStartIndex].StartsWith(":"))
		{
			var seedValueText = Args[argsStartIndex].Substring(1);
			if (!int.TryParse(seedValueText, NumberStyles.None, NumberFormatInfo.CurrentInfo, out int parsedValue) || parsedValue < 0)
				throw new ArgumentException($"invalid seed value '{seedValueText}' (must be an integer in the range of 0 - 2147483647)");

			seed = parsedValue;
			++argsStartIndex;
		}

		string? configFileName = null;
		if (Args.Length > argsStartIndex && !Args[argsStartIndex].StartsWith("-") && Args[argsStartIndex].EndsWith(".json", StringComparison.OrdinalIgnoreCase))
		{
			configFileName = Args[argsStartIndex];
			++argsStartIndex;
		}

		AddAssembly(assembly, assemblyFileName, configFileName, seed);

		return ParseInternal(argsStartIndex);
	}
}
