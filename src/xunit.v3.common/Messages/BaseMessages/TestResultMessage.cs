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
	string? output;

	/// <inheritdoc/>
	public required decimal ExecutionTime
	{
		get => this.ValidateNullablePropertyValue(executionTime, nameof(ExecutionTime));
		set => executionTime = value;
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
		ValidatePropertyIsNotNull(output, nameof(Output), invalidProperties);
	}
}
