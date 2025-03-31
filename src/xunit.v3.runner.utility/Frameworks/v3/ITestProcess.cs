using System.IO;

namespace Xunit.v3;

/// <summary>
/// Represents a v3 test process that has been launched. When the process is disposed,
/// it should be allowed to clean up and exit within an appropriate amount of time,
/// and then killed if it will not stop cleanly.
/// </summary>
/// <remarks>
/// This extends the simplified <see cref="ITestProcessBase"/> by adding access to streams which
/// represent standard input and standard output.
/// </remarks>
public interface ITestProcess : ITestProcessBase
{
	/// <summary>
	/// Gets a <see cref="TextWriter"/> that can be used to write text from the standard
	/// input of the test process.
	/// </summary>
	TextWriter StandardInput { get; }

	/// <summary>
	/// Gets a <see cref="TextReader"/> that can be used to read text from the standard
	/// output of the test process.
	/// </summary>
	TextReader StandardOutput { get; }
}
