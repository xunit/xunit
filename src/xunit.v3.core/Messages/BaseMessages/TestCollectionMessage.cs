using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class TestCollectionMessage
{
	string? testCollectionUniqueID;

	/// <inheritdoc/>
	public required string TestCollectionUniqueID
	{
		get => this.ValidateNullablePropertyValue(testCollectionUniqueID, nameof(TestCollectionUniqueID));
		set => testCollectionUniqueID = Guard.ArgumentNotNullOrEmpty(value, nameof(TestCollectionUniqueID));
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(testCollectionUniqueID, nameof(TestCollectionUniqueID), invalidProperties);
	}
}
