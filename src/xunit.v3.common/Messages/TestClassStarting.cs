using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test class is about to begin executing.
/// </summary>
[JsonTypeID("test-class-starting")]
public sealed class TestClassStarting : TestClassMessage, ITestClassMetadata, IWritableTestClassMetadata
{
	string? testClassName;
	IReadOnlyDictionary<string, IReadOnlyList<string>>? traits;

	/// <inheritdoc/>
	public string TestClassName
	{
		get => this.ValidateNullablePropertyValue(testClassName, nameof(TestClassName));
		set => testClassName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestClassName));
	}

	/// <inheritdoc/>
	public string? TestClassNamespace { get; set; }

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits
	{
		get => this.ValidateNullablePropertyValue(traits, nameof(Traits));
		set => traits = Guard.ArgumentNotNull(value, nameof(Traits));
	}

	string ITestClassMetadata.UniqueID =>
		this.ValidateNullablePropertyValue(TestClassUniqueID, nameof(TestClassUniqueID));

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		root.DeserializeTestClassMetadata(this);
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.SerializeTestClassMetadata(this);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} class={1}", base.ToString(), testClassName.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(testClassName, nameof(TestClassName), invalidProperties);
		ValidatePropertyIsNotNull(traits, nameof(Traits), invalidProperties);
	}
}
