namespace Xunit.Runner.InProc.SystemConsole;

/// <summary>
/// A flag which indicates the automated mode we're running in.
/// </summary>
public enum AutomatedMode
{
	/// <summary>
	/// We are running in non-automated mode
	/// </summary>
	Off = 1,

	/// <summary>
	/// We are running in automated mode, without synchronous message reporting
	/// </summary>
	Async = 2,

	/// <summary>
	/// We are running in automated mode, with synchronous message reporting
	/// </summary>
	Sync = 3,
}
