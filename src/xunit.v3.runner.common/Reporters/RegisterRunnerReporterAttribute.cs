using System;

namespace Xunit.Runner.Common;

/// <summary>
/// Used to decorate xUnit.net test assemblies to indicate the availability of a custom
/// runner reporter.
/// </summary>
/// <param name="runnerReporterType">The type of the runner reporter to register. The type
/// must implement <see cref="IRunnerReporter"/>.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class RegisterRunnerReporterAttribute(Type runnerReporterType) :
	Attribute, IRegisterRunnerReporterAttribute
{
	/// <inheritdoc/>
	public Type RunnerReporterType { get; } = runnerReporterType;
}
