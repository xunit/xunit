#nullable enable

namespace Xunit.Generators
{
	/// <summary>
	/// These are property values that we expect to be able to read from MSBuild.
	/// </summary>
	/// <remarks>
	/// In order to property support these properties, you should add the following to the <c>.props</c>
	/// file for the NuGet package of your source generators:<br />
	/// <br />
	/// <code>
	/// &lt;ItemGroup&gt;
	///   &lt;CompilerVisibleProperty Include="MSBuildProjectFullPath;XunitTestProjectAOT" /&gt;
	/// &lt;/ItemGroup&gt;
	/// </code>
	/// </remarks>
	public class XunitMSBuildProperties
	{
		/// <summary>
		/// Gets the value read from MSBuild variable <c>MSBuildProjectFullPath</c>.
		/// </summary>
		/// <remarks>
		/// Will return <see langword="null"/> if <c>&lt;CompilerVisibleProperty&gt;</c> was not set
		/// in the NuGet package's <c>.props</c> file.
		/// </remarks>
		public string? MSBuildProjectFullPath { get; set; }

		/// <summary>
		/// Gets the value read from MSBuild variable <c>XunitTestProjectAOT</c>.
		/// </summary>
		/// <remarks>
		/// Will return <see langword="null"/> if <c>&lt;CompilerVisibleProperty&gt;</c> was not set
		/// in the NuGet package's <c>.props</c> file.
		/// </remarks>
		public string? XunitTestProjectAOT { get; set; }
	}
}
