using System.ComponentModel;
using System.Reflection;

namespace Xunit.Runner.Common;

/// <summary/>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RegisteredRunnerReporters
{
	/// <summary>
	/// Please use <see cref="RegisteredRunnerConfig.GetRunnerReporters"/>.
	/// This method will be removed in the next major version.
	/// </summary>
	[Obsolete("Please use RegisteredRunnerConfig.GetRunnerReporters. This method will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static List<IRunnerReporter> Get(
		Assembly assembly,
		out List<string> messages) =>
			RegisteredRunnerConfig.GetRunnerReporters(assembly, out messages).ToList();
}
