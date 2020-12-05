using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Internal;
using Xunit.Runner.Common;

namespace Xunit.Runner.InProc.SystemConsole
{
	/// <summary>
	/// The command line parser for the console runner.
	/// </summary>
	public class CommandLine
	{
		readonly Stack<string> arguments = new Stack<string>();
		readonly List<string> unknownOptions = new List<string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandLine"/> class.
		/// </summary>
		/// <param name="assemblyFileName">The assembly filename.</param>
		/// <param name="args">The command line arguments passed to the Main method.</param>
		/// <param name="fileExists">An optional delegate which checks for file existence.
		/// Available as an override solely for testing purposes.</param>
		protected CommandLine(
			string assemblyFileName,
			string[] args,
			Predicate<string>? fileExists = null)
		{
			fileExists ??= File.Exists;

			for (var i = args.Length - 1; i >= 0; i--)
				arguments.Push(args[i]);

			Project = Parse(assemblyFileName, fileExists);
		}

		/// <summary>
		/// <para>Option: -debug</para>
		/// <para>When set to <c>true</c>, will launch/attach the debugger before
		/// running any tests.</para>
		/// </summary>
		public bool Debug { get; protected set; }

		/// <summary>
		/// <para>Option: -diagnostics</para>
		/// <para>When set to <c>true</c>, will emit diagnostic messages to the console.</para>
		/// </summary>
		public bool DiagnosticMessages { get; protected set; }

		/// <summary>
		/// <para>Option: -failskips</para>
		/// <para>When set to <c>true</c>, converts skipped tests into failed tests.</para>
		/// </summary>
		public bool FailSkips { get; protected set; }

		/// <summary>
		/// <para>Option: -internaldiagnostics</para>
		/// <para>When set to <c>true</c>, will emit internal diagnostic messages to
		/// the console. This is typically only useful for the developers of xUnit.net
		/// itself, but may be requested when diagnosing issues.</para>
		/// </summary>
		public bool InternalDiagnosticMessages { get; protected set; }

		/// <summary>
		/// <para>Option: -maxthreads &lt;default | unlimited | n&gt;</para>
		/// <para>Sets the maximum number of simultaneous threads that will be allowed
		/// to run when tests are parallelized. The default is one thread per CPU core;
		/// unlimited will not limit threads; any number greater than 0 will use the
		/// given number of threads.</para>
		/// </summary>
		public int? MaxParallelThreads { get; set; }

		/// <summary>
		/// <para>Option: -noautoreporters</para>
		/// <para>When set to <c>true</c>, will prevent automatic reporters from
		/// being used. This typically includes environmentally-enabled reporters
		/// used in continuous integration systems like TeamCity, AppVeyor, or
		/// Azure DevOps.</para>
		/// </summary>
		public bool NoAutoReporters { get; protected set; }

		/// <summary>
		/// <para>Option: -nocolor</para>
		/// <para>When set to <c>true</c>, will prevent using any colors when printing
		/// messages to the console.</para>
		/// </summary>
		public bool NoColor { get; protected set; }

		/// <summary>
		/// <para>Option: -nologo</para>
		/// <para>When set to <c>true</c>, will suppress the copyright and version information
		/// that's normally printed at the top of the console output.</para>
		/// </summary>
		public bool NoLogo { get; protected set; }

		/// <summary>
		/// <para>Option: -pause</para>
		/// <para>When set to <c>true</c>, will pause the test runner just before running tests.</para>
		/// </summary>
		public bool Pause { get; protected set; }

		/// <summary>
		/// <para>Option: -preenumeratetheories</para>
		/// <para>When set to <c>true</c>, will force the pre-enumeration of theories before running
		/// tests. This is disabled by default for performance reasons.</para>
		/// </summary>
		public bool PreEnumerateTheories { get; protected set; }

		/// <summary>
		/// Gets or sets the project that describes the assembly to be tested.
		/// </summary>
		public XunitProject Project { get; protected set; }

		/// <summary>
		/// <para>Option: -parallel &lt;none | collections&gt;</para>
		/// <para>Will be <c>true</c> when the "collections" flag is passed; will be <c>false</c>
		/// when the "none" flag is passed.</para>
		/// </summary>
		public bool? ParallelizeTestCollections { get; set; }

		/// <summary>
		/// <para>Option: -serialize</para>
		/// <para>When set to <c>true</c>, will serialize all tests cases. Useful by the
		/// core developers to ensure that all test cases are properly serializable.</para>
		/// </summary>
		public bool Serialize { get; protected set; }

		/// <summary>
		/// <para>Option: -stoponfail</para>
		/// <para>When set to <c>true</c>, will attempt to stop running tests as soon as one
		/// has failed (by default, all tests will be run regardless of failures).</para>
		/// </summary>
		public bool StopOnFail { get; protected set; }

