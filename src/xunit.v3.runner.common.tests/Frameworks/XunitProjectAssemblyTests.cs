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
			var project = new XunitProject();
			var projectAssembly = new XunitProjectAssembly(project)
			{
				AssemblyFilename = assemblyFileName
			};

			var displayName = projectAssembly.AssemblyDisplayName;

			Assert.Equal("<dynamic>", displayName);
		}

		[Fact]
		public void WhenAssemblyFilenameIsSet_ReturnsFileNameWithoutExtension()
		{
			var project = new XunitProject();
			var projectAssembly = new XunitProjectAssembly(project)
			{
				AssemblyFilename = "/foo/bar/baz.exe"
			};

			var displayName = projectAssembly.AssemblyDisplayName;

			Assert.Equal("baz", displayName);
		}
	}
}
