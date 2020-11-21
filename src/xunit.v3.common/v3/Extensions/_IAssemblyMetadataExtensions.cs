using System.Reflection;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// Extension methods for <see cref="_IAssemblyMetadata"/>.
	/// </summary>
	public static class _IAssemblyMetadataExtensions
	{
		/// <summary>
		/// Computes the simple assembly name from <see cref="_IAssemblyMetadata.AssemblyName"/>.
		/// </summary>
		/// <returns>The simple assembly name.</returns>
		public static string SimpleAssemblyName(this _IAssemblyMetadata assemblyMetadata)
		{
			Guard.ArgumentNotNull(nameof(assemblyMetadata), assemblyMetadata);
			Guard.ArgumentValidNotNullOrEmpty(nameof(assemblyMetadata), $"{nameof(assemblyMetadata)}.{nameof(_IAssemblyMetadata.AssemblyName)}", assemblyMetadata.AssemblyName);

			var parsedAssemblyName = new AssemblyName(assemblyMetadata.AssemblyName);
			Guard.ArgumentValidNotNullOrEmpty(nameof(assemblyMetadata), $"{nameof(assemblyMetadata)}.{nameof(_IAssemblyMetadata.AssemblyName)} must include a name component", parsedAssemblyName.Name);

			return parsedAssemblyName.Name;
		}
	}
}