		/// <summary>
		/// <para>Option: -wait</para>
		/// <para>When set to <c>true</c>, will pause the test runner after all tests have
		/// finished running, but before exiting.</para>
		/// </summary>
		public bool Wait { get; protected set; }

		/// <summary>
		/// Chooses a reporter from the list of available reporters. Unless <see cref="NoAutoReporters"/>
		/// is set to <c>true</c>, it will first look for an environmentally enabled reporter;
		/// if none is available, then it will search through the command line options to
		/// determine which one to run. If there are no environmentally enabled reporters and
		/// no reporters passed on the command line, it will return an instance of
		/// <see cref="DefaultRunnerReporter"/>.
		/// </summary>
		/// <param name="reporters">The list of available reporters to choose from</param>
		/// <returns>The reporter that should be used during testing</returns>
		public IRunnerReporter ChooseReporter(IReadOnlyList<IRunnerReporter> reporters)
		{
			var result = default(IRunnerReporter);

			foreach (var unknownOption in unknownOptions)
			{
				var reporter = reporters.FirstOrDefault(r => r.RunnerSwitch == unknownOption) ?? throw new ArgumentException($"unknown option: -{unknownOption}");

				if (result != null)
					throw new ArgumentException("only one reporter is allowed");

				result = reporter;
			}

			if (!NoAutoReporters)
				result = reporters.FirstOrDefault(r => r.IsEnvironmentallyEnabled) ?? result;

			return result ?? new DefaultRunnerReporter();
		}

		/// <summary>
		/// For testing purposes only. Do not use.
		/// </summary>
		protected virtual string GetFullPath(string fileName) =>
			Path.GetFullPath(fileName);

		XunitProject GetProjectFile(
			string? assemblyFileName,
			string? configFileName) =>
				new XunitProject
				{
					new XunitProjectAssembly
					{
						AssemblyFilename = assemblyFileName,
						ConfigFilename = configFileName != null ? GetFullPath(configFileName) : null
					}
				};

		static void GuardNoOptionValue(KeyValuePair<string, string?> option)
		{
			if (option.Value != null)
				throw new ArgumentException($"error: unknown command line option: {option.Value}");
		}

		static bool IsConfigFile(string fileName) =>
			fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);

		/// <summary>
		/// Parses the command line, and returns an instance of <see cref="CommandLine"/> that
		/// has been populated based on the command line options that were passed.
		/// </summary>
		/// <param name="assemblyFileName">The optional assembly filename.</param>
		/// <param name="args">The command line arguments passed to the Main method.</param>
		/// <returns>The instance of the <see cref="CommandLine"/> object.</returns>
		public static CommandLine Parse(
			string assemblyFileName,
			params string[] args) =>
				new CommandLine(assemblyFileName, args);

