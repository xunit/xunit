using System;

namespace Xunit.v3;

/// <summary>
/// Used to decorate xUnit.net test assemblies to indicate that the developer wishes to have code
/// that runs during the test pipeline startup and shutdown (including both discovery and execution).
/// </summary>
/// <remarks>Test pipeline startup attributes are only valid at the assembly level, and only a
/// single instance is allowed.</remarks>
public interface ITestPipelineStartupAttribute
{
	/// <summary>
	/// Gets the test pipeline startup type. Must implement <see cref="ITestPipelineStartup"/>.
	/// </summary>
	Type TestPipelineStartupType { get; }
}
