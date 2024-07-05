using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test collection is about to start executing.
/// </summary>
[JsonTypeID("test-collection-starting")]
public sealed class _TestCollectionStarting : _TestCollectionMessage, _ITestCollectionMetadata, _IWritableTestCollectionMetadata
{
	string? testCollectionDisplayName;
	IReadOnlyDictionary<string, IReadOnlyList<string>>? traits;

	/// <inheritdoc/>
	public string? TestCollectionClassName { get; set; }

	/// <inheritdoc/>
	public string TestCollectionDisplayName
	{
		get => this.ValidateNullablePropertyValue(testCollectionDisplayName, nameof(TestCollectionDisplayName));
		set => testCollectionDisplayName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestCollectionDisplayName));
	}

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits
	{
		get => this.ValidateNullablePropertyValue(traits, nameof(Traits));
		set => traits = Guard.ArgumentNotNull(value, nameof(Traits));
	}

	string _ITestCollectionMetadata.UniqueID =>
		TestCollectionUniqueID;

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		root.DeserializeTestCollectionMetadata(this);
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.SerializeTestCollectionMetadata(this);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} name={1} class={2}", base.ToString(), testCollectionDisplayName.Quoted(), TestCollectionClassName.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(testCollectionDisplayName, nameof(TestCollectionDisplayName), invalidProperties);
		ValidatePropertyIsNotNull(traits, nameof(Traits), invalidProperties);
	}
}
