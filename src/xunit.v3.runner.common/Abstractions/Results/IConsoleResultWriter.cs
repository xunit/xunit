namespace Xunit.Runner.Common;

/// <summary>
/// Represents a result writer that can run in the console runner (notably, in both
/// <c>xunit.v3.runner.console</c> and the in-process console runner that's built into
/// v3 test projects), and report results to an output file of the user's choosing.
/// </summary>
public interface IConsoleResultWriter : IResultWriter
{
	/// <summary>
	/// Gets the description of the result file that's presented to the user when they
	/// ask for command line help.
	/// </summary>
	/// <remarks>
	/// Example: <c>"output results to xUnit.net v2+ XML file"</c>.
	/// </remarks>
	string Description { get; }
}
