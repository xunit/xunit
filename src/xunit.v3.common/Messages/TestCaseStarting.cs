using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test case is about to start executing.
/// </summary>
[JsonTypeID("test-case-starting")]
public sealed class TestCaseStarting : TestCaseMessage, ITestCaseMetadata
{
	string? testCaseDisplayName;
	string? testClassName;
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits;

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
	public required string? TestClassName
	{
		get
		{
			if (testClassName is null && TestMethodName is not null)
				throw new InvalidOperationException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Illegal null {0} on an instance of '{1}' when {2} is not null",
						nameof(TestClassName),
						GetType().SafeName(),
						nameof(TestMethodName)
					)
				);

			return testClassName;
		}
		set => testClassName = value;
	}

	/// <inheritdoc/>
	public required string? TestClassNamespace { get; set; }

	/// <inheritdoc/>
	public required int? TestMethodMetadataToken { get; set; }

	/// <inheritdoc/>
	public required string? TestMethodName { get; set; }

	/// <inheritdoc/>
	public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits
	{
		get => this.ValidateNullablePropertyValue(traits, nameof(Traits));
		set => traits = Guard.ArgumentNotNull(value, nameof(Traits));
	}

	string ITestCaseMetadata.UniqueID =>
		TestCaseUniqueID;

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		SkipReason = JsonDeserializer.TryGetString(root, nameof(SkipReason));
		SourceFilePath = JsonDeserializer.TryGetString(root, nameof(SourceFilePath));
		SourceLineNumber = JsonDeserializer.TryGetInt(root, nameof(SourceLineNumber));
		testCaseDisplayName = JsonDeserializer.TryGetString(root, nameof(TestCaseDisplayName));
		TestClassMetadataToken = JsonDeserializer.TryGetInt(root, nameof(TestClassMetadataToken));
		testClassName = JsonDeserializer.TryGetString(root, nameof(TestClassName));
		TestClassNamespace = JsonDeserializer.TryGetString(root, nameof(TestClassNamespace));
		TestMethodMetadataToken = JsonDeserializer.TryGetInt(root, nameof(TestMethodMetadataToken));
		TestMethodName = JsonDeserializer.TryGetString(root, nameof(TestMethodName));
		traits = JsonDeserializer.TryGetTraits(root, nameof(Traits));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(SkipReason), SkipReason);
		serializer.Serialize(nameof(SourceFilePath), SourceFilePath);
		serializer.Serialize(nameof(SourceLineNumber), SourceLineNumber);
		serializer.Serialize(nameof(TestCaseDisplayName), TestCaseDisplayName);
		serializer.Serialize(nameof(TestClassMetadataToken), TestClassMetadataToken);
		serializer.Serialize(nameof(TestClassName), TestClassName);
		serializer.Serialize(nameof(TestClassNamespace), TestClassNamespace);
		serializer.Serialize(nameof(TestMethodMetadataToken), TestMethodMetadataToken);
		serializer.Serialize(nameof(TestMethodName), TestMethodName);
		serializer.SerializeTraits(nameof(Traits), Traits);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} name={1}", base.ToString(), testCaseDisplayName.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(testCaseDisplayName, nameof(TestCaseDisplayName), invalidProperties);
		ValidatePropertyIsNotNull(traits, nameof(Traits), invalidProperties);

		if (TestMethodName is not null)
			ValidatePropertyIsNotNull(testClassName, nameof(TestClassName), invalidProperties);
	}
}
