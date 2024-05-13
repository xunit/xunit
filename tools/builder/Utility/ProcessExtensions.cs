using System;
using System.Diagnostics;

namespace Xunit.BuildTools.Utility;

public static class ProcessExtensions
{
	public static void SendSigInt(this Process? process)
	{
		if (process is null)
			return;

		if (OperatingSystem.IsWindows())
			NativeMethods.Windows.GenerateConsoleCtrlEvent(NativeMethods.Windows.CTRL_C, 0);
		else
			NativeMethods.Unix.kill(process.Id, NativeMethods.Unix.SIGINT);
	}
}
