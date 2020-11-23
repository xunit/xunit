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
	/// A delegating implementation of <see cref="IExecutionSink"/> which converts all
	/// skipped tests into failures before passing them on to the inner sink.
	/// </summary>
	public class DelegatingFailSkipSink : LongLivedMarshalByRefObject, IExecutionSink
	{
		bool disposed;
		readonly IExecutionSink innerSink;
		int skipCount;

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegatingFailSkipSink"/> class.
		/// </summary>
		/// <param name="innerSink">The sink to delegate messages to.</param>
		public DelegatingFailSkipSink(IExecutionSink innerSink)
		{
			Guard.ArgumentNotNull(nameof(innerSink), innerSink);

			this.innerSink = innerSink;
		}

		/// <inheritdoc/>
		public ExecutionSummary ExecutionSummary => innerSink.ExecutionSummary;

		/// <inheritdoc/>
		public ManualResetEvent Finished => innerSink.Finished;

		/// <inheritdoc/>
		public void Dispose()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			innerSink.Dispose();
		}

		/// <inheritdoc/>
		public bool OnMessage(IMessageSinkMessage message)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			var messageTypes = default(HashSet<string>);  // TODO temporary

			var testSkipped = message.Cast<_TestSkipped>(messageTypes);
			if (testSkipped != null)
			{
				skipCount++;

				// TODO: This is broken because of the null ITest, to be fixed soon
				var testFailed = new TestFailed(
					null!, 0M, "",
					new[] { "FAIL_SKIP" },
					new[] { testSkipped.Reason },
					new[] { "" },
					new[] { -1 }
				);

				return innerSink.OnMessage(testFailed);
			}

			var testCollectionFinished = message.Cast<_TestCollectionFinished>(messageTypes);
			if (testCollectionFinished != null)
			{
				testCollectionFinished = new _TestCollectionFinished
				{
					ExecutionTime = testCollectionFinished.ExecutionTime,
					TestCollectionUniqueID = testCollectionFinished.TestCollectionUniqueID,
					TestsFailed = testCollectionFinished.TestsFailed + testCollectionFinished.TestsSkipped,
					TestsRun = testCollectionFinished.TestsRun,
					TestsSkipped = 0
				};

				return innerSink.OnMessage(testCollectionFinished);
			}

			var assemblyFinished = message.Cast<_TestAssemblyFinished>(messageTypes);
			if (assemblyFinished != null)
			{
				assemblyFinished = new _TestAssemblyFinished
				{
					AssemblyUniqueID = assemblyFinished.AssemblyUniqueID,
					ExecutionTime = assemblyFinished.ExecutionTime,
					TestsFailed = assemblyFinished.TestsFailed + assemblyFinished.TestsSkipped,
					TestsRun = assemblyFinished.TestsRun,
					TestsSkipped = 0
				};

				return innerSink.OnMessage(assemblyFinished);
			}

			return innerSink.OnMessage(message);
		}
	}
}
