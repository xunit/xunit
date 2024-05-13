using System.Runtime.InteropServices;

namespace Xunit.BuildTools.Utility;

public static class NativeMethods
{
	public static class Windows
	{
		public const uint CTRL_C = 0;

		// [DllImport("kernel32.dll", SetLastError = true)]
		// public static extern bool AttachConsole(uint dwProcessId);

		// [DllImport("kernel32.dll", SetLastError = true)]
		// public static extern bool FreeConsole();

		// https://learn.microsoft.com/en-us/windows/console/generateconsolectrlevent
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool GenerateConsoleCtrlEvent(uint eventType, uint processGroupID);

		// // https://learn.microsoft.com/en-us/windows/console/getconsoleprocesslist
		// [DllImport("kernel32.dll", SetLastError = true)]
		// public static extern uint GetConsoleProcessList(/* out */uint[] lpdwProcessList, uint dwProcessCount);

		// [DllImport("kernel32.dll", SetLastError = true)]
		// public static extern bool SetConsoleCtrlHandler(Func<uint, bool>? handler, bool add);
	}

	public static class Unix
	{
		// https://www.tutorialspoint.com/unix/unix-signals-traps.htm
		public const int SIGINT = 2;

		// https://man7.org/linux/man-pages/man2/kill.2.html
		[DllImport("libc", SetLastError = true)]
		public static extern int kill(int pid, int sig);
	}
}
