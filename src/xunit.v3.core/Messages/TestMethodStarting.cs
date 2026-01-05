using System;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class TestMethodStarting
{
	string? methodName;
	DateTimeOffset? startTime;
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits;

	/// <inheritdoc/>
	public required int? MethodArity { get; set; }

	/// <inheritdoc/>
	public required string MethodName
	{
		get => this.ValidateNullablePropertyValue(methodName, nameof(MethodName));
		set => methodName = Guard.ArgumentNotNullOrEmpty(value, nameof(MethodName));
	}

	/// <inheritdoc/>
	public required DateTimeOffset StartTime
	{
		get => this.ValidateNullablePropertyValue(startTime, nameof(StartTime));
		set => startTime = value;
	}

	/// <inheritdoc/>
	public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits
	{
		get => this.ValidateNullablePropertyValue(traits, nameof(Traits));
		set => traits = Guard.ArgumentNotNull(value, nameof(Traits));
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(methodName, nameof(MethodName), invalidProperties);
		ValidatePropertyIsNotNull(startTime, nameof(StartTime), invalidProperties);
		ValidatePropertyIsNotNull(traits, nameof(Traits), invalidProperties);
	}
}
