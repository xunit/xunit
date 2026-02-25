using Xunit;
using Xunit.Sdk;

public class IAssemblyMetadataExtensionsTests
{
	public class SimpleAssemblyName
	{
		[Fact]
		public void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>("assemblyMetadata", () => IAssemblyMetadataExtensions.SimpleAssemblyName(null!));

			var metadata = TestData.AssemblyMetadata(assemblyName: null!);
			Assert.Throws<ArgumentNullException>("assemblyMetadata.AssemblyName", () => IAssemblyMetadataExtensions.SimpleAssemblyName(metadata));
		}

		[Fact]
		public void ReturnsSimpleName()
		{
			var metadata = TestData.AssemblyMetadata(assemblyName: "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

			var result = IAssemblyMetadataExtensions.SimpleAssemblyName(metadata);

			Assert.Equal("mscorlib", result);
		}
	}
}
