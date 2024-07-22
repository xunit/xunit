using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class TestCaseMessage
{
	string? testCaseUniqueID;

	/// <inheritdoc/>
	public required string TestCaseUniqueID
	{
		get => this.ValidateNullablePropertyValue(testCaseUniqueID, nameof(TestCaseUniqueID));
		set => testCaseUniqueID = Guard.ArgumentNotNullOrEmpty(value, nameof(TestCaseUniqueID));
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(testCaseUniqueID, nameof(TestCaseUniqueID), invalidProperties);
	}
}
