using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

public partial class TestClassStarting
{
	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="MessageSinkMessage.UnsetStringPropertyValue"/> if there was no value provided during deserialization.
	/// </remarks>
	public required string TestClassName { get; set; } = UnsetStringPropertyValue;

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>null</c> if there was no value provided during deserialization.
	/// </remarks>
	public required string? TestClassNamespace { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="MessageSinkMessage.UnsetStringPropertyValue"/> if there was no value provided during deserialization.
	/// </remarks>
	public required string TestClassSimpleName { get; set; } = UnsetStringPropertyValue;

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be an empty dictionary if there was no value provided during deserialization.
	/// </remarks>
	public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; } = EmptyTraits;

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		TestClassName = JsonDeserializer.TryGetString(root, nameof(TestClassName)) ?? TestClassName;
		TestClassNamespace = JsonDeserializer.TryGetString(root, nameof(TestClassNamespace));
		TestClassSimpleName = JsonDeserializer.TryGetString(root, nameof(TestClassSimpleName)) ?? TestClassSimpleName;
		Traits = JsonDeserializer.TryGetTraits(root, nameof(Traits)) ?? Traits;
	}
}
