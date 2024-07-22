using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

public partial class TestCaseCleanupFailure
{
	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be an empty array if there was no value provided during deserialization.
	/// </remarks>
	public required int[] ExceptionParentIndices { get; set; } = [];

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be an empty array if there was no value provided during deserialization.
	/// </remarks>
	public required string?[] ExceptionTypes { get; set; } = [];

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be an empty array if there was no value provided during deserialization.
	/// </remarks>
	public required string[] Messages { get; set; } = [];

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be an empty array if there was no value provided during deserialization.
	/// </remarks>
	public required string?[] StackTraces { get; set; } = [];

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		ExceptionParentIndices = JsonDeserializer.TryGetArrayOfInt(root, nameof(ExceptionParentIndices)) ?? ExceptionParentIndices;
		ExceptionTypes = JsonDeserializer.TryGetArrayOfNullableString(root, nameof(ExceptionTypes)) ?? ExceptionTypes;
		Messages = JsonDeserializer.TryGetArrayOfString(root, nameof(Messages)) ?? Messages;
		StackTraces = JsonDeserializer.TryGetArrayOfNullableString(root, nameof(StackTraces)) ?? StackTraces;
	}
}
