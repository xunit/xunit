using System;
using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

public partial class TestResultMessage
{
	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>0</c> if there was no value provided during deserialization.
	/// </remarks>
	public required decimal ExecutionTime { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="DateTimeOffset.MinValue"/> if there was no value provided during deserialization.
	/// </remarks>
	public required DateTimeOffset FinishTime { get; set; } = DateTimeOffset.MinValue;

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="MessageSinkMessage.UnsetStringPropertyValue"/> if there was no value provided during deserialization.
	/// </remarks>
	public required string Output { get; set; } = UnsetStringPropertyValue;

	/// <inheritdoc/>
	public required string[]? Warnings { get; set; }

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		ExecutionTime = JsonDeserializer.TryGetDecimal(root, nameof(ExecutionTime)) ?? ExecutionTime;
		FinishTime = JsonDeserializer.TryGetDateTimeOffset(root, nameof(FinishTime)) ?? FinishTime;
		Output = JsonDeserializer.TryGetString(root, nameof(Output), defaultEmptyString: true) ?? Output;
		Warnings = JsonDeserializer.TryGetArrayOfString(root, nameof(Warnings));
	}
}
