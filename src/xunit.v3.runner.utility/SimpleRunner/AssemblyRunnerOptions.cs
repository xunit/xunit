using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.SimpleRunner;

/// <summary>
/// Represents the options used to execute a test assembly with <see cref="AssemblyRunner"/>.
/// </summary>
public class AssemblyRunnerOptions
{
	Action<MessageInfo>? onDiagnosticMessage;
	Action<MessageInfo>? onInternalDiagnosticsMessage;

	/// <summary>
	/// Initializes a new instance of the <see cref="AssemblyRunnerOptions"/> class.
	/// </summary>
	/// <param name="assemblyFileName">The test assembly file name</param>
	/// <param name="configFileName">The optional configuration file name</param>
	/// <param name="throwForConfigurationErrors">Set to throw if there are configuration problems</param>
	public AssemblyRunnerOptions(
		string assemblyFileName,
		string? configFileName = null,
		bool throwForConfigurationErrors = true)
	{
		Guard.FileExists(assemblyFileName);

		var assemblyMetadata =
			AssemblyUtility.GetAssemblyMetadata(assemblyFileName)
				?? throw new ArgumentException(
					string.Format(CultureInfo.CurrentCulture, "Assembly '{0}' is not a valid .NET assembly", assemblyFileName),
					nameof(assemblyFileName)
				);

		if (assemblyMetadata.XunitVersion == 0)
			throw new ArgumentException(
				string.Format(CultureInfo.CurrentCulture, "Assembly '{0}' does not appear to be an xUnit.net test assembly", assemblyFileName),
				nameof(assemblyFileName)
			);

		ProjectAssembly = new XunitProjectAssembly(new XunitProject(), assemblyFileName, assemblyMetadata) { ConfigFileName = configFileName };

		var warnings = throwForConfigurationErrors ? new List<string>() : null;
		ConfigReader.Load(ProjectAssembly.Configuration, ProjectAssembly.AssemblyFileName, ProjectAssembly.ConfigFileName, warnings);

		if (warnings?.Count > 0)
			throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"One or more configuration problems exist:{0}{1}",
					Environment.NewLine,
					string.Join(Environment.NewLine, warnings.Select(w => "* " + w))
				)
			);

		// Values computed based on event subscriptions
		ProjectAssembly.Configuration.DiagnosticMessages = false;
		ProjectAssembly.Configuration.InternalDiagnosticMessages = false;

		// Don't pre-enumerate, because we always call FindAndRun
		ProjectAssembly.Configuration.PreEnumerateTheories = false;
	}

	// ========== Test assembly information ==========

	/// <summary>
	/// Gets the test assembly file name.
	/// </summary>
	public string AssemblyFileName =>
		ProjectAssembly.AssemblyFileName;

	/// <summary>
	/// Gets the optional configuration file name.
	/// </summary>
	public string? ConfigFileName =>
		ProjectAssembly.ConfigFileName;

	internal XunitProjectAssembly ProjectAssembly { get; }

	/// <summary>
	/// Gets the target framework identifier the assembly was built against.
	/// </summary>
	public TargetFrameworkIdentifier TargetFrameworkIdentifier =>
		ProjectAssembly.AssemblyMetadata.TargetFrameworkIdentifier;

	/// <summary>
	/// Gets the version of the target framework identifier that the assembly was built against
	/// (i.e., <c>4.7.2</c> for <c>net472</c>, or <c>8.0</c> for <c>net8.0</c>).
	/// </summary>
	public Version TargetFrameworkVersion =>
		ProjectAssembly.AssemblyMetadata.TargetFrameworkVersion;

	/// <summary>
	/// Gets the product version of xUnit.net this assembly targets (<c>1</c> for <c>xunit</c> package version 1.x,
	/// <c>2</c> for <c>xunit</c> package version 2.x, or <c>3</c> for the <c>xunit.v3</c> package).
	/// </summary>
	public int XunitVersion =>
		ProjectAssembly.AssemblyMetadata.XunitVersion;

	// ========== Configuration options ==========

