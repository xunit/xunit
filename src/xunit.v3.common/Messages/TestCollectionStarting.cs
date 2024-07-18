using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test collection is about to start executing.
/// </summary>
[JsonTypeID("test-collection-starting")]
public sealed class TestCollectionStarting : TestCollectionMessage, ITestCollectionMetadata
{
	string? testCollectionDisplayName;
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits;

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

	string ITestCollectionMetadata.UniqueID =>
		TestCollectionUniqueID;

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		TestCollectionClassName = JsonDeserializer.TryGetString(root, nameof(TestCollectionClassName));
		testCollectionDisplayName = JsonDeserializer.TryGetString(root, nameof(TestCollectionDisplayName));
		traits = JsonDeserializer.TryGetTraits(root, nameof(Traits));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(TestCollectionClassName), TestCollectionClassName);
		serializer.Serialize(nameof(TestCollectionDisplayName), TestCollectionDisplayName);
		serializer.SerializeTraits(nameof(Traits), Traits);
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
