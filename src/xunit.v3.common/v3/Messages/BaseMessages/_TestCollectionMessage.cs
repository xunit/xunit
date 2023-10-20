using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// Base message for all messages related to test collections.
/// </summary>
public class _TestCollectionMessage : _TestAssemblyMessage
{
	string? testCollectionUniqueID;

	/// <summary>
	/// Gets the test collection's unique ID. Can be used to correlate test messages with the appropriate
	/// test collection that they're related to. Test collection metadata (as <see cref="_ITestCollectionMetadata"/>)
	/// is provided via <see cref="_TestCollectionStarting"/> (during execution) and should be cached as needed.
	/// </summary>
	public string TestCollectionUniqueID
	{
		get => this.ValidateNullablePropertyValue(testCollectionUniqueID, nameof(TestCollectionUniqueID));
		set => testCollectionUniqueID = Guard.ArgumentNotNullOrEmpty(value, nameof(TestCollectionUniqueID));
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0}({1})", GetType().Name, testCollectionUniqueID.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidateNullableProperty(testCollectionUniqueID, nameof(TestCollectionUniqueID), invalidProperties);
	}
}
