using System;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class TestCollectionStarting
{
	DateTimeOffset? startTime;
	string? testCollectionDisplayName;
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits;

	/// <inheritdoc/>
	public required DateTimeOffset StartTime
	{
		get => this.ValidateNullablePropertyValue(startTime, nameof(StartTime));
		set => startTime = value;
	}

	/// <inheritdoc/>
	public required string? TestCollectionClassName { get; set; }

	/// <inheritdoc/>
	public required string TestCollectionDisplayName
	{
		get => this.ValidateNullablePropertyValue(testCollectionDisplayName, nameof(TestCollectionDisplayName));
		set => testCollectionDisplayName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestCollectionDisplayName));
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

		ValidatePropertyIsNotNull(startTime, nameof(StartTime), invalidProperties);
		ValidatePropertyIsNotNull(testCollectionDisplayName, nameof(TestCollectionDisplayName), invalidProperties);
		ValidatePropertyIsNotNull(traits, nameof(Traits), invalidProperties);
	}
}
