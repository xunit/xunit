using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message indicates that a test method is about to begin executing.
/// </summary>
[JsonTypeID("test-method-starting")]
public class _TestMethodStarting : _TestMethodMessage, _ITestMethodMetadata, _IWritableTestMethodMetadata
{
	string? testMethod;

	/// <inheritdoc/>
	public string TestMethod
	{
		get => this.ValidateNullablePropertyValue(testMethod, nameof(TestMethod));
		set => testMethod = Guard.ArgumentNotNullOrEmpty(value, nameof(TestMethod));
	}

	internal override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		root.DeserializeTestMethodMetadata(this);
	}

	internal override void Serialize(JsonObjectSerializer serializer)
	{
		base.Serialize(serializer);

		serializer.SerializeTestMethodMetadata(this);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} method={1}", base.ToString(), testMethod.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(testMethod, nameof(TestMethod), invalidProperties);
	}
}
