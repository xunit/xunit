using System;

namespace Xunit.Internal;

internal static class EnvironmentUtility
{
	public static string? Computer =>
		// Windows
		Environment.GetEnvironmentVariable("COMPUTERNAME") ??
		// Linux
		Environment.GetEnvironmentVariable("HOSTNAME") ??
		Environment.GetEnvironmentVariable("NAME") ??
		// macOS
		Environment.GetEnvironmentVariable("HOST");

	public static string? Domain =>
		// Windows
		Environment.GetEnvironmentVariable("USERDOMAIN");

	public static string? User =>
		// Windows
		Environment.GetEnvironmentVariable("USERNAME") ??
		// Linux/macOS
		Environment.GetEnvironmentVariable("LOGNAME") ??
		Environment.GetEnvironmentVariable("USER");
}
