using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test method is about to begin executing.
/// </summary>
[JsonTypeID("test-method-starting")]
public sealed class TestMethodStarting : TestMethodMessage, ITestMethodMetadata, IWritableTestMethodMetadata
{
	string? testMethod;
	IReadOnlyDictionary<string, IReadOnlyList<string>>? traits;

	/// <inheritdoc/>
	public string MethodName
	{
		get => this.ValidateNullablePropertyValue(testMethod, nameof(MethodName));
		set => testMethod = Guard.ArgumentNotNullOrEmpty(value, nameof(MethodName));
	}

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits
	{
		get => this.ValidateNullablePropertyValue(traits, nameof(Traits));
		set => traits = Guard.ArgumentNotNull(value, nameof(Traits));
	}

	string ITestMethodMetadata.UniqueID =>
		this.ValidateNullablePropertyValue(TestMethodUniqueID, nameof(TestMethodUniqueID));

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		root.DeserializeTestMethodMetadata(this);
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

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

		ValidatePropertyIsNotNull(testMethod, nameof(MethodName), invalidProperties);
		ValidatePropertyIsNotNull(traits, nameof(Traits), invalidProperties);
	}
}
