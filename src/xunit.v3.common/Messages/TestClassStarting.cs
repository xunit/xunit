using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test class is about to begin executing.
/// </summary>
[JsonTypeID("test-class-starting")]
public sealed class TestClassStarting : TestClassMessage, ITestClassMetadata
{
	string? testClassName;
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits;

	/// <inheritdoc/>
	public required string TestClassName
	{
		get => this.ValidateNullablePropertyValue(testClassName, nameof(TestClassName));
		set => testClassName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestClassName));
	}

	/// <inheritdoc/>
	public required string? TestClassNamespace { get; set; }

	/// <inheritdoc/>
	public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits
	{
		get => this.ValidateNullablePropertyValue(traits, nameof(Traits));
		set => traits = Guard.ArgumentNotNull(value, nameof(Traits));
	}

	string ITestClassMetadata.UniqueID =>
		this.ValidateNullablePropertyValue(TestClassUniqueID, nameof(TestClassUniqueID));

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		testClassName = JsonDeserializer.TryGetString(root, nameof(TestClassName));
		TestClassNamespace = JsonDeserializer.TryGetString(root, nameof(TestClassNamespace));
		traits = JsonDeserializer.TryGetTraits(root, nameof(Traits));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(TestClassName), TestClassName);
		serializer.Serialize(nameof(TestClassNamespace), TestClassNamespace);
		serializer.SerializeTraits(nameof(Traits), Traits);
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
