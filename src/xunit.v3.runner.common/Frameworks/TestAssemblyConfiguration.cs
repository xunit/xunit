using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents the configuration items set in the configuration file of a test assembly.
/// Should be read with the <see cref="T:Xunit.Runner.Common.ConfigReader"/> class.
/// </summary>
public class TestAssemblyConfiguration
{
	/// <summary>
	/// Initalizes a new instance of the <see cref="TestAssemblyConfiguration"/> class.
	/// </summary>
	public TestAssemblyConfiguration() =>
		Filters = new();

	internal TestAssemblyConfiguration(
		AppDomainSupport? appDomain,
		int? assertEquivalentMaxDepth,
		string? culture,
		bool? diagnosticMessages,
		ExplicitOption? explicitOption,
		bool? failSkips,
		bool? failTestsWithWarnings,
		XunitFilters filters,
		bool? includeSourceInformation,
		bool? internalDiagnosticMessages,
		int? longRunningTestSeconds,
		int? maxParallelThreads,
		TestMethodDisplay? methodDisplay,
		TestMethodDisplayOptions? methodDisplayOptions,
		ParallelAlgorithm? parallelAlgorithm,
		bool? parallelizeAssembly,
		ParallelMode? parallelMode,
		bool? preEnumerateTheories,
		int? printMaxEnumerableLength,
		int? printMaxObjectDepth,
		int? printMaxObjectMemberCount,
		int? printMaxStringLength,
		int? seed,
		bool? shadowCopy,
		string? shadowCopyFolder,
		bool? showLiveOutput,
		int? shutdownForegroundThreadWaitSeconds,
		bool? stopOnFail,
		bool? synchronousMessageReporting)
	{
		AppDomain = appDomain;
		AssertEquivalentMaxDepth = assertEquivalentMaxDepth;
		Culture = culture;
		DiagnosticMessages = diagnosticMessages;
		ExplicitOption = explicitOption;
		FailSkips = failSkips;
		FailTestsWithWarnings = failTestsWithWarnings;
		Filters = Guard.ArgumentNotNull(filters);
		IncludeSourceInformation = includeSourceInformation;
		InternalDiagnosticMessages = internalDiagnosticMessages;
		LongRunningTestSeconds = longRunningTestSeconds;
		MaxParallelThreads = maxParallelThreads;
		MethodDisplay = methodDisplay;
		MethodDisplayOptions = methodDisplayOptions;
		ParallelAlgorithm = parallelAlgorithm;
		ParallelizeAssembly = parallelizeAssembly;
		ParallelMode = parallelMode;
		PreEnumerateTheories = preEnumerateTheories;
		PrintMaxEnumerableLength = printMaxEnumerableLength;
		PrintMaxObjectDepth = printMaxObjectDepth;
		PrintMaxObjectMemberCount = printMaxObjectMemberCount;
		PrintMaxStringLength = printMaxStringLength;
		Seed = seed;
		ShadowCopy = shadowCopy;
		ShadowCopyFolder = shadowCopyFolder;
		ShowLiveOutput = showLiveOutput;
		ShutdownForegroundThreadWaitSeconds = shutdownForegroundThreadWaitSeconds;
		StopOnFail = stopOnFail;
		SynchronousMessageReporting = synchronousMessageReporting;
	}

	/// <summary>
	/// Gets or sets a flag indicating whether an app domain should be used to discover and run tests.
	/// </summary>
	public AppDomainSupport? AppDomain { get; set; }

	/// <summary>
	/// Gets or sets a flag indicating whether an app domain should be used to discover and run tests.
	/// If the flag is not set, returns the default value (<see cref="AppDomainSupport.IfAvailable"/>).
	/// </summary>
	public AppDomainSupport AppDomainOrDefault => AppDomain ?? AppDomainSupport.IfAvailable;

	/// <summary>
	/// Gets a value which indicates the maximum object depth to compare when using <c>Assert.Equivalent</c>.
	/// </summary>
	public int? AssertEquivalentMaxDepth { get; set; }

