using System;
using System.IO;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL INTERFACE. DO NOT USE.
/// </summary>
public interface ITestProcess : IDisposable
{
	/// <summary/>
	int ID { get; }

	/// <summary/>
	StreamReader StandardOutput { get; }

	/// <summary/>
	bool WaitForExit(int milliseconds);
}
