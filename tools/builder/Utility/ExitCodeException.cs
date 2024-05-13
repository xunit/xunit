using System;

namespace Xunit.BuildTools.Utility;

public class ExitCodeException : Exception
{
	public ExitCodeException(int exitCode) :
		base($"Process exited with code {exitCode}")
	{
		ExitCode = exitCode;
	}

	public int ExitCode { get; }
}
