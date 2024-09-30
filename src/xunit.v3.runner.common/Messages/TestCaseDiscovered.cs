using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

public partial class TestCaseDiscovered
{
	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>false</c> if there was no value provided during deserialization.
	/// </remarks>
	public required bool Explicit { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="MessageSinkMessage.UnsetStringPropertyValue"/> if there was no value provided during deserialization.
	/// </remarks>
	public required string Serialization { get; set; } = UnsetStringPropertyValue;

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>null</c> if there was no value provided during deserialization.
	/// </remarks>
	public required string? SkipReason { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>null</c> if there was no value provided during deserialization.
	/// </remarks>
	public required string? SourceFilePath { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>null</c> if there was no value provided during deserialization.
	/// </remarks>
	public required int? SourceLineNumber { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="MessageSinkMessage.UnsetStringPropertyValue"/> if there was no value provided during deserialization.
	/// </remarks>
	public required string TestCaseDisplayName { get; set; } = UnsetStringPropertyValue;

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>null</c> if there was no value provided during deserialization.
	/// </remarks>
	public required int? TestClassMetadataToken { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>null</c> if there was no value provided during deserialization.
	/// </remarks>
	public required string? TestClassName { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>null</c> if there was no value provided during deserialization.
	/// </remarks>
	public required string? TestClassNamespace { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>null</c> if there was no value provided during deserialization.
	/// </remarks>
	public required string? TestClassSimpleName { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>null</c> if there was no value provided during deserialization.
	/// </remarks>
	public required int? TestMethodMetadataToken { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>null</c> if there was no value provided during deserialization.
	/// </remarks>
	public required string? TestMethodName { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>null</c> if there was no value provided during deserialization.
	/// </remarks>
	public required string[]? TestMethodParameterTypesVSTest { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>null</c> if there was no value provided during deserialization.
	/// </remarks>
	public required string? TestMethodReturnTypeVSTest { get; set; }

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

		Explicit = JsonDeserializer.TryGetBoolean(root, nameof(Explicit)) ?? false;
		Serialization = JsonDeserializer.TryGetString(root, nameof(Serialization)) ?? Serialization;
		SkipReason = JsonDeserializer.TryGetString(root, nameof(SkipReason));
		SourceFilePath = JsonDeserializer.TryGetString(root, nameof(SourceFilePath));
		SourceLineNumber = JsonDeserializer.TryGetInt(root, nameof(SourceLineNumber));
		TestCaseDisplayName = JsonDeserializer.TryGetString(root, nameof(TestCaseDisplayName)) ?? TestCaseDisplayName;
		TestClassMetadataToken = JsonDeserializer.TryGetInt(root, nameof(TestClassMetadataToken));
		TestClassName = JsonDeserializer.TryGetString(root, nameof(TestClassName));
		TestClassNamespace = JsonDeserializer.TryGetString(root, nameof(TestClassNamespace));
		TestClassSimpleName = JsonDeserializer.TryGetString(root, nameof(TestClassSimpleName));
		TestMethodMetadataToken = JsonDeserializer.TryGetInt(root, nameof(TestMethodMetadataToken));
		TestMethodName = JsonDeserializer.TryGetString(root, nameof(TestMethodName));
		TestMethodParameterTypesVSTest = JsonDeserializer.TryGetArrayOfString(root, nameof(TestMethodParameterTypesVSTest));
		TestMethodReturnTypeVSTest = JsonDeserializer.TryGetString(root, nameof(TestMethodReturnTypeVSTest));
		Traits = JsonDeserializer.TryGetTraits(root, nameof(Traits)) ?? Traits;
	}
}
