using System.IO;
using Xunit;
using Xunit.Runner.Common;

public class CecilSourceInformationProviderTests
{
	[Fact]
	public void CanRetrieveSourceInformation()
	{
		var provider = CecilSourceInformationProvider.Create(typeof(CecilSourceInformationProviderTests).Assembly.Location);

		var sourceInformation = provider.GetSourceInformation(typeof(CecilSourceInformationProviderTests).FullName, nameof(CanRetrieveSourceInformation));

		Assert.Equal("CecilSourceInformationProviderTests.cs", Path.GetFileName(sourceInformation.SourceFile));
#if DEBUG
		Assert.Equal(9, sourceInformation.SourceLine);
#else
		Assert.Equal(10, sourceInformation.SourceLine);
#endif
	}

	[Fact]
	public void AssemblyNotFound_ReturnsNullProvider()
	{
		var provider = CecilSourceInformationProvider.Create("/foo/bar/baz.dll");

		Assert.IsType<NullSourceInformationProvider>(provider);
	}

	[Fact]
	public void AssemblyWithoutDebugSymbols_ReturnsNullProvider()
	{
		var provider = CecilSourceInformationProvider.Create(typeof(string).Assembly.Location);

		Assert.IsType<NullSourceInformationProvider>(provider);
	}
}
