using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// A delegating implementation of <see cref="IExecutionSink"/> which converts all
/// passing tests with warnings into failures before passing them on to the inner sink.
/// </summary>
public class DelegatingFailWarnSink : IExecutionSink
{
	bool disposed;
	readonly IExecutionSink innerSink;
	readonly Dictionary<string, int> failCountsByUniqueID = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="DelegatingFailWarnSink"/> class.
	/// </summary>
	/// <param name="innerSink">The sink to delegate messages to.</param>
	public DelegatingFailWarnSink(IExecutionSink innerSink)
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

		if (message is _TestPassed testPassed && testPassed.Warnings?.Length > 0)
		{
			var testFailed = new _TestFailed
			{
				AssemblyUniqueID = testPassed.AssemblyUniqueID,
				Cause = FailureCause.Other,
				ExceptionParentIndices = new[] { -1 },
				ExceptionTypes = new[] { "FAIL_WARN" },
				ExecutionTime = testPassed.ExecutionTime,
				Messages = new[] { "This test failed due to one or more warnings" },
				Output = testPassed.Output,
				StackTraces = new[] { "" },
				TestCaseUniqueID = testPassed.TestCaseUniqueID,
				TestClassUniqueID = testPassed.TestClassUniqueID,
				TestCollectionUniqueID = testPassed.TestCollectionUniqueID,
				TestMethodUniqueID = testPassed.TestMethodUniqueID,
				TestUniqueID = testPassed.TestUniqueID,
				Warnings = testPassed.Warnings,
			};

			lock (failCountsByUniqueID)
			{
				failCountsByUniqueID[testPassed.TestCaseUniqueID] = failCountsByUniqueID.GetOrAdd(testPassed.TestCaseUniqueID) + 1;
				if (testPassed.TestMethodUniqueID is not null)
					failCountsByUniqueID[testPassed.TestMethodUniqueID] = failCountsByUniqueID.GetOrAdd(testPassed.TestMethodUniqueID) + 1;
				if (testPassed.TestClassUniqueID is not null)
					failCountsByUniqueID[testPassed.TestClassUniqueID] = failCountsByUniqueID.GetOrAdd(testPassed.TestClassUniqueID) + 1;
				failCountsByUniqueID[testPassed.TestCollectionUniqueID] = failCountsByUniqueID.GetOrAdd(testPassed.TestCollectionUniqueID) + 1;
				failCountsByUniqueID[testPassed.AssemblyUniqueID] = failCountsByUniqueID.GetOrAdd(testPassed.AssemblyUniqueID) + 1;
			}

			return innerSink.OnMessage(testFailed);
		}

		if (message is _TestCaseFinished testCaseFinished)
		{
			int failedByCase;

			lock (failCountsByUniqueID)
				if (!failCountsByUniqueID.TryGetValue(testCaseFinished.TestCaseUniqueID, out failedByCase))
					failedByCase = 0;

			testCaseFinished = new _TestCaseFinished
			{
				AssemblyUniqueID = testCaseFinished.AssemblyUniqueID,
				ExecutionTime = testCaseFinished.ExecutionTime,
				TestCaseUniqueID = testCaseFinished.TestCaseUniqueID,
				TestClassUniqueID = testCaseFinished.TestClassUniqueID,
				TestCollectionUniqueID = testCaseFinished.TestCollectionUniqueID,
				TestMethodUniqueID = testCaseFinished.TestMethodUniqueID,
				TestsFailed = testCaseFinished.TestsFailed + failedByCase,
				TestsNotRun = testCaseFinished.TestsNotRun,
				TestsTotal = testCaseFinished.TestsTotal,
				TestsSkipped = testCaseFinished.TestsSkipped,
			};

			return innerSink.OnMessage(testCaseFinished);
		}

