using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents an assembly in an <see cref="XunitProject"/>.
/// </summary>
public class XunitProjectAssembly
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
		TestCasesToRun = [];
	}

	internal XunitProjectAssembly(
		Assembly? assembly,
		string assemblyFileName,
		AssemblyMetadata assemblyMetadata,
		string? configFileName,
		TestAssemblyConfiguration configuration,
		XunitProject project,
		List<string> testCasesToRun)
	{
		Assembly = assembly;
		AssemblyFileName = Guard.ArgumentNotNull(assemblyFileName);
		AssemblyMetadata = Guard.ArgumentNotNull(assemblyMetadata);
		ConfigFileName = configFileName;
		Configuration = Guard.ArgumentNotNull(configuration);
		Project = Guard.ArgumentNotNull(project);
		TestCasesToRun = testCasesToRun;
	}

	/// <summary>
	/// Gets or sets the assembly under test. May be <c>null</c> when the test assembly is not
	/// loaded into the current <see cref="AppDomain"/>.
	/// </summary>
	public Assembly? Assembly { get; set; }

	/// <summary>
	/// Gets the assembly display name.
	/// </summary>
	public string AssemblyDisplayName =>
		Path.GetFileNameWithoutExtension(AssemblyFileName);

	/// <summary>
	/// Gets or sets the assembly file name.
	/// </summary>
	public string AssemblyFileName { get; set; }

	/// <summary>
	/// Gets or sets the metadata about the assembly.
	/// </summary>
	public AssemblyMetadata AssemblyMetadata { get; set; }

	/// <summary>
	/// Gets or sets the config file name.
	/// </summary>
	public string? ConfigFileName { get; set; }

	/// <summary>
	/// Gets the configuration values for the test assembly.
	/// </summary>
	public TestAssemblyConfiguration Configuration { get; }

	/// <summary>
	/// Gets an identifier for the current assembly. This is guaranteed to be unique, but not necessarily repeatable
	/// across runs (because it relies on <see cref="Assembly.GetHashCode"/>).
	/// </summary>
	public string Identifier =>
		ConfigFileName is null
			? AssemblyFileName
			: string.Format(CultureInfo.InvariantCulture, "{0} :: {1}", AssemblyFileName, ConfigFileName);

	/// <summary>
	/// Gets the project that this project assembly belongs to.
	/// </summary>
	public XunitProject Project { get; }

	/// <summary>
	/// Gets a list of serialized test cases to be run. If the list is empty, then all test cases
	/// (that match the filters) will be run.
	/// </summary>
	public List<string> TestCasesToRun { get; }

	/// <summary>
	/// Create an instance of <see cref="XunitProjectAssembly"/> populated by front controller settings.
	/// </summary>
	/// <param name="settings">The settings</param>
	public XunitProjectAssembly WithSettings(FrontControllerFindSettings settings) =>
		new(
			Assembly,
			AssemblyFileName,
			AssemblyMetadata,
			ConfigFileName,
			new(
				Configuration.AppDomain,
				Configuration.AssertEquivalentMaxDepth,
				Guard.ArgumentNotNull(settings).Options.GetCulture() ?? Configuration.Culture,
				settings.Options.GetDiagnosticMessages() ?? Configuration.DiagnosticMessages,
				Configuration.ExplicitOption,
				Configuration.FailSkips,
				Configuration.FailTestsWithWarnings,
				settings.Filters ?? Configuration.Filters,
				settings.Options.GetIncludeSourceInformation() ?? Configuration.IncludeSourceInformation,
				settings.Options.GetInternalDiagnosticMessages() ?? Configuration.InternalDiagnosticMessages,
				Configuration.LongRunningTestSeconds,
				Configuration.MaxParallelThreads,
				settings.Options.GetMethodDisplay() ?? Configuration.MethodDisplay,
				settings.Options.GetMethodDisplayOptions() ?? Configuration.MethodDisplayOptions,
				Configuration.ParallelAlgorithm,
				Configuration.ParallelizeAssembly,
				Configuration.ParallelizeTestCollections,
				settings.Options.GetPreEnumerateTheories() ?? Configuration.PreEnumerateTheories,
				settings.Options.GetPrintMaxEnumerableLength() ?? Configuration.PrintMaxEnumerableLength,
				settings.Options.GetPrintMaxObjectDepth() ?? Configuration.PrintMaxObjectDepth,
				settings.Options.GetPrintMaxObjectMemberCount() ?? Configuration.PrintMaxObjectMemberCount,
				settings.Options.GetPrintMaxStringLength() ?? Configuration.PrintMaxStringLength,
				Configuration.Seed,
				Configuration.ShadowCopy,
				Configuration.ShadowCopyFolder,
				Configuration.ShowLiveOutput,
				Configuration.StopOnFail,
				settings.Options.GetSynchronousMessageReporting() ?? Configuration.SynchronousMessageReporting
			),
			XunitProject.WithLaunchOptions(Project, settings.LaunchOptions),
			TestCasesToRun
		);

	/// <summary>
	/// Create an instance of <see cref="XunitProjectAssembly"/> populated by front controller settings.
	/// </summary>
	/// <param name="settings">The settings</param>
	public XunitProjectAssembly WithSettings(FrontControllerFindAndRunSettings settings) =>
		new(
			Assembly,
			AssemblyFileName,
			AssemblyMetadata,
			ConfigFileName,
			new(
				Configuration.AppDomain,
				Guard.ArgumentNotNull(settings).ExecutionOptions.GetAssertEquivalentMaxDepth() ?? Configuration.AssertEquivalentMaxDepth,
				settings.ExecutionOptions.GetCulture() ?? settings.DiscoveryOptions.GetCulture() ?? Configuration.Culture,
				settings.ExecutionOptions.GetDiagnosticMessages() ?? settings.DiscoveryOptions.GetDiagnosticMessages() ?? Configuration.DiagnosticMessages,
				settings.ExecutionOptions.GetExplicitOption() ?? Configuration.ExplicitOption,
				settings.ExecutionOptions.GetFailSkips() ?? Configuration.FailSkips,
				settings.ExecutionOptions.GetFailTestsWithWarnings() ?? Configuration.FailTestsWithWarnings,
				settings.Filters ?? Configuration.Filters,
				settings.DiscoveryOptions.GetIncludeSourceInformation() ?? Configuration.IncludeSourceInformation,
				settings.ExecutionOptions.GetInternalDiagnosticMessages() ?? settings.DiscoveryOptions.GetInternalDiagnosticMessages() ?? Configuration.InternalDiagnosticMessages,
				Configuration.LongRunningTestSeconds,
				settings.ExecutionOptions.GetMaxParallelThreads() ?? Configuration.MaxParallelThreads,
				settings.DiscoveryOptions.GetMethodDisplay() ?? Configuration.MethodDisplay,
				settings.DiscoveryOptions.GetMethodDisplayOptions() ?? Configuration.MethodDisplayOptions,
				settings.ExecutionOptions.GetParallelAlgorithm() ?? Configuration.ParallelAlgorithm,
				settings.ExecutionOptions.GetDisableParallelization() switch
				{
					true => false,
					false => true,
					_ => Configuration.ParallelizeAssembly,
				},
				settings.ExecutionOptions.GetDisableParallelization() switch
				{
					true => false,
					false => true,
					_ => Configuration.ParallelizeTestCollections,
				},
				settings.DiscoveryOptions.GetPreEnumerateTheories() ?? Configuration.PreEnumerateTheories,
				settings.ExecutionOptions.GetPrintMaxEnumerableLength() ?? settings.DiscoveryOptions.GetPrintMaxEnumerableLength() ?? Configuration.PrintMaxEnumerableLength,
				settings.ExecutionOptions.GetPrintMaxObjectDepth() ?? settings.DiscoveryOptions.GetPrintMaxObjectDepth() ?? Configuration.PrintMaxObjectDepth,
				settings.ExecutionOptions.GetPrintMaxObjectMemberCount() ?? settings.DiscoveryOptions.GetPrintMaxObjectMemberCount() ?? Configuration.PrintMaxObjectMemberCount,
				settings.ExecutionOptions.GetPrintMaxStringLength() ?? settings.DiscoveryOptions.GetPrintMaxStringLength() ?? Configuration.PrintMaxStringLength,
				settings.ExecutionOptions.GetSeed() ?? Configuration.Seed,
				Configuration.ShadowCopy,
				Configuration.ShadowCopyFolder,
				settings.ExecutionOptions.GetShowLiveOutput() ?? Configuration.ShowLiveOutput,
				settings.ExecutionOptions.GetStopOnTestFail() ?? Configuration.StopOnFail,
				settings.ExecutionOptions.GetSynchronousMessageReporting() ?? settings.DiscoveryOptions.GetSynchronousMessageReporting() ?? Configuration.SynchronousMessageReporting
			),
			XunitProject.WithLaunchOptions(Project, settings.LaunchOptions),
			TestCasesToRun
		);

	/// <summary>
	/// Create an instance of <see cref="XunitProjectAssembly"/> populated by front controller settings.
	/// </summary>
	/// <param name="settings">The settings</param>
	public XunitProjectAssembly WithSettings(FrontControllerRunSettings settings) =>
		new(
			Assembly,
			AssemblyFileName,
			AssemblyMetadata,
			ConfigFileName,
			new(
				Configuration.AppDomain,
				Guard.ArgumentNotNull(settings).Options.GetAssertEquivalentMaxDepth() ?? Configuration.AssertEquivalentMaxDepth,
				settings.Options.GetCulture() ?? Configuration.Culture,
				settings.Options.GetDiagnosticMessages() ?? Configuration.DiagnosticMessages,
				settings.Options.GetExplicitOption() ?? Configuration.ExplicitOption,
				settings.Options.GetFailSkips() ?? Configuration.FailSkips,
				settings.Options.GetFailTestsWithWarnings() ?? Configuration.FailTestsWithWarnings,
				Configuration.Filters,
				Configuration.IncludeSourceInformation,
				settings.Options.GetInternalDiagnosticMessages() ?? Configuration.InternalDiagnosticMessages,
				Configuration.LongRunningTestSeconds,
				settings.Options.GetMaxParallelThreads() ?? Configuration.MaxParallelThreads,
				Configuration.MethodDisplay,
				Configuration.MethodDisplayOptions,
				settings.Options.GetParallelAlgorithm() ?? Configuration.ParallelAlgorithm,
				settings.Options.GetDisableParallelization() switch
				{
					true => false,
					false => true,
					_ => Configuration.ParallelizeAssembly,
				},
				settings.Options.GetDisableParallelization() switch
				{
					true => false,
					false => true,
					_ => Configuration.ParallelizeTestCollections,
				},
				Configuration.PreEnumerateTheories,
				settings.Options.GetPrintMaxEnumerableLength() ?? Configuration.PrintMaxEnumerableLength,
				settings.Options.GetPrintMaxObjectDepth() ?? Configuration.PrintMaxObjectDepth,
				settings.Options.GetPrintMaxObjectMemberCount() ?? Configuration.PrintMaxObjectMemberCount,
				settings.Options.GetPrintMaxStringLength() ?? Configuration.PrintMaxStringLength,
				settings.Options.GetSeed() ?? Configuration.Seed,
				Configuration.ShadowCopy,
				Configuration.ShadowCopyFolder,
				settings.Options.GetShowLiveOutput() ?? Configuration.ShowLiveOutput,
				settings.Options.GetStopOnTestFail() ?? Configuration.StopOnFail,
				settings.Options.GetSynchronousMessageReporting() ?? Configuration.SynchronousMessageReporting
			),
			XunitProject.WithLaunchOptions(Project, settings.LaunchOptions),
			settings.SerializedTestCases.CastOrToList()
		);
}