		/// <summary>
		/// For testing purposes only. Do not use.
		/// </summary>
		protected XunitProject Parse(
			string assemblyFileName,
			Predicate<string> fileExists)
		{
			var configFileName = default(string);

			if (arguments.Count > 0 && !arguments.Peek().StartsWith("-", StringComparison.Ordinal))
			{
				configFileName = arguments.Pop();
				if (!IsConfigFile(configFileName))
					throw new ArgumentException($"expecting config file, got: {configFileName}");
				if (!fileExists(configFileName))
					throw new ArgumentException($"config file not found: {configFileName}");
			}

			var project = GetProjectFile(assemblyFileName, configFileName);

			while (arguments.Count > 0)
			{
				var option = PopOption(arguments);
				var optionName = option.Key.ToLowerInvariant();

				if (!optionName.StartsWith("-", StringComparison.Ordinal))
					throw new ArgumentException($"expected option, instead got: {option.Key}");

				optionName = optionName.Substring(1);

				if (optionName == "nologo")
				{
					GuardNoOptionValue(option);
					NoLogo = true;
				}
				else if (optionName == "failskips")
				{
					GuardNoOptionValue(option);
					FailSkips = true;
				}
				else if (optionName == "stoponfail")
				{
					GuardNoOptionValue(option);
					StopOnFail = true;
				}
				else if (optionName == "nocolor")
				{
					GuardNoOptionValue(option);
					NoColor = true;
				}
				else if (optionName == "noautoreporters")
				{
					GuardNoOptionValue(option);
					NoAutoReporters = true;
				}
				else if (optionName == "pause")
				{
					GuardNoOptionValue(option);
					Pause = true;
				}
				else if (optionName == "preenumeratetheories")
				{
					GuardNoOptionValue(option);
					PreEnumerateTheories = true;
				}
				else if (optionName == "debug")
				{
					GuardNoOptionValue(option);
					Debug = true;
				}
				else if (optionName == "serialize")
				{
					GuardNoOptionValue(option);
					Serialize = true;
				}
				else if (optionName == "wait")
				{
					GuardNoOptionValue(option);
					Wait = true;
				}
				else if (optionName == "diagnostics")
				{
					GuardNoOptionValue(option);
					DiagnosticMessages = true;
				}
				else if (optionName == "internaldiagnostics")
				{
					GuardNoOptionValue(option);
					InternalDiagnosticMessages = true;
				}
				else if (optionName == "maxthreads")
				{
					if (option.Value == null)
						throw new ArgumentException("missing argument for -maxthreads");

					switch (option.Value)
					{
						case "default":
							MaxParallelThreads = 0;
							break;

						case "unlimited":
							MaxParallelThreads = -1;
							break;

						default:
							int threadValue;
							if (!int.TryParse(option.Value, out threadValue) || threadValue < 1)
								throw new ArgumentException("incorrect argument value for -maxthreads (must be 'default', 'unlimited', or a positive number)");

							MaxParallelThreads = threadValue;
							break;
					}
				}
				else if (optionName == "parallel")
				{
					if (option.Value == null)
						throw new ArgumentException("missing argument for -parallel");

					if (!Enum.TryParse(option.Value, ignoreCase: true, out ParallelismOption parallelismOption))
						throw new ArgumentException("incorrect argument value for -parallel");

					ParallelizeTestCollections = parallelismOption switch
					{
						ParallelismOption.collections => true,
						_ => false,
					};
				}
				else if (optionName == "trait")
				{
					if (option.Value == null)
						throw new ArgumentException("missing argument for -trait");

					var pieces = option.Value.Split('=');
					if (pieces.Length != 2 || string.IsNullOrEmpty(pieces[0]) || string.IsNullOrEmpty(pieces[1]))
						throw new ArgumentException("incorrect argument format for -trait (should be \"name=value\")");

					var name = pieces[0];
					var value = pieces[1];
					project.Filters.IncludedTraits.Add(name, value);
				}
				else if (optionName == "notrait")
				{
					if (option.Value == null)
						throw new ArgumentException("missing argument for -notrait");

					var pieces = option.Value.Split('=');
					if (pieces.Length != 2 || string.IsNullOrEmpty(pieces[0]) || string.IsNullOrEmpty(pieces[1]))
						throw new ArgumentException("incorrect argument format for -notrait (should be \"name=value\")");

					var name = pieces[0];
					var value = pieces[1];
					project.Filters.ExcludedTraits.Add(name, value);
				}
				else if (optionName == "class")
				{
					if (option.Value == null)
						throw new ArgumentException("missing argument for -class");

					project.Filters.IncludedClasses.Add(option.Value);
				}
				else if (optionName == "noclass")
				{
					if (option.Value == null)
						throw new ArgumentException("missing argument for -noclass");

					project.Filters.ExcludedClasses.Add(option.Value);
				}
				else if (optionName == "method")
				{
					if (option.Value == null)
						throw new ArgumentException("missing argument for -method");

					project.Filters.IncludedMethods.Add(option.Value);
				}
				else if (optionName == "nomethod")
				{
					if (option.Value == null)
						throw new ArgumentException("missing argument for -nomethod");

					project.Filters.ExcludedMethods.Add(option.Value);
				}
				else if (optionName == "namespace")
				{
					if (option.Value == null)
						throw new ArgumentException("missing argument for -namespace");

					project.Filters.IncludedNamespaces.Add(option.Value);
				}
				else if (optionName == "nonamespace")
				{
					if (option.Value == null)
						throw new ArgumentException("missing argument for -nonamespace");

					project.Filters.ExcludedNamespaces.Add(option.Value);
				}
				else
				{
					// Might be a result output file...
					if (TransformFactory.AvailableTransforms.Any(t => t.ID.Equals(optionName, StringComparison.OrdinalIgnoreCase)))
					{
						if (option.Value == null)
							throw new ArgumentException($"missing filename for {option.Key}");

						EnsurePathExists(option.Value);

						project.Output.Add(optionName, option.Value);
					}
					// ...or it might be a reporter (we won't know until later)
					else
					{
						GuardNoOptionValue(option);
						unknownOptions.Add(optionName);
					}
				}
			}

			return project;
		}

		static KeyValuePair<string, string?> PopOption(Stack<string> arguments)
		{
			var option = arguments.Pop();
			string? value = null;

			if (arguments.Count > 0 && !arguments.Peek().StartsWith("-", StringComparison.Ordinal))
				value = arguments.Pop();

			return new KeyValuePair<string, string?>(option, value);
		}

		static void EnsurePathExists(string path)
		{
			var directory = Path.GetDirectoryName(path);

			if (string.IsNullOrEmpty(directory))
				return;

			Directory.CreateDirectory(directory);
		}
	}
}
