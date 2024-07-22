using System;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class TestStarting
{
	bool? @explicit;
	DateTimeOffset? startTime;
	string? testDisplayName;
	int? timeout;
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits;

	/// <inheritdoc/>
	public required bool Explicit
	{
		get => this.ValidateNullablePropertyValue(@explicit, nameof(Explicit));
		set => @explicit = value;
	}

	/// <inheritdoc/>
	public required DateTimeOffset StartTime
	{
		get => this.ValidateNullablePropertyValue(startTime, nameof(StartTime));
		set => startTime = value;
	}

	/// <inheritdoc/>
	public required string TestDisplayName
	{
		get => this.ValidateNullablePropertyValue(testDisplayName, nameof(TestDisplayName));
		set => testDisplayName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestDisplayName));
	}

	/// <inheritdoc/>
	public required int Timeout
	{
		get => this.ValidateNullablePropertyValue(timeout, nameof(Timeout));
		set => timeout = value;
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

		ValidatePropertyIsNotNull(@explicit, nameof(Explicit), invalidProperties);
		ValidatePropertyIsNotNull(startTime, nameof(StartTime), invalidProperties);
		ValidatePropertyIsNotNull(testDisplayName, nameof(TestDisplayName), invalidProperties);
		ValidatePropertyIsNotNull(timeout, nameof(Timeout), invalidProperties);
		ValidatePropertyIsNotNull(traits, nameof(Traits), invalidProperties);
	}
}
