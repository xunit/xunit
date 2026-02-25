using System.ComponentModel;
using System.Reflection;

namespace Xunit.Runner.Common;

partial class XunitProjectAssembly
{
	/// <summary>Initializes a new instance of the <see cref="XunitProjectAssembly"/> class</summary>
	/// <param name="project">The project this assembly belongs to.</param>
	/// <param name="assemblyFileName">The assembly filename</param>
	/// <param name="assemblyMetadata">The assembly metadata</param>
	public XunitProjectAssembly(
		XunitProject project,
		string assemblyFileName,
		AssemblyMetadata assemblyMetadata)
	{
		AssemblyFileName = Guard.ArgumentNotNull(assemblyFileName);
		AssemblyMetadata = Guard.ArgumentNotNull(assemblyMetadata);
		Configuration = new();
		Project = Guard.ArgumentNotNull(project);
		TestCaseIDsToRun = [];
	}

	internal XunitProjectAssembly(
		Assembly? assembly,
		string assemblyFileName,
		AssemblyMetadata assemblyMetadata,
		bool autoEnableExplicit,
		string? configFileName,
		TestAssemblyConfiguration configuration,
		XunitProject project,
		List<string> testCaseIDsToRun)
	{
		Assembly = assembly;
		AssemblyFileName = Guard.ArgumentNotNull(assemblyFileName);
		AssemblyMetadata = Guard.ArgumentNotNull(assemblyMetadata);
		AutoEnableExplicit = autoEnableExplicit;
		ConfigFileName = configFileName;
		Configuration = Guard.ArgumentNotNull(configuration);
		Project = Guard.ArgumentNotNull(project);
		TestCaseIDsToRun = testCaseIDsToRun;
	}

	/// <summary>
	/// Gets a list of test case IDs to be run. If this list is empty, then all test cases (that match the filters)
	/// will be run.
	/// </summary>
	public List<string> TestCaseIDsToRun { get; }

	/// <summary>
	/// Serialization is not supported on Native AOT.
	/// </summary>
	[Obsolete("Serialization is not supported on Native AOT")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public List<string> TestCasesToRun =>
		throw new PlatformNotSupportedException("Serialization is not supported on Native AOT");
}
