using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

public partial class TestMethodStarting
{
	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="MessageSinkMessage.UnsetStringPropertyValue"/> if there was no value provided during deserialization.
	/// </remarks>
	public required string MethodName { get; set; } = UnsetStringPropertyValue;

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

		MethodName = JsonDeserializer.TryGetString(root, nameof(MethodName)) ?? MethodName;
		Traits = JsonDeserializer.TryGetTraits(root, nameof(Traits)) ?? Traits;
	}
}