	/// <summary>
	/// Gets or sets the desired culture to run the tests under. Use <see langword="null"/> (default) to
	/// indicate that we should use the default OS culture; use an empty string to indicate that
	/// we should use the invariant culture; or use any culture value that is valid for
	/// calling <see cref="CultureInfo(string)"/>.
	/// </summary>
	public string? Culture { get; set; }

	/// <summary>
	/// Gets or sets a flag indicating that the end user wants diagnostic messages
	/// from the test framework.
	/// </summary>
	public bool? DiagnosticMessages { get; set; }

	/// <summary>
	/// Gets a flag indicating that the end user wants diagnostic messages
	/// from the test framework. If the flag is not set, returns the default
	/// value (<see langword="false"/>).
	/// </summary>
	public bool DiagnosticMessagesOrDefault => DiagnosticMessages ?? false;

	/// <summary>
	/// Gets or sets a flag indicating how explicit tests should be handled.
	/// </summary>
	public ExplicitOption? ExplicitOption { get; set; }

	/// <summary>
	/// Gets a flag indicating how explicit tests should be handled. If the flag
	/// isn't set, returns the default value (<see cref="ExplicitOption.Off"/>).
	/// </summary>
	public ExplicitOption ExplicitOptionOrDefault => ExplicitOption ?? Sdk.ExplicitOption.Off;

	/// <summary>
	/// Gets or sets a flag indicating that skipped tests should be converted into
	/// failed tests.
	/// </summary>
	public bool? FailSkips { get; set; }

	/// <summary>
	/// Gets a flag indicating that skipped tests should be converted into failed
	/// tests. If the flag is not set, returns the default value (<see langword="false"/>).
	/// </summary>
	public bool FailSkipsOrDefault => FailSkips ?? false;

	/// <summary>
	/// Gets or sets a flag indicating that passing tests with warnings should be
	/// converted into failed tests.
	/// </summary>
	public bool? FailTestsWithWarnings { get; set; }

	/// <summary>
	/// Gets or sets a flag indicating that passing tests with warnings should be
	/// converted into failed tests. If the flag is not set, returns the default
	/// value (<see langword="false"/>).
	/// </summary>
	public bool FailTestsWithWarningsOrDefault => FailTestsWithWarnings ?? false;

	/// <summary>
	/// Gets the list of filters used during test discovery.
	/// </summary>
	public XunitFilters Filters { get; }

	/// <summary>
	/// Gets or sets a flag indicating that discovery should include source information
	/// for the test cases.
	/// </summary>
	public bool? IncludeSourceInformation { get; set; }

	/// <summary>
	/// Gets a flag indicating that discovery should include source information for the
	/// test cases. If the flag is not set, returns the default value (<see langword="false"/>).
	/// </summary>
	public bool IncludeSourceInformationOrDefault => IncludeSourceInformation ?? false;

	/// <summary>
	/// Gets or sets a flag indicating that the end user wants internal diagnostic messages
	/// from the test framework.
	/// </summary>
	public bool? InternalDiagnosticMessages { get; set; }

	/// <summary>
	/// Gets a flag indicating that the end user wants internal diagnostic messages
	/// from the test framework. If the flag is not set, returns the default
	/// value (<see langword="false"/>).
	/// </summary>
	public bool InternalDiagnosticMessagesOrDefault => InternalDiagnosticMessages ?? false;

	/// <summary>
	/// Gets the number of seconds that a test can run before being considered "long running". Set to a positive
	/// value to enable the feature.
	/// </summary>
	public int? LongRunningTestSeconds { get; set; }

	/// <summary>
	/// Gets the number of seconds that a test can run before being considered "long running". If the value is not
	/// set, returns the default value (-1).
	/// </summary>
	public int LongRunningTestSecondsOrDefault => LongRunningTestSeconds ?? -1;

