using Xunit;
using Xunit.Runner.Common;

public class XunitProjectAssemblyTests
{
	public class AssemblyDisplayName
	{
		[Theory]
		[InlineData(default(string))]
		[InlineData("")]
		public void WhenAssemblyFilenameIsNotSet_ReturnsDynamic(string? assemblyFileName)
		{
			var projectAssembly = new XunitProjectAssembly
			{
				AssemblyFilename = assemblyFileName
			};

			var displayName = projectAssembly.AssemblyDisplayName;

			Assert.Equal("<dynamic>", displayName);
		}

		[Fact]
		public void WhenAssemblyFilenameIsSet_ReturnsFileNameWithoutExtension()
		{
			var projectAssembly = new XunitProjectAssembly
			{
				AssemblyFilename = "/foo/bar/baz.exe"
			};

			var displayName = projectAssembly.AssemblyDisplayName;

			Assert.Equal("baz", displayName);
		}
	}
}
