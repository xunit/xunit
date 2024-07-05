using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test method has finished executing (meaning, all
/// the test cases that derived from the test method have finished).
/// </summary>
[JsonTypeID("test-method-finished")]
public sealed class _TestMethodFinished : _TestMethodMessage, _IExecutionSummaryMetadata, _IWritableExecutionSummaryMetadata
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
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		root.DeserializeExecutionSummaryMetadata(this);
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.SerializeExecutionSummaryMetadata(this);
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(executionTime, nameof(ExecutionTime), invalidProperties);
		ValidatePropertyIsNotNull(testsFailed, nameof(TestsFailed), invalidProperties);
		ValidatePropertyIsNotNull(testsNotRun, nameof(TestsNotRun), invalidProperties);
		ValidatePropertyIsNotNull(testsSkipped, nameof(TestsSkipped), invalidProperties);
		ValidatePropertyIsNotNull(testsTotal, nameof(TestsTotal), invalidProperties);
	}
}
