using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message is sent during execution to indicate that the After method of a
/// <see cref="T:Xunit.v3.IBeforeAfterTestAttribute"/> has completed executing.
/// </summary>
[JsonTypeID("after-test-finished")]
public sealed class _AfterTestFinished : _TestMessage
{
	string? attributeName;

	/// <summary>
	/// Gets or sets the fully qualified type name of the <see cref="T:Xunit.v3.IBeforeAfterTestAttribute"/>.
	/// </summary>
	public string AttributeName
	{
		get => this.ValidateNullablePropertyValue(attributeName, nameof(AttributeName));
		set => attributeName = Guard.ArgumentNotNull(value, nameof(AttributeName));
	}

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		attributeName = JsonDeserializer.TryGetString(root, nameof(AttributeName));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(AttributeName), AttributeName);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} attr={1}", base.ToString(), attributeName.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(attributeName, nameof(AttributeName), invalidProperties);
	}
}
