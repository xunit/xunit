using System;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class TestClassStarting
{
	DateTimeOffset? startTime;
	string? testClassName;
	string? testClassSimpleName;
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits;

	/// <inheritdoc/>
	public required DateTimeOffset StartTime
	{
		get => this.ValidateNullablePropertyValue(startTime, nameof(StartTime));
		set => startTime = value;
	}

	/// <inheritdoc/>
	public required string TestClassName
	{
		get => this.ValidateNullablePropertyValue(testClassName, nameof(TestClassName));
		set => testClassName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestClassName));
	}

	/// <inheritdoc/>
	public required string? TestClassNamespace { get; set; }

	/// <inheritdoc/>
	public required string TestClassSimpleName
	{
		get => this.ValidateNullablePropertyValue(testClassSimpleName, nameof(TestClassSimpleName));
		set => testClassSimpleName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestClassSimpleName));
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
		ValidatePropertyIsNotNull(testClassName, nameof(TestClassName), invalidProperties);
		ValidatePropertyIsNotNull(traits, nameof(Traits), invalidProperties);
	}
}
