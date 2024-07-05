using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test is about to start executing.
/// </summary>
[JsonTypeID("test-starting")]
public sealed class _TestStarting : _TestMessage, _ITestMetadata, _IWritableTestMetadata
{
	string? testDisplayName;
	IReadOnlyDictionary<string, IReadOnlyList<string>>? traits;

	/// <inheritdoc/>
	public bool Explicit { get; set; }

	/// <inheritdoc/>
	public string TestDisplayName
	{
		get => this.ValidateNullablePropertyValue(testDisplayName, nameof(TestDisplayName));
		set => testDisplayName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestDisplayName));
	}

	/// <inheritdoc/>
	public int Timeout { get; set; }

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits
	{
		get => this.ValidateNullablePropertyValue(traits, nameof(Traits));
		set => traits = Guard.ArgumentNotNull(value, nameof(Traits));
	}

	string _ITestMetadata.UniqueID =>
		TestUniqueID;

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		root.DeserializeTestMetadata(this);

		if (JsonDeserializer.TryGetBoolean(root, nameof(Explicit)) is bool @explicit)
			Explicit = @explicit;
		if (JsonDeserializer.TryGetInt(root, nameof(Timeout)) is int timeout)
			Timeout = timeout;
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.SerializeTestMetadata(this);

		serializer.Serialize(nameof(Explicit), Explicit);
		serializer.Serialize(nameof(Timeout), Timeout);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} name={1}", base.ToString(), testDisplayName.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(testDisplayName, nameof(TestDisplayName), invalidProperties);
		ValidatePropertyIsNotNull(traits, nameof(Traits), invalidProperties);
	}
}
