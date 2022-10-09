using Xunit;
using Xunit.Runner.Common;

public class XunitProjectAssemblyTests
{
	public class AssemblyDisplayName
	{
		[Fact]
		public void WhenAssemblyFileNameIsNotSet_AndAssemblyIsNotSet_ReturnsDynamic()
		{
			var project = new XunitProject();
			var projectAssembly = new XunitProjectAssembly(project);

			var displayName = projectAssembly.AssemblyDisplayName;

			Assert.Equal("<unnamed dynamic assembly>", displayName);
		}

		[Fact]
		public void WhenAssemblyFileNameIsNotSet_AndAssemblyIsSet_ReturnsAssemblyName()
		{
			var project = new XunitProject();
			var projectAssembly = new XunitProjectAssembly(project)
			{
				Assembly = typeof(XunitProjectAssemblyTests).Assembly
			};

			var displayName = projectAssembly.AssemblyDisplayName;

			Assert.Equal(typeof(XunitProjectAssemblyTests).Assembly.GetName().Name, displayName);
		}

		[Fact]
		public void WhenAssemblyFileNameIsSet_ReturnsFileNameWithoutExtension()
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
