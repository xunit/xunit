using System;
using System.Globalization;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Represents the configuration items set in the configuration file of a test assembly.
	/// Should be read with the <see cref="T:Xunit.ConfigReader"/> class.
	/// </summary>
	public class TestAssemblyConfiguration
	{
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
		/// Gets or sets the desired culture to run the tests under. Use <c>null</c> (default) to
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
		/// value (<c>false</c>).
		/// </summary>
		public bool DiagnosticMessagesOrDefault => DiagnosticMessages ?? false;

		/// <summary>
		/// Gets or sets a flag indicating that skipped tests should be converted into
		/// failed tests.
		/// </summary>
		public bool? FailSkips { get; set; }

		/// <summary>
		/// Gets a flag indicating that skipped tests should be converted into failed
		/// tests. If the flag is not set, returns the default value (<c>false</c>).
		/// </summary>
		public bool FailSkipsOrDefault => FailSkips ?? false;

		/// <summary>
		/// Gets the list of filters used during test discovery.
		/// </summary>
		public XunitFilters Filters { get; } = new();

		/// <summary>
		/// Gets or sets a flag indicating that discovery should include source information
		/// for the test cases.
		/// </summary>
		public bool? IncludeSourceInformation { get; set; }

		/// <summary>
		/// Gets a flag indicating that discovery should include source information for the
		/// test cases. If the flag is not set, returns the default value (<c>false</c>).
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
		/// value (<c>false</c>).
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
		/// Gets or sets the maximum number of thread to use when parallelizing this assembly. A value of <c>null</c>
		/// or 0 indicates that the default should be used (<see cref="Environment.ProcessorCount"/>); a value of
		/// -1 indicates that tests should run with an unlimited-sized thread pool.
		/// </summary>
		public int? MaxParallelThreads { get; set; }

		/// <summary>
		/// Gets the maximum number of thread to use when parallelizing this assembly.
		/// If the value is not set, returns the default value (<see cref="Environment.ProcessorCount"/>).
		/// </summary>
		public int MaxParallelThreadsOrDefault =>
			MaxParallelThreads == null || MaxParallelThreads == 0
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
		/// Gets or sets a flag indicating that this assembly is safe to parallelize against
		/// other assemblies.
		/// </summary>
		public bool? ParallelizeAssembly { get; set; }

		/// <summary>
		/// Gets a flag indicating that this assembly is safe to parallelize against
		/// other assemblies. If the flag is not set, returns the default value (<c>false</c>).
		/// </summary>
		public bool ParallelizeAssemblyOrDefault => ParallelizeAssembly ?? false;

		/// <summary>
		/// Gets or sets a flag indicating that this test assembly wants to run test collections
		/// in parallel against one another.
		/// </summary>
		public bool? ParallelizeTestCollections { get; set; }

		/// <summary>
		/// Gets a flag indicating that this test assembly wants to run test collections
		/// in parallel against one another. If the flag is not set, returns the default
		/// value (<c>true</c>).
		/// </summary>
		public bool ParallelizeTestCollectionsOrDefault => ParallelizeTestCollections ?? true;

		/// <summary>
		/// Gets or sets a flag indicating whether theory data should be pre-enumerated during
		/// test discovery.
		/// </summary>
		public bool? PreEnumerateTheories { get; set; }

		/// <summary>
		/// Gets or sets a flag indicating whether shadow copies should be used.
		/// </summary>
		public bool? ShadowCopy { get; set; }

		/// <summary>
		/// Gets a flag indicating whether shadow copies should be used. If the flag is not set,
		/// returns the default value (<c>true</c>).
		/// </summary>
		public bool ShadowCopyOrDefault => ShadowCopy ?? true;

		/// <summary>
		/// Gets or sets the folder to be used for shadow copy files. If the value is not set,
		/// the system defaults for shadow copying are used.
		/// </summary>
		public string? ShadowCopyFolder { get; set; }

		/// <summary>
		/// Gets or sets a flag indicating whether testing should stop on a failure.
		/// </summary>
		public bool? StopOnFail { get; set; }

		/// <summary>
		/// Gets a flag indicating whether testing should stop on a test failure. If the flag is not set,
		/// returns the default value (<c>false</c>).
		/// </summary>
		public bool StopOnFailOrDefault => StopOnFail ?? false;
	}
}
