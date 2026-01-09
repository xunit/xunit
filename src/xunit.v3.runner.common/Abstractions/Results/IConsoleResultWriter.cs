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

	/// <summary>
	/// Gets the legacy command line ID for this result writer. When not <see langword="null"/>, will register
	/// a hidden command line switch for <c>"-{LegacyID}"</c>, in addition to the normally registered
	/// command line switch for <c>"-result-{RegisteredID}"</c>.
	/// </summary>
	/// <remarks>
	/// Should only be set by the result writers that were built in prior to xUnit.net v3 4.0.
	/// Other result writers should return <see langword="null"/>.
	/// </remarks>
	string? LegacyID { get; }
}
