using System.Runtime.InteropServices;

public static class RuntimeInformationExtensions
{
	static readonly string ExecutableExtension =
		RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty;

	public static string FindTestAssembly(this string dllPath)
	{
		if (File.Exists(dllPath))
			return dllPath;

		var directoryName = Path.GetDirectoryName(dllPath)
			?? throw new ArgumentException($"Could not find test assembly '{dllPath}'");

		var executablePath = Path.Combine(directoryName, Path.GetFileNameWithoutExtension(dllPath) + ExecutableExtension);
		if (File.Exists(executablePath))
			return executablePath;

		throw new ArgumentException($"Could not find test assembly '{dllPath}' or '{executablePath}'");
	}
}
