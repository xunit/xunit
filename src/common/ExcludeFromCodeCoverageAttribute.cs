#pragma warning disable IDE0161

// Important note: do not "modernize" this file, as it's used by CSharpAcceptanceTestV3Assembly, which
// does not use a current compiler version.

using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage]

namespace System.Diagnostics.CodeAnalysis
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	internal sealed class ExcludeFromCodeCoverageAttribute : Attribute
	{
		public ExcludeFromCodeCoverageAttribute() { }
	}
}
