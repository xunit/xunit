using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents a project which contains zero or more test assemblies, as well as global
/// (cross-assembly) configuration settings.
/// </summary>
public class XunitProject
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitProject"/> class.
	/// </summary>
	public XunitProject() :
		this(new(), null)
	{ }

	XunitProject(
		TestProjectConfiguration configuration,
		IRunnerReporter? runnerReporter)
	{
		Configuration = Guard.ArgumentNotNull(configuration);
		this.runnerReporter = runnerReporter;
	}

	readonly List<XunitProjectAssembly> assemblies = [];
	IRunnerReporter? runnerReporter;

	/// <summary>
	/// Gets the assemblies that are in the project.
	/// </summary>
	public ICollection<XunitProjectAssembly> Assemblies => assemblies;

	/// <summary>
	/// Gets the configuration values for the test project.
	/// </summary>
	public TestProjectConfiguration Configuration { get; }

	/// <summary>
	/// Gets a flag indicating whether <see cref="RunnerReporter"/> has been set yet or not.
	/// </summary>
	public bool HasRunnerReporter => runnerReporter is not null;

	/// <summary>
	/// Gets or sets the runner reporter.
	/// </summary>
	public IRunnerReporter RunnerReporter
	{
		get => this.ValidateNullablePropertyValue(runnerReporter, nameof(RunnerReporter));
		set => runnerReporter = Guard.ArgumentNotNull(value, nameof(RunnerReporter));
	}

	/// <summary>
	/// Adds an assembly to the project.
	/// </summary>
	/// <param name="assembly">The assembly to add to the project.</param>
	public void Add(XunitProjectAssembly assembly)
	{
		Guard.ArgumentNotNull(assembly);

		assemblies.Add(assembly);
	}

	/// <summary>
	/// Create an instance of <see cref="XunitProject"/> populated by launch options.
	/// </summary>
	/// <param name="other">The project that is our template to copy from</param>
	/// <param name="launchOptions">The launch options</param>
	public static XunitProject WithLaunchOptions(
		XunitProject other,
		FrontControllerLaunchOptions launchOptions) =>
			new(
				new(
					Guard.ArgumentNotNull(other).Configuration.AssemblyInfo,
					other.Configuration.Debug,
					other.Configuration.IgnoreFailures,
					other.Configuration.List,
					other.Configuration.Output,
					other.Configuration.NoAutoReporters,
					other.Configuration.NoColor,
					other.Configuration.NoLogo,
					other.Configuration.Pause,
					other.Configuration.UseAnsiColor,
					other.Configuration.Wait,
					Guard.ArgumentNotNull(launchOptions).WaitForDebugger
				),
				other.runnerReporter
			);
}
