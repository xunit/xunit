using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Runner.Common;

namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="ITestProcessLauncher"/> that will launch an xUnit.net v3 test
/// in-process.
/// </summary>
/// <remarks>
/// Note that this will require the runner author to implement dependency resolution, as no attempt
/// to do so is done here.
/// </remarks>
public sealed class InProcessTestProcessLauncher : ITestProcessLauncher
{
	InProcessTestProcessLauncher()
	{ }

	/// <summary>
	/// Gets the singleton instance of <see cref="InProcessTestProcessLauncher"/>.
	/// </summary>
	public static InProcessTestProcessLauncher Instance { get; } = new();

	/// <inheritdoc/>
	public ITestProcess? Launch(
		XunitProjectAssembly projectAssembly,
		IReadOnlyList<string> arguments)
	{
		Guard.ArgumentNotNull(projectAssembly);
		Guard.ArgumentNotNull(arguments);

		if (projectAssembly.AssemblyFileName is null)
			return default;
		if (projectAssembly.AssemblyMetadata is null || projectAssembly.AssemblyMetadata.TargetFrameworkIdentifier == TargetFrameworkIdentifier.UnknownTargetFramework)
			return default;

		// TODO: Should we validate that we match target frameworks?

		return InProcessTestProcess.Create(projectAssembly.AssemblyFileName, arguments);
	}
}
