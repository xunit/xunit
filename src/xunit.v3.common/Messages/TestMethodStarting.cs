using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test method is about to begin executing.
/// </summary>
[JsonTypeID("test-method-starting")]
public sealed class TestMethodStarting : TestMethodMessage, ITestMethodMetadata
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

	string ITestMethodMetadata.UniqueID =>
		this.ValidateNullablePropertyValue(TestMethodUniqueID, nameof(TestMethodUniqueID));

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		methodName = JsonDeserializer.TryGetString(root, nameof(MethodName));
		traits = JsonDeserializer.TryGetTraits(root, nameof(Traits));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(MethodName), MethodName);
		serializer.SerializeTraits(nameof(Traits), Traits);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} method={1}", base.ToString(), methodName.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(methodName, nameof(MethodName), invalidProperties);
		ValidatePropertyIsNotNull(traits, nameof(Traits), invalidProperties);
	}
}
