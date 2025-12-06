#if !XUNIT_AOT

using NSubstitute;
using Xunit.Sdk;

// This file manufactures mocks of the message sink message interfaces and assocaited metadata.
public static partial class Mocks
{
	public static IAssemblyMetadata AssemblyMetadata(
		string assemblyName = TestData.DefaultAssemblyName,
		string assemblyPath = TestData.DefaultAssemblyPath,
		string? configFilePath = null)
	{
		var result = Substitute.For<IAssemblyMetadata, InterfaceProxy<IAssemblyMetadata>>();
		result.AssemblyName.Returns(assemblyName);
		result.AssemblyPath.Returns(assemblyPath);
		result.ConfigFilePath.Returns(configFilePath);
		return result;
	}
}

#endif
