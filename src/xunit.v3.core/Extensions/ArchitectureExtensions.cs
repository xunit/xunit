using System.Runtime.InteropServices;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class ArchitectureExtensions
{
	/// <summary>
	/// Attempts to convert <see cref="Architecture"/> into a display-friendly name.
	/// </summary>
	/// <remarks>
	/// The supported values are the ones known as of .NET 9. An unknown architecture will\
	/// return "Unknown".
	/// </remarks>
	public static string ToDisplayName(this Architecture architecture) =>
		architecture switch
		{
			Architecture.X86 => "x86",
			Architecture.X64 => "x64",
			Architecture.Arm or Architecture.Arm64 => "ARM",
			// Additional values not in netstandard2.0 have been copied from
			// https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.architecture
			// (this list is comprehensive as of .NET 9)
			(Architecture)4 => "WebAssembly",
			(Architecture)5 => "S390x",
			(Architecture)6 => "LoongArch64",
			(Architecture)7 => "ARMv6",
			(Architecture)8 => "PowerPC",
			(Architecture)9 => "RiscV",
			_ => "Unknown",
		};
}
