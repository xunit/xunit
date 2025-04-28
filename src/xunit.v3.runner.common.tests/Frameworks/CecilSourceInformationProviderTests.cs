using System.IO;
using System.Runtime.InteropServices;
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
		// We test for range here, because release PDBs can be slightly unpredictable, especially on Mono
		Assert.InRange(sourceInformation.SourceLine ?? -1, 1, 0xFEEFED);
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
#if NETFRAMEWORK
		// Mono sometimes includes symbols for mscorlib.dll
		Assert.SkipUnless(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), "This test is only supported on .NET Framework in Windows");
#endif

		var provider = CecilSourceInformationProvider.Create(typeof(string).Assembly.Location);

		Assert.IsType<NullSourceInformationProvider>(provider);
	}
}
