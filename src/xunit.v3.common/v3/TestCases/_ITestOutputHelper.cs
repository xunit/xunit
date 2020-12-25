namespace Xunit.v3
{
	/// <summary>
	/// Represents a class which can be used to provide test output.
	/// </summary>
	public interface _ITestOutputHelper
	{
		/// <summary>
		/// Adds a line of text to the output.
		/// </summary>
		/// <param name="message">The message</param>
		void WriteLine(string message);

		/// <summary>
		/// Formats a line of text and adds it to the output.
		/// </summary>
		/// <param name="format">The message format</param>
		/// <param name="args">The format arguments</param>
		void WriteLine(string format, params object[] args);
	}
}
