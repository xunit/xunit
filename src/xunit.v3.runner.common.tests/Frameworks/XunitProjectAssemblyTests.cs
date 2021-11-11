using Xunit;
using Xunit.Runner.Common;

public class XunitProjectAssemblyTests
{
	public class AssemblyDisplayName
	{
		[Fact]
		public void WhenAssemblyFilenameIsNotSet_ReturnsDynamic()
		{
			var project = new XunitProject();
			var projectAssembly = new XunitProjectAssembly(project);

			var displayName = projectAssembly.AssemblyDisplayName;

			Assert.Equal("<dynamic>", displayName);
		}

		[Fact]
		public void WhenAssemblyFilenameIsSet_ReturnsFileNameWithoutExtension()
		{
			var project = new XunitProject();
			var projectAssembly = new XunitProjectAssembly(project)
			{
				AssemblyFileName = "/foo/bar/baz.exe"
			};

			var displayName = projectAssembly.AssemblyDisplayName;

			Assert.Equal("baz", displayName);
		}
	}
}
