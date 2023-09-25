using System;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.Common;

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
		Guard.ArgumentNotNull(innerSink);

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
			return;

		disposed = true;

		GC.SuppressFinalize(this);

		innerSink.Dispose();
	}

	/// <inheritdoc/>
	public bool OnMessage(_MessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		if (message is _TestSkipped testSkipped)
		{
			var testFailed = new _TestFailed
			{
				AssemblyUniqueID = testSkipped.AssemblyUniqueID,
				Cause = FailureCause.Other,
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
				TestUniqueID = testSkipped.TestUniqueID,
				Warnings = testSkipped.Warnings,
			};

			return innerSink.OnMessage(testFailed);
		}

		if (message is _TestCaseFinished testCaseFinished)
		{
			testCaseFinished = new _TestCaseFinished
			{
				AssemblyUniqueID = testCaseFinished.AssemblyUniqueID,
				ExecutionTime = testCaseFinished.ExecutionTime,
				TestCaseUniqueID = testCaseFinished.TestCaseUniqueID,
				TestClassUniqueID = testCaseFinished.TestClassUniqueID,
				TestCollectionUniqueID = testCaseFinished.TestCollectionUniqueID,
				TestMethodUniqueID = testCaseFinished.TestMethodUniqueID,
				TestsFailed = testCaseFinished.TestsFailed + testCaseFinished.TestsSkipped,
				TestsNotRun = testCaseFinished.TestsNotRun,
				TestsTotal = testCaseFinished.TestsTotal,
				TestsSkipped = 0,
			};

			return innerSink.OnMessage(testCaseFinished);
		}

		if (message is _TestMethodFinished testMethodFinished)
		{
			testMethodFinished = new _TestMethodFinished
			{
				AssemblyUniqueID = testMethodFinished.AssemblyUniqueID,
				ExecutionTime = testMethodFinished.ExecutionTime,
				TestClassUniqueID = testMethodFinished.TestClassUniqueID,
				TestCollectionUniqueID = testMethodFinished.TestCollectionUniqueID,
				TestMethodUniqueID = testMethodFinished.TestMethodUniqueID,
				TestsFailed = testMethodFinished.TestsFailed + testMethodFinished.TestsSkipped,
				TestsNotRun = testMethodFinished.TestsNotRun,
				TestsTotal = testMethodFinished.TestsTotal,
				TestsSkipped = 0,
			};

			return innerSink.OnMessage(testMethodFinished);
		}

		if (message is _TestClassFinished testClassFinished)
		{
			testClassFinished = new _TestClassFinished
			{
				AssemblyUniqueID = testClassFinished.AssemblyUniqueID,
				ExecutionTime = testClassFinished.ExecutionTime,
				TestClassUniqueID = testClassFinished.TestClassUniqueID,
				TestCollectionUniqueID = testClassFinished.TestCollectionUniqueID,
				TestsFailed = testClassFinished.TestsFailed + testClassFinished.TestsSkipped,
				TestsNotRun = testClassFinished.TestsNotRun,
				TestsTotal = testClassFinished.TestsTotal,
				TestsSkipped = 0,
			};

			return innerSink.OnMessage(testClassFinished);
		}

		if (message is _TestCollectionFinished testCollectionFinished)
		{
			testCollectionFinished = new _TestCollectionFinished
			{
				AssemblyUniqueID = testCollectionFinished.AssemblyUniqueID,
				ExecutionTime = testCollectionFinished.ExecutionTime,
				TestCollectionUniqueID = testCollectionFinished.TestCollectionUniqueID,
				TestsFailed = testCollectionFinished.TestsFailed + testCollectionFinished.TestsSkipped,
				TestsNotRun = testCollectionFinished.TestsNotRun,
				TestsTotal = testCollectionFinished.TestsTotal,
				TestsSkipped = 0,
			};

			return innerSink.OnMessage(testCollectionFinished);
		}

		if (message is _TestAssemblyFinished assemblyFinished)
		{
			assemblyFinished = new _TestAssemblyFinished
			{
				AssemblyUniqueID = assemblyFinished.AssemblyUniqueID,
				ExecutionTime = assemblyFinished.ExecutionTime,
				FinishTime = assemblyFinished.FinishTime,
				TestsFailed = assemblyFinished.TestsFailed + assemblyFinished.TestsSkipped,
				TestsNotRun = assemblyFinished.TestsNotRun,
				TestsTotal = assemblyFinished.TestsTotal,
				TestsSkipped = 0,
			};

			return innerSink.OnMessage(assemblyFinished);
		}

		return innerSink.OnMessage(message);
	}
}
