using System;

namespace Xunit;

/// <summary>
/// Represents a class which can be used to provide test output.
/// </summary>
public interface ITestOutputHelper
{
	/// <summary>
	/// Gets the output provided by the test.
	/// </summary>
	/// <remarks>
	/// Note: This also ensures that any partial output that hasn't been reported yet gets reported.
	/// Calling in the middle of test execution is therefore not recommend.
	/// </remarks>
	string Output { get; }

	/// <summary>
	/// Adds text to the output.
	/// </summary>
	/// <param name="message">The message</param>
	void Write(string message);

	/// <summary>
	/// Adds formatted text to the output.
	/// </summary>
	/// <param name="format">The message format</param>
	/// <param name="args">The format arguments</param>
	void Write(
		string format,
		params object[] args);

	/// <summary>
	/// Adds text to the output, followed by <see cref="Environment.NewLine"/>.
	/// </summary>
	/// <param name="message">The message</param>
	void WriteLine(string message);

	/// <summary>
	/// Adds formatted text to the output, followed by <see cref="Environment.NewLine"/>.
	/// </summary>
	/// <param name="format">The message format</param>
	/// <param name="args">The format arguments</param>
	void WriteLine(
		string format,
		params object[] args);
}
