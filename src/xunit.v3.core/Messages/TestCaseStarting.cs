using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit.Internal;

namespace Xunit.v3;

public partial class TestCaseStarting
{
	string? testCaseDisplayName;
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits;

	/// <inheritdoc/>
	public required bool Explicit { get; set; }

	/// <inheritdoc/>
	public required string? SkipReason { get; set; }

	/// <inheritdoc/>
	public required string? SourceFilePath { get; set; }

	/// <inheritdoc/>
	public required int? SourceLineNumber { get; set; }

	/// <inheritdoc/>
	public required string TestCaseDisplayName
	{
		get => this.ValidateNullablePropertyValue(testCaseDisplayName, nameof(TestCaseDisplayName));
		set => testCaseDisplayName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestCaseDisplayName));
	}

	/// <inheritdoc/>
	public required int? TestClassMetadataToken { get; set; }

	/// <inheritdoc/>
	[NotNullIfNotNull(nameof(TestMethodName))]
	public required string? TestClassName { get; set; }

	/// <inheritdoc/>
	public required string? TestClassNamespace { get; set; }

	/// <inheritdoc/>
	[NotNullIfNotNull(nameof(TestMethodName))]
	public required string? TestClassSimpleName { get; set; }

	/// <inheritdoc/>
	public required int? TestMethodMetadataToken { get; set; }

	/// <inheritdoc/>
	public required string? TestMethodName { get; set; }

	/// <inheritdoc/>
	public required string[]? TestMethodParameterTypesVSTest { get; set; }

	/// <inheritdoc/>
	public required string? TestMethodReturnTypeVSTest { get; set; }

	/// <inheritdoc/>
	public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits
	{
		get => this.ValidateNullablePropertyValue(traits, nameof(Traits));
		set => traits = Guard.ArgumentNotNull(value, nameof(Traits));
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(testCaseDisplayName, nameof(TestCaseDisplayName), invalidProperties);
		ValidatePropertyIsNotNull(traits, nameof(Traits), invalidProperties);

		if (TestMethodName is not null)
			ValidatePropertyIsNotNull(TestClassName, nameof(TestClassName), invalidProperties);
	}
}
