using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

// TODO: Need to acceptance test this via Xunit3, once it comes into existence. See Xunit2Tests.cs for examples.

namespace Xunit.v3
{
	/// <summary>
	/// The implementation of <see cref="_ITestFrameworkExecutor"/> that supports execution
	/// of unit tests linked against xunit.v3.core.dll.
	/// </summary>
	public class XunitTestFrameworkExecutor : TestFrameworkExecutor<IXunitTestCase>
	{
		readonly Lazy<XunitTestFrameworkDiscoverer> discoverer;
		TestAssembly testAssembly;

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestFrameworkExecutor"/> class.
		/// </summary>
		/// <param name="assemblyInfo">The test assembly.</param>
		/// <param name="configFileName">The test configuration file.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		public XunitTestFrameworkExecutor(
			_IReflectionAssemblyInfo assemblyInfo,
			string? configFileName,
			_IMessageSink diagnosticMessageSink)
				: base(assemblyInfo, diagnosticMessageSink)
		{
			testAssembly = new TestAssembly(AssemblyInfo, configFileName, assemblyInfo.Assembly.GetName().Version);
			discoverer = new Lazy<XunitTestFrameworkDiscoverer>(() => new XunitTestFrameworkDiscoverer(AssemblyInfo, configFileName, DiagnosticMessageSink));
		}

		/// <summary>
		/// Gets the test assembly that contains the test.
		/// </summary>
		protected TestAssembly TestAssembly
		{
			get => testAssembly;
			set => testAssembly = Guard.ArgumentNotNull(nameof(TestAssembly), value);
		}

		/// <inheritdoc/>
		protected override _ITestFrameworkDiscoverer CreateDiscoverer() => discoverer.Value;

		/// <inheritdoc/>
		public override async ValueTask RunTestCases(
			IReadOnlyCollection<IXunitTestCase> testCases,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions)
		{
			await using var assemblyRunner = new XunitTestAssemblyRunner(TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink, executionOptions);
			await assemblyRunner.RunAsync();
		}
	}
}
