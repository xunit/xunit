using System;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// The callback passed to <see cref="DelegatingSummarySink"/> when execution is complete.
/// </summary>
/// <param name="summary">The summary of the execution</param>
/// <param name="assemblyMetadata">The assembly for which this summary applies</param>
public delegate void DelegatingSummarySinkCallback(
	ExecutionSummary summary,
	_IAssemblyMetadata? assemblyMetadata
);

/// <summary>
/// A delegating implementation of <see cref="IExecutionSink"/> which provides the execution
/// summary and finished events when appropriate and cancellation support. It also converts
/// the following messages:<br/>
/// <br/>
/// - <see cref="_DiscoveryStarting"/> to <see cref="TestAssemblyDiscoveryStarting"/><br/>
/// - <see cref="_DiscoveryComplete"/> to <see cref="TestAssemblyDiscoveryFinished"/><br/>
/// - <see cref="_TestAssemblyStarting"/> to <see cref="TestAssemblyExecutionStarting"/><br />
/// - <see cref="_TestAssemblyFinished"/> to <see cref="TestAssemblyExecutionFinished"/>
/// </summary>
public class DelegatingSummarySink : IExecutionSink
{
	readonly AppDomainOption appDomainOption;
	readonly XunitProjectAssembly assembly;
	readonly Func<bool> cancelThunk;
	readonly DelegatingSummarySinkCallback? completionCallback;
	readonly _ITestFrameworkDiscoveryOptions discoveryOptions;
	bool disposed;
	volatile int errors;
	readonly _ITestFrameworkExecutionOptions executionOptions;
	readonly _IMessageSink innerSink;
	readonly MessageMetadataCache metadataCache = new();
	readonly bool shadowCopy;

	/// <summary>
	/// Initializes a new instance of the <see cref="DelegatingSummarySink"/> class.
	/// </summary>
	/// <param name="assembly">The assembly under test.</param>
	/// <param name="discoveryOptions">The options used during test discovery.</param>
	/// <param name="executionOptions">The options used during test execution.</param>
	/// <param name="appDomainOption">A flag to indicate whether app domains are in use.</param>
	/// <param name="shadowCopy">A flag to indicate whether shadow copying is in use.</param>
	/// <param name="innerSink">The inner sink to pass messages to.</param>
	/// <param name="cancelThunk">The optional callback used to determine if execution should be canceled</param>
	/// <param name="completionCallback">The optional callback called when assembly execution is complete</param>
	public DelegatingSummarySink(
		XunitProjectAssembly assembly,
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestFrameworkExecutionOptions executionOptions,
		AppDomainOption appDomainOption,
		bool shadowCopy,
		_IMessageSink innerSink,
		Func<bool>? cancelThunk = null,
		DelegatingSummarySinkCallback? completionCallback = null)
	{
		this.assembly = Guard.ArgumentNotNull(assembly);
		this.discoveryOptions = Guard.ArgumentNotNull(discoveryOptions);
		this.executionOptions = Guard.ArgumentNotNull(executionOptions);
		this.appDomainOption = appDomainOption;
		this.shadowCopy = shadowCopy;
		this.innerSink = Guard.ArgumentNotNull(innerSink);
		this.cancelThunk = cancelThunk ?? (() => false);
		this.completionCallback = completionCallback;
	}

	/// <inheritdoc/>
	public ExecutionSummary ExecutionSummary { get; } = new ExecutionSummary();

	/// <inheritdoc/>
	public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);

	/// <inheritdoc/>
	public void Dispose()
	{
		if (disposed)
			return;

		disposed = true;

		GC.SuppressFinalize(this);

		Finished.Dispose();
	}

	void HandleDiscoveryComplete(MessageHandlerArgs<_DiscoveryComplete> args)
	{
		var discoveryFinished = new TestAssemblyDiscoveryFinished
		{
			Assembly = assembly,
			DiscoveryOptions = discoveryOptions,
			TestCasesToRun = args.Message.TestCasesToRun,
		};
		innerSink.OnMessage(discoveryFinished);
	}

	void HandleDiscoveryStarting(MessageHandlerArgs<_DiscoveryStarting> args)
	{
		var discoveryStarting = new TestAssemblyDiscoveryStarting
		{
			AppDomain = appDomainOption,
			Assembly = assembly,
			DiscoveryOptions = discoveryOptions,
			ShadowCopy = shadowCopy,
		};
		innerSink.OnMessage(discoveryStarting);
	}

	void HandleTestAssemblyStarting(MessageHandlerArgs<_TestAssemblyStarting> args)
	{
		metadataCache.Set(args.Message);

		var executionStarting = new TestAssemblyExecutionStarting
		{
			Assembly = assembly,
			ExecutionOptions = executionOptions,
			Seed = args.Message.Seed,
		};
		innerSink.OnMessage(executionStarting);
	}

	void HandleTestAssemblyFinished(MessageHandlerArgs<_TestAssemblyFinished> args)
	{
		ExecutionSummary.Total = args.Message.TestsTotal;
		ExecutionSummary.Failed = args.Message.TestsFailed;
		ExecutionSummary.NotRun = args.Message.TestsNotRun;
		ExecutionSummary.Skipped = args.Message.TestsSkipped;
		ExecutionSummary.Time = args.Message.ExecutionTime;
		ExecutionSummary.Errors = errors;

		var metadata = metadataCache.TryRemove(args.Message);
		if (metadata is not null)
			completionCallback?.Invoke(ExecutionSummary, metadata);
		else
			completionCallback?.Invoke(ExecutionSummary, null);

		var executionFinished = new TestAssemblyExecutionFinished
		{
			Assembly = assembly,
			ExecutionOptions = executionOptions,
			ExecutionSummary = ExecutionSummary,
		};
		innerSink.OnMessage(executionFinished);

		Finished.Set();
	}

	/// <inheritdoc/>
	public bool OnMessage(_MessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		var result = innerSink.OnMessage(message);

		return
			message.DispatchWhen<_DiscoveryComplete>(HandleDiscoveryComplete)
			&& message.DispatchWhen<_DiscoveryStarting>(HandleDiscoveryStarting)
			&& message.DispatchWhen<_ErrorMessage>(args => Interlocked.Increment(ref errors))
			&& message.DispatchWhen<_TestAssemblyCleanupFailure>(args => Interlocked.Increment(ref errors))
			&& message.DispatchWhen<_TestAssemblyFinished>(HandleTestAssemblyFinished)
			&& message.DispatchWhen<_TestAssemblyStarting>(HandleTestAssemblyStarting)
			&& message.DispatchWhen<_TestCaseCleanupFailure>(args => Interlocked.Increment(ref errors))
			&& message.DispatchWhen<_TestClassCleanupFailure>(args => Interlocked.Increment(ref errors))
			&& message.DispatchWhen<_TestCleanupFailure>(args => Interlocked.Increment(ref errors))
			&& message.DispatchWhen<_TestCollectionCleanupFailure>(args => Interlocked.Increment(ref errors))
			&& message.DispatchWhen<_TestMethodCleanupFailure>(args => Interlocked.Increment(ref errors))
			&& result
			&& !cancelThunk();
	}
}
