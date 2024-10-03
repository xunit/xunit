namespace Xunit.Runner.InProc.SystemConsole;

/// <summary>
/// A flag which indicates the automated mode we're running in.
/// </summary>
public enum AutomatedMode
{
	/// <summary>
	/// We are running in non-automated mode
	/// </summary>
	Off = 0,

	/// <summary>
	/// We are running in automated mode, without synchronous message reporting
	/// </summary>
	Async = 1,

	/// <summary>
	/// We are running in automated mode, with synchronous message reporting
	/// </summary>
	Sync = 2,
}
