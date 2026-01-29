namespace Xunit.Runners;

/// <summary>
/// An enumeration which describes the current state of the system
/// </summary>
[Obsolete("This enum is deprecated as status is not reported by Xunit.SimpleRunner.AssemblyRunner. It will be removed in the next major release.")]
public enum AssemblyRunnerStatus
{
	/// <summary>The system is not discovering or executing tests</summary>
	Idle = 1,

	/// <summary>The system is discovering tests</summary>
	Discovering = 2,

	/// <summary>The system is executing tests</summary>
	Executing = 3,
}
