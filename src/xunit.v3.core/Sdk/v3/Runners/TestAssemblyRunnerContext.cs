using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestAssemblyRunner{TContext, TTestCase}"/>.
/// </summary>
public abstract class TestAssemblyRunnerContext<TTestCase> : IAsyncLifetime
	where TTestCase : _ITestCase
{
	IMessageBus? messageBus;
	string? startupCurrentDirectory;

	/// <summary>
	/// Initializes a new instance of the <see cref="TestAssemblyRunnerContext{TTestCase}"/> class.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	/// <param name="testCases">The test cases from the assembly</param>
	/// <param name="executionMessageSink">The message sink to send execution messages to</param>
	/// <param name="executionOptions">The options used during test execution</param>
	protected TestAssemblyRunnerContext(
		_ITestAssembly testAssembly,
		IReadOnlyCollection<TTestCase> testCases,
		_IMessageSink executionMessageSink,
		_ITestFrameworkExecutionOptions executionOptions)
	{
		TestAssembly = Guard.ArgumentNotNull(testAssembly);
		TestCases = Guard.ArgumentNotNull(testCases);
		ExecutionMessageSink = Guard.ArgumentNotNull(executionMessageSink);
		ExecutionOptions = Guard.ArgumentNotNull(executionOptions);
	}

	/// <summary>
	/// Gets the aggregator used for reporting exceptions.
	/// </summary>
	public virtual ExceptionAggregator Aggregator { get; } = ExceptionAggregator.Create();

	/// <summary>
	/// Gets the cancellation token source used for cancelling test execution.
	/// </summary>
	public virtual CancellationTokenSource CancellationTokenSource { get; } = new();

	/// <summary>
	/// Gets the execution message sink provided by the runner. This is typically wrapped into
	/// the message bus by <see cref="CreateMessageBus"/>.
	/// </summary>
	protected _IMessageSink ExecutionMessageSink { get; }

	/// <summary>
	/// Gets the execution options provided by the runner.
	/// </summary>
	protected _ITestFrameworkExecutionOptions ExecutionOptions { get; }

	/// <summary>
	/// Gets a flag which indicates how explicit tests should be handled.
	/// </summary>
	public ExplicitOption ExplicitOption => ExecutionOptions.ExplicitOptionOrDefault();

	/// <summary>
	/// Gets the message bus to send execution engine messages to.
	/// </summary>
	public IMessageBus MessageBus =>
		this.ValidateNullablePropertyValue(messageBus, nameof(MessageBus));

	/// <summary>
	/// Gets the seed value used for randomization.
	/// </summary>
	public int? Seed => Randomizer.Seed;

	/// <summary>
	/// Gets the target framework against which the assembly was compiled (e.g., ".NETFramework,Version=v4.7.2").
	/// </summary>
	public virtual string TargetFramework =>
		TestAssembly.Assembly.GetTargetFramework();

	/// <summary>
	/// Gets the display name for the test framework (e.g., "xUnit.net 2.0").
	/// </summary>
	public abstract string TestFrameworkDisplayName { get; }

	/// <summary>
	/// Gets the environment information (e.g., "32-bit .NET 4.0").
	/// </summary>
	public virtual string TestFrameworkEnvironment =>
		string.Format(CultureInfo.CurrentCulture, "{0}-bit {1}", IntPtr.Size * 8, RuntimeInformation.FrameworkDescription);

	/// <summary>
	/// Gets the assembly that is being executed.
	/// </summary>
	public _ITestAssembly TestAssembly { get; }

	/// <summary>
	/// Gets the test cases associated with this test assembly.
	/// </summary>
	public IReadOnlyCollection<TTestCase> TestCases { get; }

	/// <summary>
	/// Creates the message bus to be used for test execution. By default, it inspects
	/// the options for the <see cref="TestOptionsNames.Execution.SynchronousMessageReporting"/>
	/// flag, and if present, creates a message bus that ensures all messages are delivered
	/// on the same thread.
	/// </summary>
	/// <returns>The message bus.</returns>
	protected virtual IMessageBus CreateMessageBus()
	{
		if (ExecutionOptions.SynchronousMessageReportingOrDefault())
			return new SynchronousMessageBus(ExecutionMessageSink, ExecutionOptions.StopOnTestFailOrDefault());

		return new MessageBus(ExecutionMessageSink, ExecutionOptions.StopOnTestFailOrDefault());
	}

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
