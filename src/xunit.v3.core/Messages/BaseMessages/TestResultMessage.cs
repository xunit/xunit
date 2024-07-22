using System;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class TestResultMessage
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

	/// <inheritdoc/>
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
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(executionTime, nameof(ExecutionTime), invalidProperties);
		ValidatePropertyIsNotNull(finishTime, nameof(FinishTime), invalidProperties);
		ValidatePropertyIsNotNull(output, nameof(Output), invalidProperties);
	}
}
