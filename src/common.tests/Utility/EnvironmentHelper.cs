using System;

static class EnvironmentHelper
{
	static readonly Lazy<bool> isMono = new Lazy<bool>(() => Type.GetType("Mono.Runtime") != null);

	/// <summary>
	/// Returns <c>true</c> if you're currently running in Mono; <c>false</c> if you're running in .NET Framework.
	/// </summary>
	public static bool IsMono => isMono.Value;
}
