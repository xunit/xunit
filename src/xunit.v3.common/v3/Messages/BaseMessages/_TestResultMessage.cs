using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This is the base message for all individual test results (e.g., tests which
/// pass, fail, or are skipped).
/// </summary>
public class _TestResultMessage : _TestMessage, _IExecutionMetadata
{
	decimal? executionTime;
	string? output;

	/// <inheritdoc/>
	public decimal ExecutionTime
	{
		get => this.ValidateNullablePropertyValue(executionTime, nameof(ExecutionTime));
		set => executionTime = value;
	}

	/// <inheritdoc/>
	public string Output
	{
		get => this.ValidateNullablePropertyValue(output, nameof(Output));
		set => output = Guard.ArgumentNotNull(value, nameof(Output));
	}
}
