using System;
using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

public partial class TestStarting
{
	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>false</c> if there was no value provided during deserialization.
	/// </remarks>
	public required bool Explicit { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="DateTimeOffset.MinValue"/> if there was no value provided during deserialization.
	/// </remarks>
	public required DateTimeOffset StartTime { get; set; } = DateTimeOffset.MinValue;

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="MessageSinkMessage.UnsetStringPropertyValue"/> if there was no value provided during deserialization.
	/// </remarks>
	public required string TestDisplayName { get; set; } = UnsetStringPropertyValue;

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>0</c> if there was no value provided during deserialization.
	/// </remarks>
	public required int Timeout { get; set; }

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

		Explicit = JsonDeserializer.TryGetBoolean(root, nameof(Explicit)) ?? Explicit;
		StartTime = JsonDeserializer.TryGetDateTimeOffset(root, nameof(StartTime)) ?? StartTime;
		TestDisplayName = JsonDeserializer.TryGetString(root, nameof(TestDisplayName)) ?? TestDisplayName;
		Timeout = JsonDeserializer.TryGetInt(root, nameof(Timeout)) ?? Timeout;
		Traits = JsonDeserializer.TryGetTraits(root, nameof(Traits)) ?? Traits;
	}
}