		if (message is _TestMethodFinished testMethodFinished)
		{
			int failedByMethod = 0;

			if (testMethodFinished.TestMethodUniqueID is not null)
				lock (failCountsByUniqueID)
					if (!failCountsByUniqueID.TryGetValue(testMethodFinished.TestMethodUniqueID, out failedByMethod))
						failedByMethod = 0;

			testMethodFinished = new _TestMethodFinished
			{
				AssemblyUniqueID = testMethodFinished.AssemblyUniqueID,
				ExecutionTime = testMethodFinished.ExecutionTime,
				TestClassUniqueID = testMethodFinished.TestClassUniqueID,
				TestCollectionUniqueID = testMethodFinished.TestCollectionUniqueID,
				TestMethodUniqueID = testMethodFinished.TestMethodUniqueID,
				TestsFailed = testMethodFinished.TestsFailed + failedByMethod,
				TestsNotRun = testMethodFinished.TestsNotRun,
				TestsTotal = testMethodFinished.TestsTotal,
				TestsSkipped = testMethodFinished.TestsSkipped,
			};

			return innerSink.OnMessage(testMethodFinished);
		}

		if (message is _TestClassFinished testClassFinished)
		{
			int failedByClass = 0;

			if (testClassFinished.TestClassUniqueID is not null)
				lock (failCountsByUniqueID)
					if (!failCountsByUniqueID.TryGetValue(testClassFinished.TestClassUniqueID, out failedByClass))
						failedByClass = 0;

			testClassFinished = new _TestClassFinished
			{
				AssemblyUniqueID = testClassFinished.AssemblyUniqueID,
				ExecutionTime = testClassFinished.ExecutionTime,
				TestClassUniqueID = testClassFinished.TestClassUniqueID,
				TestCollectionUniqueID = testClassFinished.TestCollectionUniqueID,
				TestsFailed = testClassFinished.TestsFailed + failedByClass,
				TestsNotRun = testClassFinished.TestsNotRun,
				TestsTotal = testClassFinished.TestsTotal,
				TestsSkipped = testClassFinished.TestsSkipped,
			};

			return innerSink.OnMessage(testClassFinished);
		}

		if (message is _TestCollectionFinished testCollectionFinished)
		{
			int failedByCollection;

			lock (failCountsByUniqueID)
				if (!failCountsByUniqueID.TryGetValue(testCollectionFinished.TestCollectionUniqueID, out failedByCollection))
					failedByCollection = 0;

			testCollectionFinished = new _TestCollectionFinished
			{
				AssemblyUniqueID = testCollectionFinished.AssemblyUniqueID,
				ExecutionTime = testCollectionFinished.ExecutionTime,
				TestCollectionUniqueID = testCollectionFinished.TestCollectionUniqueID,
				TestsFailed = testCollectionFinished.TestsFailed + failedByCollection,
				TestsNotRun = testCollectionFinished.TestsNotRun,
				TestsTotal = testCollectionFinished.TestsTotal,
				TestsSkipped = testCollectionFinished.TestsSkipped,
			};

			return innerSink.OnMessage(testCollectionFinished);
		}

		if (message is _TestAssemblyFinished assemblyFinished)
		{
			int failedByAssembly;

			lock (failCountsByUniqueID)
				if (!failCountsByUniqueID.TryGetValue(assemblyFinished.AssemblyUniqueID, out failedByAssembly))
					failedByAssembly = 0;

			assemblyFinished = new _TestAssemblyFinished
			{
				AssemblyUniqueID = assemblyFinished.AssemblyUniqueID,
				ExecutionTime = assemblyFinished.ExecutionTime,
				FinishTime = assemblyFinished.FinishTime,
				TestsFailed = assemblyFinished.TestsFailed + failedByAssembly,
				TestsNotRun = assemblyFinished.TestsNotRun,
				TestsTotal = assemblyFinished.TestsTotal,
				TestsSkipped = assemblyFinished.TestsSkipped,
			};

			return innerSink.OnMessage(assemblyFinished);
		}

		return innerSink.OnMessage(message);
	}
}
