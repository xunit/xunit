using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.v2;

namespace Xunit.Sdk
{
	/// <summary>
	/// A reusable implementation of <see cref="ITestFrameworkExecutor"/> which contains the basic behavior
	/// for running tests.
	/// </summary>
	/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
	/// derive from <see cref="ITestCase"/>.</typeparam>
	public abstract class TestFrameworkExecutor<TTestCase> : ITestFrameworkExecutor, IAsyncDisposable
		where TTestCase : ITestCase
	{
		IReflectionAssemblyInfo assemblyInfo;
		IMessageSink diagnosticMessageSink;
		bool disposed;
		ISourceInformationProvider sourceInformationProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestFrameworkExecutor{TTestCase}"/> class.
		/// </summary>
		/// <param name="assemblyInfo">The test assembly.</param>
		/// <param name="sourceInformationProvider">The source line number information provider.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="IDiagnosticMessage"/> messages.</param>
		protected TestFrameworkExecutor(
			IReflectionAssemblyInfo assemblyInfo,
			ISourceInformationProvider sourceInformationProvider,
			IMessageSink diagnosticMessageSink)
		{
			this.assemblyInfo = Guard.ArgumentNotNull(nameof(assemblyInfo), assemblyInfo);
			this.sourceInformationProvider = Guard.ArgumentNotNull(nameof(sourceInformationProvider), sourceInformationProvider);
			this.diagnosticMessageSink = Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);
		}

		/// <summary>
		/// Gets the assembly information of the assembly under test.
		/// </summary>
		protected IReflectionAssemblyInfo AssemblyInfo
		{
			get => assemblyInfo;
			set => assemblyInfo = Guard.ArgumentNotNull(nameof(AssemblyInfo), value);
		}

		/// <summary>
		/// Gets the message sink to send diagnostic messages to.
		/// </summary>
		protected IMessageSink DiagnosticMessageSink
		{
			get => diagnosticMessageSink;
			set => diagnosticMessageSink = Guard.ArgumentNotNull(nameof(DiagnosticMessageSink), value);
		}

		/// <summary>
		/// Gets the disposal tracker for the test framework discoverer.
		/// </summary>
		protected DisposalTracker DisposalTracker { get; } = new DisposalTracker();

		/// <summary>
		/// Gets the source information provider.
		/// </summary>
		protected ISourceInformationProvider SourceInformationProvider
		{
			get => sourceInformationProvider;
			set => sourceInformationProvider = Guard.ArgumentNotNull(nameof(SourceInformationProvider), value);
		}

		/// <summary>
		/// Override to create a test framework discoverer that can be used to discover
		/// tests when the user asks to run all test.
		/// </summary>
		/// <returns>The test framework discoverer</returns>
		protected abstract ITestFrameworkDiscoverer CreateDiscoverer();

		/// <inheritdoc/>
		public virtual ITestCase Deserialize(string value)
		{
			Guard.ArgumentNotNull(nameof(value), value);

			return SerializationHelper.Deserialize<ITestCase>(value) ?? throw new ArgumentException($"Could not deserialize test case: {value}", nameof(value));
		}

		/// <inheritdoc/>
		void IDisposable.Dispose() { }  // TODO: This should be removed once shifting to v3 abstractions

		/// <inheritdoc/>
		public virtual ValueTask DisposeAsync()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			return DisposalTracker.DisposeAsync();
		}

		/// <inheritdoc/>
		public virtual void RunAll(
			IMessageSink executionMessageSink,
			ITestFrameworkDiscoveryOptions discoveryOptions,
			ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull("executionMessageSink", executionMessageSink);
			Guard.ArgumentNotNull("discoveryOptions", discoveryOptions);
			Guard.ArgumentNotNull("executionOptions", executionOptions);

			var discoverySink = new TestDiscoveryVisitor();

			using (var discoverer = CreateDiscoverer())
			{
				discoverer.Find(false, discoverySink, discoveryOptions);
				discoverySink.Finished.WaitOne();
			}

			RunTestCases(discoverySink.TestCases.Cast<TTestCase>(), executionMessageSink, executionOptions);
		}

		/// <inheritdoc/>
		public virtual void RunTests(
			IEnumerable<ITestCase> testCases,
			IMessageSink executionMessageSink,
			ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull("testCases", testCases);
			Guard.ArgumentNotNull("executionMessageSink", executionMessageSink);
			Guard.ArgumentNotNull("executionOptions", executionOptions);

			RunTestCases(testCases.Cast<TTestCase>(), executionMessageSink, executionOptions);
		}

		/// <summary>
		/// Override to run test cases.
		/// </summary>
		/// <param name="testCases">The test cases to be run.</param>
		/// <param name="executionMessageSink">The message sink to report run status to.</param>
		/// <param name="executionOptions">The user's requested execution options.</param>
		protected abstract void RunTestCases(
			IEnumerable<TTestCase> testCases,
			IMessageSink executionMessageSink,
			ITestFrameworkExecutionOptions executionOptions
		);
	}
}
