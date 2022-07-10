using System;

namespace Xunit.v3;

/// <summary>
/// This message indicates that the execution process has been completed for
/// the requested assembly.
/// </summary>
public class _TestAssemblyFinished : _TestAssemblyMessage, _IExecutionSummaryMetadata
{
	decimal? executionTime;
	int? testsFailed;
	int? testsNotRun;
	int? testsSkipped;
	int? testsTotal;

	/// <inheritdoc/>
	public decimal ExecutionTime
	{
		get => executionTime ?? throw new InvalidOperationException($"Attempted to get {nameof(ExecutionTime)} on an uninitialized '{GetType().FullName}' object");
		set => executionTime = value;
	}

	/// <inheritdoc/>
	public int TestsFailed
	{
		get => testsFailed ?? throw new InvalidOperationException($"Attempted to get {nameof(TestsFailed)} on an uninitialized '{GetType().FullName}' object");
		set => testsFailed = value;
	}

	/// <inheritdoc/>
	public int TestsNotRun
	{
		get => testsNotRun ?? throw new InvalidOperationException($"Attempted to get {nameof(TestsNotRun)} on an uninitialized '{GetType().FullName}' object");
		set => testsNotRun = value;
	}

	/// <inheritdoc/>
	public int TestsSkipped
	{
		get => testsSkipped ?? throw new InvalidOperationException($"Attempted to get {nameof(TestsSkipped)} on an uninitialized '{GetType().FullName}' object");
		set => testsSkipped = value;
	}

	/// <inheritdoc/>
	public int TestsTotal
	{
		get => testsTotal ?? throw new InvalidOperationException($"Attempted to get {nameof(TestsTotal)} on an uninitialized '{GetType().FullName}' object");
		set => testsTotal = value;
	}
}
