using System.Collections.Generic;
using Xunit.Runner.Common;

namespace Xunit.v3;

/// <summary>
/// Implement this to control the launch of an xUnit.net v3 test process.
/// </summary>
public interface ITestProcessLauncher
{
	/// <summary>
	/// Launches the test process. Returns <c>null</c> if the process could not be launched.
	/// </summary>
	/// <param name="projectAssembly">The test project assembly</param>
	/// <param name="arguments">The list of arguments to be passed to the in-process runner</param>
	ITestProcess? Launch(
		XunitProjectAssembly projectAssembly,
		IReadOnlyList<string> arguments);
}
