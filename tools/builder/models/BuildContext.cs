using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Bullseye;
using Bullseye.Internal;
using McMaster.Extensions.CommandLineUtils;
using SimpleExec;

[Command(Name = "build", Description = "Build utility for xUnit.net")]
[HelpOption("-?|-h|--help")]
public class BuildContext
{
	// Calculated properties

	public string BaseFolder { get; private set; }

	public string ConfigurationText => Configuration.ToString();

	public string ConsoleRunnerExe { get; private set; }

	public string ConsoleRunner32Exe { get; private set; }

	public bool NeedMono { get; private set; }

	public string PackageOutputFolder { get; private set; }

	public string TestFlagsNonParallel { get; private set; }

	public string TestFlagsParallel { get; private set; }

	public string TestOutputFolder { get; private set; }

	// User-controllable command-line options

	[Option("-c|--configuration", Description = "The target configuration (default: 'Release'; values: 'Debug', 'Release')")]
	public Configuration Configuration { get; } = Configuration.Release;

	[Option("-N|--no-color", Description = "Disable colored output")]
	public bool NoColor { get; }

	[Option("-s|--skip-dependencies", Description = "Do not run targets' dependencies")]
	public bool SkipDependencies { get; }

	[Argument(0, "targets", Description = "The target(s) to run (default: 'PR'; common values: 'Build', 'CI', 'Packages', 'PR', 'Restore', 'Test', 'TestV3')")]
	public BuildTarget[] Targets { get; } = new[] { BuildTarget.PR };

	[Option("-t|--timing", Description = "Emit timing information for each target")]
	public bool Timing { get; }

	[Option("-v|--verbosity", Description = "Set verbosity level (default: 'minimal'; values: 'q[uiet]', 'm[inimal]', 'n[ormal]', 'd[etailed]', and 'diag[nostic]'")]
	public BuildVerbosity Verbosity { get; } = BuildVerbosity.minimal;

	// Helper methods for build target consumption

	public void BuildStep(string message)
	{
		WriteLineColor(ConsoleColor.White, $"==> {message} <==");
		Console.WriteLine();
	}

	public async Task Exec(string name, string args, string redactedArgs = null, string workingDirectory = null)
	{
		if (redactedArgs == null)
			redactedArgs = args;

		if (NeedMono && name.EndsWith(".exe"))
		{
			args = $"{name} {args}";
			redactedArgs = $"{name} {redactedArgs}";
			name = "mono";
		}

		WriteLineColor(ConsoleColor.DarkGray, $"EXEC: {name} {redactedArgs}{Environment.NewLine}");

		await Command.RunAsync(name, args, workingDirectory ?? BaseFolder, /*noEcho*/ true);

		Console.WriteLine();
	}

	public async Task<int> OnExecuteAsync()
	{
		Exception error = default;

		try
		{
			NeedMono = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
			TestFlagsNonParallel = "-parallel none -maxthreads 1 ";
			TestFlagsParallel = "";

			// Find the folder with the solution file
			BaseFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			while (true)
			{
				if (Directory.Exists(Path.Combine(BaseFolder, ".git")))
					break;

				BaseFolder = Path.GetDirectoryName(BaseFolder);
				if (BaseFolder == null)
					throw new InvalidOperationException("Could not locate a solution file in the directory hierarchy");
			}

			ConsoleRunnerExe = Path.Combine(BaseFolder, "src", "xunit.v3.runner.console", "bin", ConfigurationText, "net472", "merged", "xunit.v3.runner.console.exe");
			ConsoleRunner32Exe = Path.Combine(BaseFolder, "src", "xunit.v3.runner.console", "bin", ConfigurationText + "_x86", "net472", "merged", "xunit.v3.runner.console.x86.exe");

			// Dependent folders
			PackageOutputFolder = Path.Combine(BaseFolder, "artifacts", "packages");
			Directory.CreateDirectory(PackageOutputFolder);

			TestOutputFolder = Path.Combine(BaseFolder, "artifacts", "test");
			Directory.CreateDirectory(TestOutputFolder);

			// Parse the targets
			var targetNames = Targets.Select(x => x.ToString()).ToList();

			// Turn off test parallelization in CI, for more repeatable test timing
			if (Targets.Contains(BuildTarget.CI))
				TestFlagsParallel = TestFlagsNonParallel;

			// Find target classes
			var targetCollection = new TargetCollection();
			var targets
				= Assembly.GetExecutingAssembly()
					.ExportedTypes
					.Select(x => new { type = x, attr = x.GetCustomAttribute<TargetAttribute>() })
					.Where(x => x.attr != null);

			foreach (var target in targets)
			{
				var method = target.type.GetRuntimeMethod("OnExecute", new[] { typeof(BuildContext) });

				if (method == null)
					targetCollection.Add(new Target(target.attr.TargetName, target.attr.DependentTargets));
				else
					targetCollection.Add(new ActionTarget(target.attr.TargetName, target.attr.DependentTargets, async () =>
					{
						var sw = Stopwatch.StartNew();

						try
						{
							await (Task)method.Invoke(null, new[] { this });
						}
						finally
						{
							if (Timing)
								WriteLineColor(ConsoleColor.Cyan, $"TIMING: Target '{target.attr.TargetName}' took {sw.Elapsed}{Environment.NewLine}");
						}
					}));
			}

			var swTotal = Stopwatch.StartNew();

			// Let Bullseye run the target(s)
			await targetCollection.RunAsync(targetNames, SkipDependencies, dryRun: false, parallel: false, new NullLogger(), _ => false);

			WriteLineColor(ConsoleColor.Green, $"==> Build success! <=={Environment.NewLine}");

			if (Timing)
				WriteLineColor(ConsoleColor.Cyan, $"TIMING: Build took {swTotal.Elapsed}{Environment.NewLine}");

			return 0;
		}
		catch (Exception ex)
		{
			error = ex;
			while (error is TargetInvocationException || error is TargetFailedException)
				error = error.InnerException;
		}

		Console.WriteLine();

		if (error is NonZeroExitCodeException nonZeroExit)
		{
			WriteLineColor(ConsoleColor.Red, "==> Build failed! <==");
			return nonZeroExit.ExitCode;
		}

		WriteLineColor(ConsoleColor.Red, $"==> Build failed! An unhandled exception was thrown <==");
		Console.WriteLine(error.ToString());
		return -1;
	}

	public void WriteColor(ConsoleColor foregroundColor, string text)
	{
		if (!NoColor)
			Console.ForegroundColor = foregroundColor;

		Console.Write(text);

		if (!NoColor)
			Console.ResetColor();
	}

	public void WriteLineColor(ConsoleColor foregroundColor, string text)
		=> WriteColor(foregroundColor, $"{text}{Environment.NewLine}");
}
