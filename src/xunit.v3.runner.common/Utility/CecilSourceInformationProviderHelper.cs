using System.IO;
using Xunit.Runner.Common;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class CecilSourceInformationProviderHelper
{
	/// <summary>
	/// This is like <see cref="CecilSourceInformationProvider.Create"/> except that it ignores
	/// the value from <see cref="RunSettingsUtility.CollectSourceInformation"/>.
	/// </summary>
	public static ISourceInformationProvider ForceCreate(string? assemblyFileName)
	{
		if (!File.Exists(assemblyFileName))
			return NullSourceInformationProvider.Instance;

		return new CecilSourceInformationProvider(assemblyFileName);
	}
}
