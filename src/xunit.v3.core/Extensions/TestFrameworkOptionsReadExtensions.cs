using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// Extension methods for reading <see cref="_ITestFrameworkDiscoveryOptions"/> and <see cref="_ITestFrameworkExecutionOptions"/>.
	/// </summary>
	public static class TestFrameworkOptionsReadExtensions
	{
		// Read methods for discovery options

		/// <summary>
		/// Gets a flag that determines whether diagnostic messages will be emitted.
		/// </summary>
		public static bool? DiagnosticMessages(this _ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.DiagnosticMessages);
		}

		/// <summary>
		/// Gets a flag that determines whether diagnostic messages will be emitted. If the flag is not present,
		/// returns the default value (<c>false</c>).
		/// </summary>
		public static bool DiagnosticMessagesOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			return discoveryOptions.DiagnosticMessages() ?? false;
		}

		/// <summary>
		/// Gets a flag that determines whether discovered test cases should include source information.
		/// Note that not all runners have access to source information, so this flag does not guarantee
		/// that source information will be provided.
		/// </summary>
		public static bool? IncludeSourceInformation(this _ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.IncludeSourceInformation);
		}

		/// <summary>
		/// Gets a flag that determines whether discovered test cases should include source information.
		/// Note that not all runners have access to source information, so this flag does not guarantee
		/// that source information will be provided. If the flag is not present, returns the default
		/// value (<c>false</c>).
		/// </summary>
		public static bool IncludeSourceInformationOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			return discoveryOptions.IncludeSourceInformation() ?? false;
		}

		/// <summary>
		/// Gets a flag that determines the default display name format for test methods.
		/// </summary>
		public static TestMethodDisplay? MethodDisplay(this _ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			var methodDisplayString = discoveryOptions.GetValue<string>(TestOptionsNames.Discovery.MethodDisplay);
			return methodDisplayString != null ? (TestMethodDisplay?)Enum.Parse(typeof(TestMethodDisplay), methodDisplayString) : null;
		}

		/// <summary>
		/// Gets a flag that determines the default display options to format test methods.
		/// </summary>
		public static TestMethodDisplayOptions? MethodDisplayOptions(this _ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			var methodDisplayOptionsString = discoveryOptions.GetValue<string>(TestOptionsNames.Discovery.MethodDisplayOptions);
			return methodDisplayOptionsString != null ? (TestMethodDisplayOptions?)Enum.Parse(typeof(TestMethodDisplayOptions), methodDisplayOptionsString) : null;
		}

		/// <summary>
		/// Gets a flag that determines the default display name format for test methods. If the flag is not present,
		/// returns the default value (<see cref="TestMethodDisplay.ClassAndMethod"/>).
		/// </summary>
		public static TestMethodDisplay MethodDisplayOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			return discoveryOptions.MethodDisplay() ?? TestMethodDisplay.ClassAndMethod;
		}

		/// <summary>
		/// Gets the options that determine the default display formatting options for test methods. If no options are not present,
		/// returns the default value (<see cref="TestMethodDisplayOptions.None"/>).
		/// </summary>
		public static TestMethodDisplayOptions MethodDisplayOptionsOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			return discoveryOptions.MethodDisplayOptions() ?? TestMethodDisplayOptions.None;
		}

		/// <summary>
		/// Gets a flag that determines whether theories are pre-enumerated. If they enabled, then the
		/// discovery system will return a test case for each row of test data; they are disabled, then the
		/// discovery system will return a single test case for the theory.
		/// </summary>
		public static bool? PreEnumerateTheories(this _ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			return discoveryOptions.GetValue<bool?>(TestOptionsNames.Discovery.PreEnumerateTheories);
		}

		/// <summary>
		/// Gets a flag that determines whether theories are pre-enumerated. If enabled, then the
		/// discovery system will return a test case for each row of test data; if disabled, then the
		/// discovery system will return a single test case for the theory. If the flag is not present,
		/// returns the default value (<c>true</c>).
		/// </summary>
		public static bool PreEnumerateTheoriesOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			return discoveryOptions.PreEnumerateTheories() ?? true;
		}

		/// <summary>
		/// Gets a flag that determines whether xUnit.net should report test results synchronously.
		/// </summary>
		public static bool? SynchronousMessageReporting(this _ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			return discoveryOptions.GetValue<bool?>(TestOptionsNames.Execution.SynchronousMessageReporting);
		}

		/// <summary>
		/// Gets a flag that determines whether xUnit.net should report test results synchronously.
		/// If the flag is not set, returns the default value (<c>false</c>).
		/// </summary>
		public static bool SynchronousMessageReportingOrDefault(this _ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			return discoveryOptions.SynchronousMessageReporting() ?? false;
		}

		// Read methods for execution options

		/// <summary>
		/// Gets a flag that determines whether diagnostic messages will be emitted.
		/// </summary>
		public static bool? DiagnosticMessages(this _ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.DiagnosticMessages);
		}

		/// <summary>
		/// Gets a flag that determines whether diagnostic messages will be emitted. If the flag is not
		/// present, returns the default value (<c>false</c>).
		/// </summary>
		public static bool DiagnosticMessagesOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			return executionOptions.DiagnosticMessages() ?? false;
		}

		/// <summary>
		/// Gets a flag to disable parallelization.
		/// </summary>
		public static bool? DisableParallelization(this _ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.DisableParallelization);
		}

		/// <summary>
		/// Gets a flag to disable parallelization. If the flag is not present, returns the
		/// default value (<c>false</c>).
		/// </summary>
		public static bool DisableParallelizationOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			return executionOptions.DisableParallelization() ?? false;
		}

		/// <summary>
		/// Gets the maximum number of threads to use when running tests in parallel.
		/// </summary>
		public static int? MaxParallelThreads(this _ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			return executionOptions.GetValue<int?>(TestOptionsNames.Execution.MaxParallelThreads);
		}

		/// <summary>
		/// Gets the maximum number of threads to use when running tests in parallel. If set to 0 (or not set),
		/// the value of <see cref="Environment.ProcessorCount"/> is used; if set to a value less
		/// than 0, does not limit the number of threads.
		/// </summary>
		public static int MaxParallelThreadsOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			var result = executionOptions.MaxParallelThreads();
			if (result == null || result == 0)
				return Environment.ProcessorCount;

			return result.GetValueOrDefault();
		}

		/// <summary>
		/// Gets a flag to stop testing on test failure.
		/// </summary>
		public static bool? StopOnTestFail(this _ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.StopOnFail);
		}

		/// <summary>
		/// Gets a flag to stop testing on test failure. If the flag is not present, returns the 
		/// default value (<c>false</c>).
		/// </summary>
		public static bool StopOnTestFailOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			return executionOptions.StopOnTestFail() ?? false;
		}

		/// <summary>
		/// Gets a flag that determines whether xUnit.net should report test results synchronously.
		/// </summary>
		public static bool? SynchronousMessageReporting(this _ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			return executionOptions.GetValue<bool?>(TestOptionsNames.Execution.SynchronousMessageReporting);
		}

		/// <summary>
		/// Gets a flag that determines whether xUnit.net should report test results synchronously.
		/// If the flag is not set, returns the default value (<c>false</c>).
		/// </summary>
		public static bool SynchronousMessageReportingOrDefault(this _ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			return executionOptions.SynchronousMessageReporting() ?? false;
		}
	}
}
