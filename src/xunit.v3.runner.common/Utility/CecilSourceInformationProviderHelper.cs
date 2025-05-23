using System.IO;
using Xunit.Runner.Common;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class CecilSourceInformationProviderHelper
{
	/// <summary/>
	public static ISourceInformationProvider CreateForTesting(string? assemblyFileName)
	{
		if (!File.Exists(assemblyFileName))
			return NullSourceInformationProvider.Instance;

		return new CecilSourceInformationProvider(assemblyFileName);
	}
}
