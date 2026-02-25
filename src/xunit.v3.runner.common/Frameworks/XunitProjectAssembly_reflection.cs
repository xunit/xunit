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
		TestCasesToRun = [];
	}

	internal XunitProjectAssembly(
		Assembly? assembly,
		string assemblyFileName,
		AssemblyMetadata assemblyMetadata,
		bool autoEnableExplicit,
		string? configFileName,
		TestAssemblyConfiguration configuration,
		XunitProject project,
		List<string> testCaseIDsToRun,
		List<string> testCasesToRun)
	{
		Assembly = assembly;
		AssemblyFileName = Guard.ArgumentNotNull(assemblyFileName);
		AssemblyMetadata = Guard.ArgumentNotNull(assemblyMetadata);
		AutoEnableExplicit = autoEnableExplicit;
		ConfigFileName = configFileName;
		Configuration = Guard.ArgumentNotNull(configuration);
		Project = Guard.ArgumentNotNull(project);
		TestCaseIDsToRun = testCaseIDsToRun;
		TestCasesToRun = testCasesToRun;
	}

	/// <summary>
	/// Gets a list of test case IDs to be run. If this list and <see cref="TestCasesToRun"/> are empty, then all test
	/// cases (that match the filters) will be run.
	/// </summary>
	public List<string> TestCaseIDsToRun { get; }

	/// <summary>
	/// Gets a list of serialized test cases to be run. If this list and <see cref="TestCaseIDsToRun"/> are empty, then
	/// all test cases (that match the filters) will be run.
	/// </summary>
	public List<string> TestCasesToRun { get; }
}
