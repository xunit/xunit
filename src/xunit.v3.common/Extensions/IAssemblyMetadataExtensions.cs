using System.Globalization;
using System.Reflection;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Extension methods for <see cref="_IAssemblyMetadata"/>.
/// </summary>
public static class IAssemblyMetadataExtensions
{
	/// <summary>
	/// Computes the simple assembly name from <see cref="_IAssemblyMetadata.AssemblyName"/>.
	/// </summary>
	/// <returns>The simple assembly name.</returns>
	public static string SimpleAssemblyName(this _IAssemblyMetadata assemblyMetadata)
	{
		Guard.ArgumentNotNull(assemblyMetadata);
		Guard.ArgumentNotNullOrEmpty(assemblyMetadata.AssemblyName);

		var parsedAssemblyName = new AssemblyName(assemblyMetadata.AssemblyName);
		Guard.ArgumentNotNullOrEmpty(() => string.Format(CultureInfo.CurrentCulture, "{0}.{1} must include a name component", nameof(assemblyMetadata), nameof(_IAssemblyMetadata.AssemblyName)), parsedAssemblyName.Name, "assemblyMetadata.AssemblyName");

		return parsedAssemblyName.Name;
	}
}
