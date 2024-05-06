using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message indicates that a test class is about to begin executing.
/// </summary>
[JsonTypeID("test-class-starting")]
public class _TestClassStarting : _TestClassMessage, _ITestClassMetadata, _IWritableTestClassMetadata
{
	string? testClass;

	/// <inheritdoc/>
	public string TestClass
	{
		get => this.ValidateNullablePropertyValue(testClass, nameof(TestClass));
		set => testClass = Guard.ArgumentNotNullOrEmpty(value, nameof(TestClass));
	}

	internal override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		root.DeserializeTestClassMetadata(this);
	}

	internal override void Serialize(JsonObjectSerializer serializer)
	{
		base.Serialize(serializer);

		serializer.SerializeTestClassMetadata(this);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} class={1}", base.ToString(), testClass.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(testClass, nameof(TestClass), invalidProperties);
	}
}