#if NETFRAMEWORK

	/// <summary>
	/// Determines how app domains are used.
	/// </summary>
	/// <remarks>
	/// <para>The default value is <see cref="AppDomainSupport.IfAvailable"/> .</para>
	/// <para><em>App domains are only valid for xUnit.net v1 and xUnit.net v2 test projects that target .NET Framework.</em></para>
	/// </remarks>
	public AppDomainSupport? AppDomain
	{
		get => ProjectAssembly.Configuration.AppDomain;
		set
		{
			GuardAppDomainCompatible(value, nameof(AppDomain));
			GuardValidValue(value, v => v.IsValid(), nameof(AppDomain));

			ProjectAssembly.Configuration.AppDomain = value;
		}
	}

#endif  // NETFRAMEWORK

	/// <summary>
	/// Set to influence the maximum depth when printing values for <c>Assert.Equivalent</c> failures.
	/// </summary>
	/// <remarks>
	/// <para>By default, the maximum depth to print is <c>50</c>.</para>
	/// <para><em>Setting <c>Assert.Equivalent</c> max depth is only valid for xUnit.net v3 test projects.</em></para>
	/// </remarks>
	public int? AssertEquivalentMaxDepth
	{
		get => ProjectAssembly.Configuration.AssertEquivalentMaxDepth;
		set
		{
			GuardMinimumXunitVersion(3, value, nameof(AssertEquivalentMaxDepth));
			GuardMinimumValue(1, value, nameof(AssertEquivalentMaxDepth), "must be a positive integer");

			ProjectAssembly.Configuration.AssertEquivalentMaxDepth = value;
		}
	}

	/// <summary>
	/// Set to override the culture which the unit tests run at.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	///   <item><see langword="null"/> = use the current culture</item>
	///   <item>Empty string = use the invariant culture</item>
	///   <item>Non-empty string = use the specified culture</item>
	/// </list>
	/// <para>By default, the current culture will be used.</para>
	/// <para><em>Setting culture is only valid for xUnit.net v3 test projects.</em></para>
	/// </remarks>
	public string? Culture
	{
		get => ProjectAssembly.Configuration.Culture;
		set
		{
			GuardMinimumXunitVersion(3, value, nameof(Culture));

			ProjectAssembly.Configuration.Culture = value;
		}
	}

	/// <summary>
	/// Indicates how explicit tests should be handled.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	///   <item><c>Off</c> = run only non-explicit tests</item>
	///   <item><c>Only</c> = run only explicit tests</item>
	///   <item><c>On</c> = run both explicit and non-explicit tests</item>
	/// </list>
	/// <para>The default value is <see cref="ExplicitOption.Off"/>.</para>
	/// <para><em>Explicit tests are only valid for xUnit.net v3 test projects.</em></para>
	/// </remarks>
	public ExplicitOption? ExplicitOption
	{
		get => ProjectAssembly.Configuration.ExplicitOption;
		set
		{
			GuardMinimumXunitVersion(3, value, nameof(ExplicitOption));
			GuardValidValue(value, v => v.IsValid(), nameof(ExplicitOption));

			ProjectAssembly.Configuration.ExplicitOption = value;
		}
	}

	/// <summary>
	/// Set to convert skipped tests into failing tests.
	/// </summary>
	public bool? FailSkips
	{
		get => ProjectAssembly.Configuration.FailSkips;
		set => ProjectAssembly.Configuration.FailSkips = value;
	}

	/// <summary>
	/// Set to convert passing tests with warnings into failing tests.
	/// </summary>
	/// <remarks>
	/// <em>Tests with warnings are only valid for xUnit.net v3 test projects.</em>
	/// </remarks>
	public bool? FailTestsWithWarnings
	{
		get => ProjectAssembly.Configuration.FailTestsWithWarnings;
		set
		{
			GuardMinimumXunitVersion(3, value, nameof(Culture));

			ProjectAssembly.Configuration.FailTestsWithWarnings = value;
		}
	}

	/// <summary>
	/// Gets the filters that can be used to run tests selectively.
	/// </summary>
	public XunitFilters Filters =>
		ProjectAssembly.Configuration.Filters;

	/// <summary>
	/// Determines how long a test must be running before a message is printed to indicate that the
	/// test is taking a long time to run.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	///   <item><c>0</c> = long running test detection is disabled</item>
	///   <item>Positive integer = number of seconds for a test to becoming "long running"</item>
	/// </list>
	/// <para>The default value is <c>0</c>.</para>
	/// </remarks>
	public int? LongRunningTestSeconds
	{
		get => ProjectAssembly.Configuration.LongRunningTestSeconds;
		set
		{
			GuardMinimumValue(0, value, nameof(LongRunningTestSeconds), "must be 0 or a positive integer");

			ProjectAssembly.Configuration.LongRunningTestSeconds = value;
		}
	}

	/// <summary>
	/// Indicates how many threads to use to run parallel tests (will have no affect
	/// if parallelism is turned off).
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	///   <item><c>-1</c> = no thread limit</item>
	///   <item><c>0</c> = default (<see cref="Environment.ProcessorCount"/>)</item>
	///   <item>Positive integer = thread limit</item>
	/// </list>
	/// <para>By default, the value is <see cref="Environment.ProcessorCount"/>.</para>
	/// <para><em>Parallelized tests are only valid for xUnit.net v2 or xUnit.net v3 test projects.</em></para>
	/// </remarks>
	public int? MaxParallelThreads
	{
		get => ProjectAssembly.Configuration.MaxParallelThreads;
		set
		{
			GuardMinimumXunitVersion(2, value, nameof(MaxParallelThreads));
			GuardMinimumValue(-1, value, nameof(MaxParallelThreads), "must be -1, 0, or a positive integer");

			ProjectAssembly.Configuration.MaxParallelThreads = value;
		}
	}

	/// <summary>
	/// Indicates how to display test methods.
	/// </summary>
	/// <remarks>
	/// <para>The default value is <see cref="TestMethodDisplay.ClassAndMethod"/>.</para>
	/// <para><em>Setting test method display is only valid for xUnit.net v2 or xUnit.net v3 test projects.</em></para>
	/// </remarks>
	public TestMethodDisplay? MethodDisplay
	{
		get => ProjectAssembly.Configuration.MethodDisplay;
		set
		{
			GuardMinimumXunitVersion(2, value, nameof(MethodDisplay));
			GuardValidValue(value, v => v.IsValid(), nameof(MethodDisplay));

			ProjectAssembly.Configuration.MethodDisplay = value;
		}
	}

	/// <summary>
	/// Indicates how to interpret test method names for display.
	/// </summary>
	/// <remarks>
	/// <para>By default, the value is <see cref="TestMethodDisplayOptions.None"/>.</para>
	/// <para><em>Setting test method display options is only valid for xUnit.net v2 or xUnit.net v3 test projects.</em></para>
	/// </remarks>
	public TestMethodDisplayOptions? MethodDisplayOptions
	{
		get => ProjectAssembly.Configuration.MethodDisplayOptions;
		set
		{
			GuardMinimumXunitVersion(2, value, nameof(MethodDisplayOptions));
			GuardValidValue(value, v => v.IsValid(), nameof(MethodDisplayOptions));

			ProjectAssembly.Configuration.MethodDisplayOptions = value;
		}
	}

	/// <summary>
	/// Indicates which algorithm to use when parallelizing tests. Setting this will have
	/// no effect if parallelism is turned off or if the max parallel threads is set to <c>-1</c>.
	/// </summary>
	/// <remarks>
	/// <para>The default value is <see cref="ParallelAlgorithm.Conservative"/>.</para>
	/// <para>For more information on the parallelism algorithms, see <see href="https://xunit.net/docs/running-tests-in-parallel#algorithms"/>.</para>
	/// <para><em>Setting the parallel algorithm is only valid for xUnit.net v2 (2.8.0+) and xUnit.net v3 test projects.</em></para>
	/// </remarks>
	public ParallelAlgorithm? ParallelAlgorithm
	{
		get => ProjectAssembly.Configuration.ParallelAlgorithm;
		set
		{
			GuardMinimumXunitVersion(2, value, nameof(ParallelAlgorithm));
			GuardValidValue(value, v => v.IsValid(), nameof(MethodDisplayOptions));

			ProjectAssembly.Configuration.ParallelAlgorithm = value;
		}
	}

	/// <summary>
	/// Indicates whether to run test collections in parallel.
	/// </summary>
	/// <remarks>
	/// <em>Parallelized test collections are only valid for xUnit.net v2 and xUnit.net v3 test projects.</em>
	/// </remarks>
	public bool? ParallelizeTestCollections
	{
		get => ProjectAssembly.Configuration.ParallelizeTestCollections;
		set
		{
			GuardMinimumXunitVersion(2, value, nameof(ParallelizeTestCollections));

			ProjectAssembly.Configuration.ParallelizeTestCollections = value;
		}
	}

	/// <summary>
	/// Set to influence the maximum length when printing collections.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	///   <item><c>0</c> = always print the full collection</item>
	///   <item>Positive integer = limit the printed collection size</item>
	/// </list>
	/// <para>By default, the maximum length when printing collections is <c>5</c>.</para>
	/// <para><em>Setting the maximum enumerable length is only valid for xUnit.net v3 test projects.</em></para>
	/// </remarks>
	public int? PrintMaxEnumerableLength
	{
		get => ProjectAssembly.Configuration.PrintMaxEnumerableLength;
		set
		{
			GuardMinimumXunitVersion(3, value, nameof(PrintMaxEnumerableLength));
			GuardMinimumValue(0, value, nameof(PrintMaxEnumerableLength), "must be 0 or a positive integer");

			ProjectAssembly.Configuration.PrintMaxEnumerableLength = value;
		}
	}

	/// <summary>
	/// Set to influence the maximum depth of objects to show when printing values.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	///   <item><c>0</c> = always print the objects at all depths</item>
	///   <item>Positive integer = limit the depth to print object</item>
	/// </list>
	/// <para>By default, the maximum object depth is <c>3</c>.</para>
	/// <para><strong>Warning:</strong> Using <c>0</c> when printing objects will circular references
	/// could result in an infinite loop that will cause an <see cref="OutOfMemoryException"/> and
	/// crash the test runner process.</para>
	/// <para><em>Setting maximum printed object depth is only valid for v3 test projects.</em></para>
	/// </remarks>
	public int? PrintMaxObjectDepth
	{
		get => ProjectAssembly.Configuration.PrintMaxObjectDepth;
		set
		{
			GuardMinimumXunitVersion(3, value, nameof(PrintMaxObjectDepth));
			GuardMinimumValue(0, value, nameof(PrintMaxObjectDepth), "must be 0 or a positive integer");

			ProjectAssembly.Configuration.PrintMaxObjectDepth = value;
		}
	}

	/// <summary>
	/// Set to influence the maximum number of members to show when printing an object.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	///   <item><c>0</c> = always print all members of an object</item>
	///   <item>Positive integer = limit the number of members to print</item>
	/// </list>
	/// <para>By default, the maximum number of members to show is <c>5</c>.</para>
	/// <para><em>Setting maximum object member count is only valid for v3 test projects.</em></para>
	/// </remarks>
	public int? PrintMaxObjectMemberCount
	{
		get => ProjectAssembly.Configuration.PrintMaxObjectMemberCount;
		set
		{
			GuardMinimumXunitVersion(3, value, nameof(PrintMaxObjectMemberCount));
			GuardMinimumValue(0, value, nameof(PrintMaxObjectMemberCount), "must be 0 or a positive integer");

			ProjectAssembly.Configuration.PrintMaxObjectMemberCount = value;
		}
	}

	/// <summary>
	/// Set to influence the maximum length when printing a string value.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	///   <item><c>0</c> = always print full strings</item>
	///   <item>Positive integer = limit the print length of strings</item>
	/// </list>
	/// <para>By default, the maximum printed string length is <c>50</c>.</para>
	/// <para><em>Setting maximum string length is only valid for v3 test projects.</em></para>
	/// </remarks>
	public int? PrintMaxStringLength
	{
		get => ProjectAssembly.Configuration.PrintMaxStringLength;
		set
		{
			GuardMinimumXunitVersion(3, value, nameof(PrintMaxStringLength));
			GuardMinimumValue(0, value, nameof(PrintMaxStringLength), "must be 0 or a positive integer");

			ProjectAssembly.Configuration.PrintMaxStringLength = value;
		}
	}

	/// <summary>
	/// Set to override the seed used for randomization.
	/// </summary>
	/// <remarks>
	/// <para>The seed value cannot be a negative number.</para>
	/// <para><em>Setting the seed is only valid for v3 test projects.</em></para>
	/// </remarks>
	public int? Seed
	{
		get => ProjectAssembly.Configuration.Seed;
		set
		{
			GuardMinimumXunitVersion(3, value, nameof(Seed));
			GuardMinimumValue(0, value, nameof(Seed), "cannot be negative");

			ProjectAssembly.Configuration.Seed = value;
		}
	}

