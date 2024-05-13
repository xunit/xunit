using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bullseye;
using Bullseye.Internal;
using McMaster.Extensions.CommandLineUtils;
using Xunit.BuildTools.Utility;
using OperatingSystem = System.OperatingSystem;

namespace Xunit.BuildTools.Models;

[Command(Name = "build", Description = "Build utility for xUnit.net web site")]
[HelpOption("-?|-h|--help")]
public partial class BuildContext
{
	string? baseFolder;
	string? siteDestFolder;
	string? siteSourceFolder;

	// Calculated properties

	public string BaseFolder
	{
		get => baseFolder ?? throw new InvalidOperationException($"Tried to retrieve unset {nameof(BuildContext)}.{nameof(BaseFolder)}");
		private set => baseFolder = value ?? throw new ArgumentNullException(nameof(BaseFolder));
	}

	public string SiteDestFolder
	{
		get => siteDestFolder ?? throw new InvalidOperationException($"Tried to retrieve unset {nameof(BuildContext)}.{nameof(SiteDestFolder)}");
		private set => siteDestFolder = value ?? throw new ArgumentNullException(nameof(SiteDestFolder));
	}

	public string SiteSourceFolder
	{
		get => siteSourceFolder ?? throw new InvalidOperationException($"Tried to retrieve unset {nameof(BuildContext)}.{nameof(SiteSourceFolder)}");
		private set => siteSourceFolder = value ?? throw new ArgumentNullException(nameof(SiteSourceFolder));
	}

	// User-controllable command-line options

	[Option("-N|--no-color", Description = "Disable colored output")]
	public bool NoColor { get; }

	[Option("-s|--skip-dependencies", Description = "Do not run targets' dependencies")]
	public bool SkipDependencies { get; }

	[Argument(0, "targets", Description = "The target(s) to run (default: 'Build'; common values: 'Build', 'Serve', 'Restore')")]
	public string[] Targets { get; } = [BuildTarget.Build];

	[Option("-t|--timing", Description = "Emit timing information for each target")]
	public bool Timing { get; }

	// Helper methods for build target consumption

	public void BuildStep(string message)
	{
		WriteLineColor(ConsoleColor.White, $"==> {message} <==");
		Console.WriteLine();
	}

	public async Task Exec(
		string name,
		string args,
		string? redactedArgs = null,
		string? workingDirectory = null,
		bool throwOnNonZeroExitCode = true)
	{
		redactedArgs ??= args;

		var displayName =
			name.Contains(" ") || name.Contains("'")
				? "'" + name.Replace("'", "''") + "'"
				: name;

		WriteLineColor(ConsoleColor.DarkGray, $"EXEC: & {displayName} {redactedArgs}{Environment.NewLine}");

		var pi = new ProcessStartInfo(name, args) { WorkingDirectory = workingDirectory ?? BaseFolder };
		Process? process = null;
		var processedCtrlC = false;

		void cancelHandler(object? sender, ConsoleCancelEventArgs e)
		{
			if (process is null)
				return;

			if (!processedCtrlC)
			{
				processedCtrlC = true;
				process.SendSigInt();
			}

			e.Cancel = true;
		}

		Console.CancelKeyPress += cancelHandler;
		process = Process.Start(pi);

		if (process is not null)
		{
			process.EnableRaisingEvents = true;
			await process.WaitForExitAsync();
		}

		Console.CancelKeyPress -= cancelHandler;

		if (throwOnNonZeroExitCode && process is not null && process.ExitCode != 0)
			throw new ExitCodeException(process.ExitCode);

		Console.WriteLine();
	}

	// We run 'bundle' via cmd.exe on Windows, because it ends up being a batch file, so trying
	// to directly execute it fails (and if we tell Process.Start to use shell execute, it ends
	// up popping it into a new/temporary window rather than giving the output inline).
	public Task ExecBundle(
		string args,
		string? redactedArgs = null,
		string? workingDirectory = null,
		bool throwOnNonZeroExitCode = true)
	{
		if (OperatingSystem.IsWindows())
			return Exec("cmd", $"/c bundle " + args, redactedArgs is null ? null : "/c bundle " + redactedArgs, workingDirectory, throwOnNonZeroExitCode);
		else
			return Exec("bundle", args, redactedArgs, workingDirectory, throwOnNonZeroExitCode);
	}

	async Task<int> OnExecuteAsync()
	{
		var swTotal = Stopwatch.StartNew();

		try
		{
			// Find the root folder
			var baseFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			while (true)
			{
				if (baseFolder == null)
					throw new InvalidOperationException("Could not locate the build script in the directory hierarchy");

				if (Directory.GetFiles(baseFolder, "build.ps1").Length != 0)
					break;

				baseFolder = Path.GetDirectoryName(baseFolder);
			}

			BaseFolder = baseFolder;
			SiteSourceFolder = Path.Combine(BaseFolder, "site");
			SiteDestFolder = Path.Combine(BaseFolder, "_site");

			// Parse the targets
			var targetNames = Targets.Select(x => x.ToString()).ToList();

			// Find target classes
			var targetCollection = new TargetCollection();
			var targets =
				Assembly
					.GetExecutingAssembly()
					.ExportedTypes
					.Select(x => new { type = x, attr = x.GetCustomAttribute<TargetAttribute>() });

			foreach (var target in targets)
				if (target.attr != null)
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
								var instance = method.IsStatic ? null : Activator.CreateInstance(target.type);
								var task = (Task?)method.Invoke(instance, new[] { this });
								if (task != null)
									await task;
							}
							finally
							{
								if (Timing)
									WriteLineColor(ConsoleColor.Cyan, $"TIMING: Target '{target.attr.TargetName}' took {sw.Elapsed}{Environment.NewLine}");
							}
						}));
				}

			// Let Bullseye run the target(s)
			await targetCollection.RunAsync(targetNames, SkipDependencies, dryRun: false, parallel: false, new NullLogger(), _ => false);

			WriteLineColor(ConsoleColor.Green, $"==> Build success! <=={Environment.NewLine}");

			return 0;
		}
		catch (Exception ex)
		{
			var error = ex;
			while ((error is TargetInvocationException || error is TargetFailedException) && error.InnerException != null)
				error = error.InnerException;

			Console.WriteLine();

			if (error is ExitCodeException nonZeroExit)
			{
				WriteLineColor(ConsoleColor.Red, "==> Build failed! <==");
				return nonZeroExit.ExitCode;
			}

			WriteLineColor(ConsoleColor.Red, $"==> Build failed! An unhandled exception was thrown <==");
			Console.WriteLine(error.ToString());
			return -1;
		}
		finally
		{
			if (Timing)
				WriteLineColor(ConsoleColor.Cyan, $"TIMING: Build took {swTotal.Elapsed}{Environment.NewLine}");
		}
	}

	public void WriteColor(
		ConsoleColor foregroundColor,
		string text)
	{
		if (!NoColor)
			Console.ForegroundColor = foregroundColor;

		Console.Write(text);

		if (!NoColor)
			Console.ResetColor();
	}

	public void WriteLineColor(ConsoleColor foregroundColor, string text) =>
		WriteColor(foregroundColor, $"{text}{Environment.NewLine}");
}
