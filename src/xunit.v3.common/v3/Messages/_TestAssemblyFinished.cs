using System;
using System.Collections.Generic;
using Xunit.Internal;

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
		get => this.ValidateNullablePropertyValue(executionTime, nameof(ExecutionTime));
		set => executionTime = value;
	}

	/// <summary>
	/// Gets or sets the date and time when the test assembly execution finished.
	/// </summary>
	public DateTimeOffset FinishTime { get; set; }

	/// <inheritdoc/>
	public int TestsFailed
	{
		get => this.ValidateNullablePropertyValue(testsFailed, nameof(TestsFailed));
		set => testsFailed = value;
	}

	/// <inheritdoc/>
	public int TestsNotRun
	{
		get => this.ValidateNullablePropertyValue(testsNotRun, nameof(TestsNotRun));
		set => testsNotRun = value;
	}

	/// <inheritdoc/>
	public int TestsSkipped
	{
		get => this.ValidateNullablePropertyValue(testsSkipped, nameof(TestsSkipped));
		set => testsSkipped = value;
	}

	/// <inheritdoc/>
	public int TestsTotal
	{
		get => this.ValidateNullablePropertyValue(testsTotal, nameof(TestsTotal));
		set => testsTotal = value;
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidateNullableProperty(executionTime, nameof(ExecutionTime), invalidProperties);
		ValidateNullableProperty(testsFailed, nameof(TestsFailed), invalidProperties);
		ValidateNullableProperty(testsNotRun, nameof(TestsNotRun), invalidProperties);
		ValidateNullableProperty(testsSkipped, nameof(TestsSkipped), invalidProperties);
		ValidateNullableProperty(testsTotal, nameof(TestsTotal), invalidProperties);
	}
}
