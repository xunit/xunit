using System;

namespace Xunit.Sdk;

/// <summary>
/// Test options names
/// </summary>
public static class TestOptionsNames
{
	/// <summary>
	/// Test options names used with <see cref="ITestFrameworkDiscoveryOptions"/>.
	/// </summary>
	public static class Discovery
	{
		/// <summary>
		/// The culture to be used for discovery. <c>null</c> means the default system culture,
		/// <see cref="string.Empty"/> means the invariant culture, and any other value is assumed
		/// to be a culture name that the system understands.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="string"/><br/>
		/// Consumed by: v3
		/// </remarks>
		public static readonly string Culture = "xunit.discovery.Culture";

		/// <summary>
		/// Set to <c>true</c> to enable display of diagnostic messages.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="bool"/><br/>
		/// Consumed by: v2, v3
		/// </remarks>
		public static readonly string DiagnosticMessages = "xunit.discovery.DiagnosticMessages";

		/// <summary>
		/// Set to <c>true</c> to include source information during discovery, when possible. (Note that
		/// most source information is applied by the runner, not the discoverer, because it utilizes the
		/// <c>DiaSession</c> support provided by Visual Studio, which means it's applied after the fact
		/// by <c>xunit.runner.visualstudio</c>. This flag, then, is a signal for custom test frameworks
		/// that may be able to provide source information via some other mechanism.)
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="bool"/><br/>
		/// Consumed by: v3
		/// </remarks>
		public static readonly string IncludeSourceInformation = "xunit.discovery.IncludeSourceInformation";

		/// <summary>
		/// Set to <c>true</c> to enable display of internal diagnostic messages.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="bool"/><br/>
		/// Consumed by: v2, v3
		/// </remarks>
		public static readonly string InternalDiagnosticMessages = "xunit.discovery.InternalDiagnosticMessages";

		/// <summary>
		/// A flag which indicates how the default test method display name is calculated.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="TestMethodDisplay"/><br/>
		/// Consumed by: v2, v3
		/// </remarks>
		public static readonly string MethodDisplay = "xunit.discovery.MethodDisplay";

		/// <summary>
		/// A flag which indicates how the test method display name calculation can be modified by special
		/// naming patterns.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="TestMethodDisplayOptions"/><br/>
		/// Consumed by: v2, v3
		/// </remarks>
		public static readonly string MethodDisplayOptions = "xunit.discovery.MethodDisplayOptions";

		/// <summary>
		/// Set to <c>true</c> to enable pre-enumeration of theories during discovery.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="bool"/><br/>
		/// Consumed by: v2, v3
		/// </remarks>
		public static readonly string PreEnumerateTheories = "xunit.discovery.PreEnumerateTheories";

		/// <summary>
		/// Set to <c>true</c> to enable synchronous message reporting; set to <c>false</c> to enable
		/// asynchronous message reporting. Synchronous in this case means the system will wait for the
		/// runner to process a message before delivering the next one.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="bool"/><br/>
		/// Consumed by: v2, v3
		/// </remarks>
		public static readonly string SynchronousMessageReporting = "xunit.discovery.SynchronousMessageReporting";
	}

	/// <summary>
	/// Test options names used with <see cref="ITestFrameworkExecutionOptions"/>.
	/// </summary>
	public static class Execution
	{
		/// <summary>
		/// The culture to be used for execution. <c>null</c> means the default system culture,
		/// <see cref="string.Empty"/> means the invariant culture, and any other value is assumed
		/// to be a culture name that the system understands.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="string"/><br/>
		/// Consumed by: v3
		/// </remarks>
		public static readonly string Culture = "xunit.execution.Culture";

		/// <summary>
		/// Set to <c>true</c> to enable display of diagnostic messages.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="bool"/><br/>
		/// Consumed by: v2, v3
		/// </remarks>
		public static readonly string DiagnosticMessages = "xunit.execution.DiagnosticMessages";

		/// <summary>
		/// Set to <c>true</c> to disable running tests in parallel.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="bool"/><br/>
		/// Consumed by: v2, v3
		/// </remarks>
		public static readonly string DisableParallelization = "xunit.execution.DisableParallelization";

		/// <summary>
		/// Gets a flag which indicates the user's desire to run explicit tests.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="Sdk.ExplicitOption"/><br/>
		/// Consumed by: v3
		/// </remarks>
		public static readonly string ExplicitOption = "xunit.execution.ExplicitOption";

		/// <summary>
		/// Set to <c>true</c> to convert skipped tests into failed tests.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="bool"/><br/>
		/// Consumed by: v3
		/// </remarks>
		public static readonly string FailSkips = "xunit.execution.FailSkips";

		/// <summary>
		/// Set to <c>true</c> to convert passing tests with warnings into failed tests.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="bool"/><br/>
		/// Consumed by: v3
		/// </remarks>
		public static readonly string FailTestsWithWarnings = "xunit.execution.FailTestsWithWarnings";

		/// <summary>
		/// Set to <c>true</c> to enable display of internal diagnostic messages.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="bool"/><br/>
		/// Consumed by: v2, v3
		/// </remarks>
		public static readonly string InternalDiagnosticMessages = "xunit.execution.InternalDiagnosticMessages";

		/// <summary>
		/// Sets the maximum number of parallel threads to use during execution. Set to <c>-1</c>
		/// to run with unlimited threads; set to <c>0</c> to use the system default (equal to
		/// <see cref="Environment.ProcessorCount"/>; set to any other positive integer to use
		/// that number of threads.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="int"/><br/>
		/// Consumed by: v2, v3
		/// </remarks>
		public static readonly string MaxParallelThreads = "xunit.execution.MaxParallelThreads";

		/// <summary>
		/// Set the algorithm to use for parallelization.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="Sdk.ParallelAlgorithm"/><br/>
		/// Consumed by: v2, v3
		/// </remarks>
		public static readonly string ParallelAlgorithm = "xunit.execution.ParallelAlgorithm";

		/// <summary>
		/// Set the seed to use for randomization. When unset (or set to <c>null</c>), will use the default
		/// system-computed seed.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="int"/><br/>
		/// Consumed by: v3
		/// </remarks>
		public static readonly string Seed = "xunit.execution.Seed";

		/// <summary>
		/// Set to <c>true</c> to show output live while tests are running, in addition to showing collected output
		/// when the test has finished.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="bool"/><br/>
		/// Consumed by: v2, v3
		/// </remarks>
		public static readonly string ShowLiveOutput = "xunit.execution.ShowLiveOutput";

		/// <summary>
		/// Set to <c>true</c> to attempt to stop execution as soon the first test fails.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="bool"/><br/>
		/// Consumed by: v2, v3
		/// </remarks>
		public static readonly string StopOnFail = "xunit.execution.StopOnFail";

		/// <summary>
		/// Set to <c>true</c> to enable synchronous message reporting; set to <c>false</c> to enable
		/// asynchronous message reporting. Synchronous in this case means the system will wait for the
		/// runner to process a message before delivering the next one.
		/// </summary>
		/// <remarks>
		/// Value type: <see cref="bool"/><br/>
		/// Consumed by: v2, v3
		/// </remarks>
		public static readonly string SynchronousMessageReporting = "xunit.execution.SynchronousMessageReporting";
	}
}
