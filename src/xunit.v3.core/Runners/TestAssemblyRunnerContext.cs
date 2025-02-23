using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestAssemblyRunner{TContext, TTestCase, TTestAssembly, TTestCollection}"/>.
/// </summary>
/// <typeparam name="TTestAssembly">The type of the test assembly object model. Must derive
/// from <see cref="ITestAssembly"/>.</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ITestCase"/>.</typeparam>
/// <param name="testAssembly">The test assembly</param>
/// <param name="testCases">The test cases from the assembly</param>
/// <param name="executionMessageSink">The message sink to send execution messages to</param>
/// <param name="executionOptions">The options used during test execution</param>
/// <param name="cancellationToken">The cancellation token used to cancel execution</param>
public class TestAssemblyRunnerContext<TTestAssembly, TTestCase>(
	TTestAssembly testAssembly,
	IReadOnlyCollection<TTestCase> testCases,
	IMessageSink executionMessageSink,
	ITestFrameworkExecutionOptions executionOptions,
	CancellationToken cancellationToken) :
		IAsyncLifetime
		where TTestCase : class, ITestCase
		where TTestAssembly : class, ITestAssembly
{
	IMessageBus? messageBus;
	string? startupCurrentDirectory;

	/// <summary>
	/// Gets the aggregator used for reporting exceptions.
	/// </summary>
	public virtual ExceptionAggregator Aggregator { get; } = ExceptionAggregator.Create();

	/// <summary>
	/// Gets the cancellation token source used for cancelling test execution.
	/// </summary>
	public virtual CancellationTokenSource CancellationTokenSource { get; } = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

	/// <summary>
	/// Gets the execution message sink provided by the runner. This is typically wrapped into
	/// the message bus by <see cref="CreateMessageBus"/>.
	/// </summary>
	protected IMessageSink ExecutionMessageSink { get; } = Guard.ArgumentNotNull(executionMessageSink);

	/// <summary>
	/// Gets the execution options provided by the runner.
	/// </summary>
	protected ITestFrameworkExecutionOptions ExecutionOptions { get; } = Guard.ArgumentNotNull(executionOptions);

	/// <summary>
	/// Gets the message bus to send execution engine messages to.
	/// </summary>
	public IMessageBus MessageBus =>
		this.ValidateNullablePropertyValue(messageBus, nameof(MessageBus));

	/// <summary>
	/// Gets the target framework that the test assembly is targeting. By default, this returns <c>null</c>.
	/// Test frameworks that are based on .NET runtime assemblies can look for <see cref="TargetFrameworkAttribute"/>
	/// decorated at the assembly level, which is added automatically by the compiler.
	/// </summary>
	public virtual string? TargetFramework =>
		null;

	/// <summary>
	/// Gets the assembly that is being executed.
	/// </summary>
	public TTestAssembly TestAssembly { get; } = Guard.ArgumentNotNull(testAssembly);

	/// <summary>
	/// Gets the test cases associated with this test assembly.
	/// </summary>
	public IReadOnlyCollection<TTestCase> TestCases { get; protected set; } = Guard.ArgumentNotNull(testCases);

	/// <summary>
	/// Gets the environment that the test are running in. By default, returns a string which
	/// indicates process bitness, processor architecture (when known), and the description of
	/// the runtime environment (from <see cref="RuntimeInformation.FrameworkDescription"/>).
	/// </summary>
	/// <remarks>
	/// Example: "64-bit (x64) .NET 8.0.11"
	/// </remarks>
	public virtual string TestEnvironment =>
		string.Format(
			CultureInfo.CurrentCulture,
			"{0}-bit ({1}) {2}",
			IntPtr.Size * 8,
			RuntimeInformation.ProcessArchitecture.ToDisplayName(),
			RuntimeInformation.FrameworkDescription
		);

	/// <summary>
	/// Creates the message bus to be used for test execution. By default, it inspects
	/// the options for the <see cref="TestOptionsNames.Execution.SynchronousMessageReporting"/>
	/// flag, and if present, creates a message bus that ensures all messages are delivered
	/// on the same thread.
	/// </summary>
	/// <returns>The message bus.</returns>
	protected virtual IMessageBus CreateMessageBus() =>
		ExecutionOptions.SynchronousMessageReportingOrDefault()
			? new SynchronousMessageBus(ExecutionMessageSink, ExecutionOptions.StopOnTestFailOrDefault())
			: new MessageBus(ExecutionMessageSink, ExecutionOptions.StopOnTestFailOrDefault());

	/// <inheritdoc/>
	public virtual ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		messageBus?.Dispose();

		try
		{
			if (startupCurrentDirectory is not null)
				Directory.SetCurrentDirectory(startupCurrentDirectory);
		}
		catch { }

		return default;
	}

	/// <inheritdoc/>
	public virtual ValueTask InitializeAsync()
	{
		startupCurrentDirectory = Directory.GetCurrentDirectory();
		messageBus = CreateMessageBus();

		return default;
	}
}
