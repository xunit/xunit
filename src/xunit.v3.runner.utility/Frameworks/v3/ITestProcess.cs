using System;
using System.IO;

namespace Xunit.v3;

/// <summary>
/// Represents a v3 test process that has been launched. When the process is disposed,
/// it should be allowed to cleaned up and exit within an appropriate amount of time,
/// and then killed if it will not stop cleanly.
/// </summary>
public interface ITestProcess : IDisposable
{
	/// <summary>
	/// Gets a flag to indicate whether the test process has exited or not yet.
	/// </summary>
	bool HasExited { get; }

	/// <summary>
	/// Gets a <see cref="TextReader"/> that can be used to read text from the standard
	/// output of the test process.
	/// </summary>
	TextReader StandardOutput { get; }

	/// <summary>
	/// Wait for the specified number of milliseconds for the test process to exit.
	/// </summary>
	/// <param name="milliseconds">The amount of time, in milliseconds, to wait</param>
	/// <returns>Returns <c>true</c> if the process exited; <c>false</c>, otherwise</returns>
	bool WaitForExit(int milliseconds);
}