#if NETFRAMEWORK

	/// <summary>
	/// Determines if shadow copying is enabled when app domains are being used.
	/// </summary>
	/// <remarks>
	/// <para>The default value is <see langword="false"/>.</para>
	/// <para><em>App domains are only valid for xUnit.net v1 and xUnit.net v2 test projects that target .NET Framework.</em></para>
	/// </remarks>
	public bool? ShadowCopy
	{
		get => ProjectAssembly.Configuration.ShadowCopy;
		set
		{
			GuardAppDomainCompatible(value, nameof(ShadowCopy));

			ProjectAssembly.Configuration.ShadowCopy = value;
		}
	}

	/// <summary>
	/// Determines the folder used for shadow copying if it's enabled and app domains are being used.
	/// </summary>
	/// <remarks>
	/// <em>App domains are only valid for xUnit.net v1 and xUnit.net v2 test projects that target .NET Framework.</em>
	/// </remarks>
	public string? ShadowCopyFolder
	{
		get => ProjectAssembly.Configuration.ShadowCopyFolder;
		set
		{
			GuardAppDomainCompatible(value, nameof(ShadowCopyFolder));

			ProjectAssembly.Configuration.ShadowCopyFolder = value;
		}
	}

#endif  // NETFRAMEWORK

	/// <summary>
	/// Set this to attempt to stop running tests as soon as there is a test failure.
	/// </summary>
	/// <remarks>
	/// <para><em>Stopping on failure is only valid for xUnit.net v2 and xUnit.net v3 test projects.</em></para>
	/// </remarks>
	public bool? StopOnFail
	{
		get => ProjectAssembly.Configuration.StopOnFail;
		set
		{
			GuardMinimumXunitVersion(2, value, nameof(StopOnFail));

			ProjectAssembly.Configuration.StopOnFail = value;
		}
	}

	// ========== Runtime notifications ==========

	/// <summary>
	/// Set to get notification of diagnostic messages.
	/// </summary>
	public Action<MessageInfo>? OnDiagnosticMessage
	{
		get => onDiagnosticMessage;
		set
		{
			ProjectAssembly.Configuration.DiagnosticMessages = value is not null;
			onDiagnosticMessage = value;
		}
	}

	/// <summary>
	/// Set to get notification of when test discovery is complete.
	/// </summary>
	public Action<DiscoveryCompleteInfo>? OnDiscoveryComplete { get; set; }

	/// <summary>
	/// Set to get notification of when test discovery is starting.
	/// </summary>
	public Action? OnDiscoveryStarting { get; set; }

	/// <summary>
	/// Set to get notification of error messages (unhandled exceptions outside of tests).
	/// </summary>
	public Action<ErrorMessageInfo>? OnErrorMessage { get; set; }

	/// <summary>
	/// Set to get notification of when test execution is complete.
	/// </summary>
	public Action<ExecutionCompleteInfo>? OnExecutionComplete { get; set; }

	/// <summary>
	/// Set to get notification of when test execution is starting.
	/// </summary>
	public Action<ExecutionStartingInfo>? OnExecutionStarting { get; set; }

	/// <summary>
	/// Set to get notification of messages internal diagnostic messages.
	/// </summary>
	public Action<MessageInfo>? OnInternalDiagnosticMessage
	{
		get => onInternalDiagnosticsMessage;
		set
		{
			ProjectAssembly.Configuration.InternalDiagnosticMessages = value is not null;
			onInternalDiagnosticsMessage = value;
		}
	}

	/// <summary>
	/// Set to get notification of failed tests.
	/// </summary>
	/// <remarks>
	/// Failed tests are also reported to <see cref="OnTestFinished"/> with an instance of <see cref="TestSkippedInfo"/>.
	/// </remarks>
	public Action<TestFailedInfo>? OnTestFailed { get; set; }

	/// <summary>
	/// Set to get notification of finished tests (regardless of outcome).
	/// </summary>
	/// <remarks>
	/// The <see cref="TestFinishedInfo"/> class is an abstract base class, and the classes which
	/// have status information (i.e., <see cref="TestPassedInfo"/>) are derived from it. The events
	/// for <see cref="OnTestFailed"/>, <see cref="OnTestNotRun"/>, <see cref="OnTestPassed"/>,
	/// and <see cref="OnTestSkipped"/> can be used to have status-specific handling. The same
	/// finished info object is sent in both cases, so you can alternatively just subscribe to this
	/// one handler and differentiate status based on info class type.<br />
	/// <br />
	/// If you subscribe to both the specific handlers and this generic handler, note that the generic
	/// handler will be called after the appropriate specific handler.
	/// </remarks>
	public Action<TestFinishedInfo>? OnTestFinished { get; set; }

	/// <summary>
	/// Set to get real-time notification of test output.
	/// </summary>
	/// <remarks>
	/// Live test output is provided in addition to the complete output during test completion events.<br />
	/// <br />
	/// Live output is only available for v2 and v3 test projects.
	/// </remarks>
	public Action<TestOutputInfo>? OnTestOutput { get; set; }

	/// <summary>
	/// Set to get notification of tests which were not run.
	/// </summary>
	/// <remarks>
	/// Not-run tests are those tests which didn't match the explicit test filter.<br />
	/// <br />
	/// Not-run tests are also reported to <see cref="OnTestFinished"/> with an instance of <see cref="TestNotRunInfo"/>.
	/// </remarks>
	public Action<TestNotRunInfo>? OnTestNotRun { get; set; }

	/// <summary>
	/// Set to get notification of passing tests.
	/// </summary>
	/// <remarks>
	/// Passing tests are also reported to <see cref="OnTestFinished"/> with an instance of <see cref="TestPassedInfo"/>.
	/// </remarks>
	public Action<TestPassedInfo>? OnTestPassed { get; set; }

	/// <summary>
	/// Set to get notification of skipped tests.
	/// </summary>
	/// <remarks>
	/// Skipped tests are also reported to <see cref="OnTestFinished"/> with an instance of <see cref="TestSkippedInfo"/>.
	/// </remarks>
	public Action<TestSkippedInfo>? OnTestSkipped { get; set; }

	/// <summary>
	/// Set to get notification of when a test starts running.
	/// </summary>
	public Action<TestStartingInfo>? OnTestStarting { get; set; }

	// ========== Report generators ==========

	/// <summary>
	/// Adds a generated report when test execution has completed.
	/// </summary>
	/// <param name="reportType">The report type</param>
	/// <param name="outputFileName">The file name to save the report to</param>
	public void AddReport(
		ReportType reportType,
		string outputFileName) =>
			ProjectAssembly.Project.Configuration.Output.Add(reportType.ToKey(), outputFileName);

	// ========== Helpers ==========

