using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class TestMethodStarting
{
	string? methodName;
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits;

	/// <inheritdoc/>
	public required string MethodName
	{
		get => this.ValidateNullablePropertyValue(methodName, nameof(MethodName));
		set => methodName = Guard.ArgumentNotNullOrEmpty(value, nameof(MethodName));
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
		ValidatePropertyIsNotNull(traits, nameof(Traits), invalidProperties);
	}
}
