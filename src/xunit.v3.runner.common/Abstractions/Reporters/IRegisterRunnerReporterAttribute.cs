using System;

namespace Xunit.Runner.Common;

/// <summary>
/// Used to decorate xUnit.net test assemblies to indicate the availability of a custom
/// runner reporter.
/// </summary>
/// <remarks>
/// Runner reporter registration attributes are only valid at the assembly level.
/// </remarks>
public interface IRegisterRunnerReporterAttribute
{
	/// <summary>
	/// Gets the type of the runner reporter to be registered.
	/// </summary>
	/// <remarks>
	/// The runner reporter type must implement <see cref="IRunnerReporter"/>.
	/// </remarks>
	Type RunnerReporterType { get; }
}
