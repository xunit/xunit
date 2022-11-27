using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents a project which contains zero or more test assemblies, as well as global
/// (cross-assembly) configuration settings.
/// </summary>
public class XunitProject
{
	readonly List<XunitProjectAssembly> assemblies;
	IRunnerReporter? runnerReporter;

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitProject"/> class.
	/// </summary>
	public XunitProject()
	{
		assemblies = new List<XunitProjectAssembly>();
	}

	/// <summary>
	/// Gets the assemblies that are in the project.
	/// </summary>
	public ICollection<XunitProjectAssembly> Assemblies => assemblies;

	/// <summary>
	/// Gets the configuration values for the test project.
	/// </summary>
	public TestProjectConfiguration Configuration { get; } = new();

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
}
