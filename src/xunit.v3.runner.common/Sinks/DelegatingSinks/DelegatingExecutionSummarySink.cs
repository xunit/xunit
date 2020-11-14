using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.v2;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// The callback passed to <see cref="DelegatingExecutionSummarySink"/> when execution is complete.
	/// </summary>
	/// <param name="summary">The summary of the execution</param>
	/// <param name="assemblyMetadata">The assembly for which this summary applies</param>
	public delegate void DelegatingExecutionSummarySinkCallback(ExecutionSummary summary, _IAssemblyMetadata? assemblyMetadata);

	/// <summary>
	/// A delegating implementation of <see cref="IExecutionSink"/> which provides the execution
	/// summary and finished events when appropriate and cancellation support.
	/// </summary>
	public class DelegatingExecutionSummarySink : LongLivedMarshalByRefObject, IExecutionSink
	{
		readonly Func<bool> cancelThunk;
		readonly DelegatingExecutionSummarySinkCallback? completionCallback;
		bool disposed;
		volatile int errors;
		readonly _IMessageSink innerSink;
		readonly MessageMetadataCache metadataCache = new MessageMetadataCache();

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegatingExecutionSummarySink"/> class.
		/// </summary>
		/// <param name="innerSink">The inner sink to pass messages to.</param>
		/// <param name="cancelThunk">The optional callback used to determine if execution should be canceled</param>
		/// <param name="completionCallback">The optional callback called when assembly execution is complete</param>
		public DelegatingExecutionSummarySink(
			_IMessageSink innerSink,
			Func<bool>? cancelThunk = null,
			DelegatingExecutionSummarySinkCallback? completionCallback = null)
		{
			Guard.ArgumentNotNull(nameof(innerSink), innerSink);

			this.innerSink = innerSink;
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
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			Finished.Dispose();
		}

		void HandleTestAssemblyStarting(MessageHandlerArgs<_TestAssemblyStarting> args) =>
			metadataCache.Set(args.Message);

		void HandleTestAssemblyFinished(MessageHandlerArgs<_TestAssemblyFinished> args)
		{
			ExecutionSummary.Total = args.Message.TestsRun;
			ExecutionSummary.Failed = args.Message.TestsFailed;
			ExecutionSummary.Skipped = args.Message.TestsSkipped;
			ExecutionSummary.Time = args.Message.ExecutionTime;
			ExecutionSummary.Errors = errors;

			var metadata = metadataCache.TryRemove(args.Message);
			if (metadata != null)
				completionCallback?.Invoke(ExecutionSummary, metadata);
			else
				completionCallback?.Invoke(ExecutionSummary, null);

			Finished.Set();
		}

		/// <inheritdoc/>
		public bool OnMessage(IMessageSinkMessage message)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			var result = innerSink.OnMessage(message);
			var messageTypes = default(HashSet<string>);  // TODO temporary

			return
				message.Dispatch<IErrorMessage>(messageTypes, args => Interlocked.Increment(ref errors))
				&& message.Dispatch<_TestAssemblyCleanupFailure>(messageTypes, args => Interlocked.Increment(ref errors))
				&& message.Dispatch<_TestAssemblyFinished>(messageTypes, HandleTestAssemblyFinished)
				&& message.Dispatch<_TestAssemblyStarting>(messageTypes, HandleTestAssemblyStarting)
				&& message.Dispatch<ITestCaseCleanupFailure>(messageTypes, args => Interlocked.Increment(ref errors))
				&& message.Dispatch<_TestClassCleanupFailure>(messageTypes, args => Interlocked.Increment(ref errors))
				&& message.Dispatch<ITestCleanupFailure>(messageTypes, args => Interlocked.Increment(ref errors))
				&& message.Dispatch<_TestCollectionCleanupFailure>(messageTypes, args => Interlocked.Increment(ref errors))
				&& message.Dispatch<_TestMethodCleanupFailure>(messageTypes, args => Interlocked.Increment(ref errors))
				&& result
				&& !cancelThunk();
		}
	}
}
