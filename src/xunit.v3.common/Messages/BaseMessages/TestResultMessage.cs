using System;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This is the base message for all individual test results (e.g., tests which
/// pass, fail, or are skipped).
/// </summary>
public abstract class TestResultMessage : TestMessage, IExecutionMetadata
{
	decimal? executionTime;
	DateTimeOffset? finishTime;
	string? output;

	/// <inheritdoc/>
	public required decimal ExecutionTime
	{
		get => this.ValidateNullablePropertyValue(executionTime, nameof(ExecutionTime));
		set => executionTime = value;
	}

	/// <summary>
	/// Gets or sets the date and time when the test execution finished.
	/// </summary>
	public required DateTimeOffset FinishTime
	{
		get => this.ValidateNullablePropertyValue(finishTime, nameof(FinishTime));
		set => finishTime = value;
	}

	/// <inheritdoc/>
	public required string Output
	{
		get => this.ValidateNullablePropertyValue(output, nameof(Output));
		set => output = Guard.ArgumentNotNull(value, nameof(Output));
	}

	/// <inheritdoc/>
	public required string[]? Warnings { get; set; }

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		executionTime = JsonDeserializer.TryGetDecimal(root, nameof(ExecutionTime));
		finishTime = JsonDeserializer.TryGetDateTimeOffset(root, nameof(FinishTime));
		output = JsonDeserializer.TryGetString(root, nameof(Output));
		Warnings = JsonDeserializer.TryGetArrayOfString(root, nameof(Warnings));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(ExecutionTime), ExecutionTime);
		serializer.Serialize(nameof(FinishTime), FinishTime);
		serializer.Serialize(nameof(Output), Output);
		serializer.SerializeStringArray(nameof(Warnings), Warnings);
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(executionTime, nameof(ExecutionTime), invalidProperties);
		ValidatePropertyIsNotNull(finishTime, nameof(FinishTime), invalidProperties);
		ValidatePropertyIsNotNull(output, nameof(Output), invalidProperties);
	}
}