	/// <summary>
	/// Gets or sets the maximum number of thread to use when parallelizing this assembly. A value of <see langword="null"/>
	/// or 0 indicates that the default should be used (<see cref="Environment.ProcessorCount"/>); a value of
	/// -1 indicates that tests should run with an unlimited-sized thread pool.
	/// </summary>
	public int? MaxParallelThreads { get; set; }

	/// <summary>
	/// Gets the maximum number of thread to use when parallelizing this assembly.
	/// If the value is not set, returns the default value (<see cref="Environment.ProcessorCount"/>).
	/// </summary>
	public int MaxParallelThreadsOrDefault =>
		MaxParallelThreads is null or 0
			? Environment.ProcessorCount
			: MaxParallelThreads.Value;

	/// <summary>
	/// Gets or sets the default display name for test methods.
	/// </summary>
	public TestMethodDisplay? MethodDisplay { get; set; }

	/// <summary>
	/// Gets the default display name for test methods. If the value is not set, returns
	/// the default value (<see cref="TestMethodDisplay.ClassAndMethod"/>).
	/// </summary>
	public TestMethodDisplay MethodDisplayOrDefault => MethodDisplay ?? TestMethodDisplay.ClassAndMethod;

	/// <summary>
	/// Gets or sets the default display options for test methods.
	/// </summary>
	public TestMethodDisplayOptions? MethodDisplayOptions { get; set; }

	/// <summary>
	/// Gets the default display options for test methods. If the value is not set, returns
	/// the default value (<see cref="TestMethodDisplayOptions.None"/>).
	/// </summary>
	public TestMethodDisplayOptions MethodDisplayOptionsOrDefault => MethodDisplayOptions ?? TestMethodDisplayOptions.None;

	/// <summary>
	/// Gets or sets the algorithm to be used for parallelization.
	/// </summary>
	public ParallelAlgorithm? ParallelAlgorithm { get; set; }

	/// <summary>
	/// Gets or sets the algorithm to be used for parallelization.
	/// </summary>
	public ParallelAlgorithm ParallelAlgorithmOrDefault => ParallelAlgorithm ?? Sdk.ParallelAlgorithm.Conservative;

	/// <summary>
	/// Gets or sets a flag indicating that this assembly is safe to parallelize against
	/// other assemblies.
	/// </summary>
	public bool? ParallelizeAssembly { get; set; }

	/// <summary>
	/// Gets a flag indicating that this assembly is safe to parallelize against
	/// other assemblies. If the flag is not set, returns the default value (<see langword="false"/>).
	/// </summary>
	public bool ParallelizeAssemblyOrDefault => ParallelizeAssembly ?? false;