#if NETFRAMEWORK

	void GuardAppDomainCompatible(
		object? value,
		string paramName) =>
			Guard.ArgumentValid(
				"App domains are only supported by xUnit.net v1 or xUnit.net v2 test projects which target .NET Framework",
				value is null || (TargetFrameworkIdentifier == TargetFrameworkIdentifier.DotNetFramework && XunitVersion < 3),
				paramName
			);

#endif  // NETFRAMEWORK

	static void GuardMinimumValue(
		int minimumValue,
		int? value,
		string paramName,
		string message) =>
			Guard.ArgumentValid(
				() => string.Format(CultureInfo.CurrentCulture, "{0} {1}", paramName, message),
				value is null || value >= minimumValue,
				paramName
			);

	void GuardMinimumXunitVersion(
		int minimumVersion,
		object? value,
		string paramName) =>
			Guard.ArgumentValid(
				() => string.Format(CultureInfo.CurrentCulture, "{0} is not supported for xUnit.net v{1} projects", paramName, XunitVersion),
				value is null || XunitVersion >= minimumVersion,
				paramName
			);

	static void GuardValidValue<T>(
		T? value,
		Func<T, bool> validator,
		string paramName)
			where T : struct =>
				Guard.ArgumentValid(
					() => string.Format(CultureInfo.CurrentCulture, "'{0}' is not a valid value for {1}", value, paramName),
					value is null || validator(value.Value),
					paramName
				);
}
