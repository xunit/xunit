using System;
using System.Threading;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// A delegating implementation of <see cref="IExecutionSink"/> which converts all
	/// skipped tests into failures before passing them on to the inner sink.
	/// </summary>
	public class DelegatingFailSkipSink : IExecutionSink
	{
		bool disposed;
		readonly IExecutionSink innerSink;

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
		public bool OnMessage(_MessageSinkMessage message)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			if (message is _TestSkipped testSkipped)
			{
				var testFailed = new _TestFailed
				{
					AssemblyUniqueID = testSkipped.AssemblyUniqueID,
					ExceptionParentIndices = new[] { -1 },
					ExceptionTypes = new[] { "FAIL_SKIP" },
					ExecutionTime = 0m,
					Messages = new[] { testSkipped.Reason },
					Output = "",
					StackTraces = new[] { "" },
					TestCaseUniqueID = testSkipped.TestCaseUniqueID,
					TestClassUniqueID = testSkipped.TestClassUniqueID,
					TestCollectionUniqueID = testSkipped.TestCollectionUniqueID,
					TestMethodUniqueID = testSkipped.TestMethodUniqueID,
					TestUniqueID = testSkipped.TestUniqueID
				};

				return innerSink.OnMessage(testFailed);
			}

			// TODO: Shouldn't there be conversions of all the finished messages up the stack, to rectify the counts?

			if (message is _TestCollectionFinished testCollectionFinished)
			{
				testCollectionFinished = new _TestCollectionFinished
				{
					AssemblyUniqueID = testCollectionFinished.AssemblyUniqueID,
					ExecutionTime = testCollectionFinished.ExecutionTime,
					TestCollectionUniqueID = testCollectionFinished.TestCollectionUniqueID,
					TestsFailed = testCollectionFinished.TestsFailed + testCollectionFinished.TestsSkipped,
					TestsRun = testCollectionFinished.TestsRun,
					TestsSkipped = 0
				};

				return innerSink.OnMessage(testCollectionFinished);
			}

			if (message is _TestAssemblyFinished assemblyFinished)
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