	/// <summary>
	/// Please read <see cref="ParallelMode"/> instead.
	/// This property will be removed in the next major version.
	/// </summary>
	[Obsolete("Please read ParallelMode instead. This property will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool? ParallelizeTestCollections
	{
		get => ParallelMode is null ? null : ParallelMode != Sdk.ParallelMode.None;
		set => ParallelMode = value switch
		{
			true => Sdk.ParallelMode.Collections,
			false => Sdk.ParallelMode.None,
			null => null,
		};
	}

	/// <summary>
	/// Please read <see cref="ParallelModeOrDefault"/> instead.
	/// This property will be removed in the next major version.
	/// </summary>
	[Obsolete("Please read ParallelModeOrDefault instead. This property will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ParallelizeTestCollectionsOrDefault => ParallelizeTestCollections ?? true;

	/// <summary>
	/// Gets or sets a flag that indicates how test parallelization is scheduled
	/// by default in this test assembly.
	/// </summary>
	public ParallelMode? ParallelMode { get; set; }

	/// <summary>
	/// Gets a flag that indicates how test parallelization is scheduled by default in this
	/// test assembly. If the flag is not set, returns the default value (<see cref="Sdk.ParallelMode.Collections"/>).
	/// </summary>
	public ParallelMode ParallelModeOrDefault => ParallelMode ?? Sdk.ParallelMode.Collections;

	/// <summary>
	/// Gets or sets a flag indicating whether theory data should be pre-enumerated during
	/// test discovery.
	/// </summary>
	public bool? PreEnumerateTheories { get; set; }

	/// <summary>
	/// Gets a value indicating the maximum length for printing collections.
	/// </summary>
	public int? PrintMaxEnumerableLength { get; set; }

	/// <summary>
	/// Gets a value indicating the maximum recursive depth when printing objects.
	/// </summary>
	public int? PrintMaxObjectDepth { get; set; }

	/// <summary>
	/// Gets a value indicating the maximum members to show when printing objects.
	/// </summary>
	public int? PrintMaxObjectMemberCount { get; set; }

	/// <summary>
	/// Gets a value indicating the maximum length for printing string values.
	/// </summary>
	public int? PrintMaxStringLength { get; set; }

	/// <summary>
	/// Gets or sets the seed value used for randomization. Only supported for v3 or later test assemblies.
	/// </summary>
	public int? Seed { get; set; }

	/// <summary>
	/// Gets or sets a flag indicating whether shadow copies should be used.
	/// </summary>
	public bool? ShadowCopy { get; set; }

	/// <summary>
	/// Gets a flag indicating whether shadow copies should be used. If the flag is not set,
	/// returns the default value (<see langword="true"/>).
	/// </summary>
	public bool ShadowCopyOrDefault => ShadowCopy ?? true;

	/// <summary>
	/// Gets or sets the folder to be used for shadow copy files. If the value is not set,
	/// the system defaults for shadow copying are used.
	/// </summary>
	public string? ShadowCopyFolder { get; set; }

	/// <summary>
	/// Gets or sets a flag indicating whether output from <see cref="T:Xunit.ITestOutputHelper"/> should be
	/// shown live as they're logged (in addition to being collected together after the test finishes).
	/// </summary>
	public bool? ShowLiveOutput { get; set; }

	/// <summary>
	/// Gets a flag indicating whether output from <see cref="T:Xunit.ITestOutputHelper"/> should be
	/// shown live as they're logged (in addition to being collected together after the test finishes).
	/// If the flag is not set, returns the default value (<see langword="false"/>).
	/// </summary>
	public bool ShowLiveOutputOrDefault => ShowLiveOutput ?? false;

	/// <summary>
	/// Gets or sets a number of seconds to wait for foreground threads to shut down at the end
	/// of test execution before failing the test run.
	/// </summary>
	/// <remarks>
	/// Must be a non-zero positive value.
	/// </remarks>
	public int? ShutdownForegroundThreadWaitSeconds { get; set; }

	/// <summary>
	/// Gets a number of seconds to wait for foreground threads to shut down at the end of
	/// test execution before failing the test run. If the value was not set (or set to a value
	/// less than <c>1</c>), will return the default value (<c>10</c>).
	/// </summary>
	public int ShutdownForegroundThreadWaitSecondsOrDefault =>
		ShutdownForegroundThreadWaitSeconds switch
		{
			> 0 => ShutdownForegroundThreadWaitSeconds.Value,
			_ => 10,
		};

	/// <summary>
	/// Gets or sets a flag indicating whether testing should stop on a failure.
	/// </summary>
	public bool? StopOnFail { get; set; }

	/// <summary>
	/// Gets a flag indicating whether testing should stop on a test failure. If the flag is not set,
	/// returns the default value (<see langword="false"/>).
	/// </summary>
	public bool StopOnFailOrDefault => StopOnFail ?? false;

	/// <summary>
	/// Gets or sets a flag indicating that synchronous message reporting is desired.
	/// </summary>
	public bool? SynchronousMessageReporting { get; set; }

	/// <summary>
	/// Gets a flag indicating that synchronous message reporting is desired. If the flag is not set,
	/// returns the default value (<see langword="false"/>).
	/// </summary>
	public bool SynchronousMessageReportingOrDefault => SynchronousMessageReporting ?? false;
}
