using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test case has finished executing.
/// </summary>
[JsonTypeID("test-case-finished")]
public sealed class TestCaseFinished : TestCaseMessage, IExecutionSummaryMetadata
{
	decimal? executionTime;
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
		ValidatePropertyIsNotNull(testsFailed, nameof(TestsFailed), invalidProperties);
		ValidatePropertyIsNotNull(testsNotRun, nameof(TestsNotRun), invalidProperties);
		ValidatePropertyIsNotNull(testsSkipped, nameof(TestsSkipped), invalidProperties);
		ValidatePropertyIsNotNull(testsTotal, nameof(TestsTotal), invalidProperties);
	}
}
