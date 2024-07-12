using System;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that the execution process has been completed for
/// the requested assembly.
/// </summary>
[JsonTypeID("test-assembly-finished")]
public sealed class TestAssemblyFinished : TestAssemblyMessage, IExecutionSummaryMetadata
{
	decimal? executionTime;
	DateTimeOffset? finishTime;
	int? testsFailed;
	int? testsNotRun;
	int? testsSkipped;
	int? testsTotal;

	/// <inheritdoc/>
	public required decimal ExecutionTime
	{
		get => this.ValidateNullablePropertyValue(executionTime, nameof(ExecutionTime));
		set => executionTime = value;
	}

	/// <summary>
	/// Gets or sets the date and time when the test assembly execution finished.
	/// </summary>
	public required DateTimeOffset FinishTime
	{
		get => this.ValidateNullablePropertyValue(finishTime, nameof(FinishTime));
		set => finishTime = value;
	}

	/// <inheritdoc/>
	public required int TestsFailed
	{
		get => this.ValidateNullablePropertyValue(testsFailed, nameof(TestsFailed));
		set => testsFailed = value;
	}

	/// <inheritdoc/>
	public required int TestsNotRun
	{
		get => this.ValidateNullablePropertyValue(testsNotRun, nameof(TestsNotRun));
		set => testsNotRun = value;
	}

	/// <inheritdoc/>
	public required int TestsSkipped
	{
		get => this.ValidateNullablePropertyValue(testsSkipped, nameof(TestsSkipped));
		set => testsSkipped = value;
	}

	/// <inheritdoc/>
	public required int TestsTotal
	{
		get => this.ValidateNullablePropertyValue(testsTotal, nameof(TestsTotal));
		set => testsTotal = value;
	}

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		executionTime = JsonDeserializer.TryGetDecimal(root, nameof(ExecutionTime));
		finishTime = JsonDeserializer.TryGetDateTimeOffset(root, nameof(FinishTime));
		testsFailed = JsonDeserializer.TryGetInt(root, nameof(TestsFailed));
		testsNotRun = JsonDeserializer.TryGetInt(root, nameof(TestsNotRun));
		testsSkipped = JsonDeserializer.TryGetInt(root, nameof(TestsSkipped));
		testsTotal = JsonDeserializer.TryGetInt(root, nameof(TestsTotal));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(ExecutionTime), ExecutionTime);
		serializer.Serialize(nameof(FinishTime), FinishTime);
		serializer.Serialize(nameof(TestsFailed), TestsFailed);
		serializer.Serialize(nameof(TestsNotRun), TestsNotRun);
		serializer.Serialize(nameof(TestsSkipped), TestsSkipped);
		serializer.Serialize(nameof(TestsTotal), TestsTotal);
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(executionTime, nameof(ExecutionTime), invalidProperties);
		ValidatePropertyIsNotNull(finishTime, nameof(FinishTime), invalidProperties);
		ValidatePropertyIsNotNull(testsFailed, nameof(TestsFailed), invalidProperties);
		ValidatePropertyIsNotNull(testsNotRun, nameof(TestsNotRun), invalidProperties);
		ValidatePropertyIsNotNull(testsSkipped, nameof(TestsSkipped), invalidProperties);
		ValidatePropertyIsNotNull(testsTotal, nameof(TestsTotal), invalidProperties);
	}
}
