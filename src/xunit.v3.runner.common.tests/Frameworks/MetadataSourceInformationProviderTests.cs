using System.IO;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;

public class MetadataSourceInformationProviderTests
{
	[Fact]
	public void CanRetrieveSourceInformation()
	{
		var provider = MetadataSourceInformationProviderHelper.CreateForTesting(typeof(MetadataSourceInformationProviderTests).Assembly.Location);

		var sourceInformation = provider.GetSourceInformation(typeof(MetadataSourceInformationProviderTests).FullName, nameof(CanRetrieveSourceInformation));

		Assert.Equal("MetadataSourceInformationProviderTests.cs", Path.GetFileName(sourceInformation.SourceFile));
#if DEBUG
		Assert.Equal(10, sourceInformation.SourceLine);
#else
		// We test for range here, because release PDBs can be slightly unpredictable, especially on Mono
		Assert.InRange(sourceInformation.SourceLine ?? -1, 1, 0xFEEFED);
#endif
	}

	[Fact]
	public void AssemblyNotFound_ReturnsNullProvider()
	{
		var provider = MetadataSourceInformationProviderHelper.CreateForTesting("/foo/bar/baz.dll");

		Assert.IsType<NullSourceInformationProvider>(provider);
	}
}
