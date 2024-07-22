using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class TestMessage
{
	string? testUniqueID;

	/// <inheritdoc/>
	public required string TestUniqueID
	{
		get => this.ValidateNullablePropertyValue(testUniqueID, nameof(TestUniqueID));
		set => testUniqueID = Guard.ArgumentNotNullOrEmpty(value, nameof(TestUniqueID));
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(testUniqueID, nameof(TestUniqueID), invalidProperties);
	}
}
