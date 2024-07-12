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
public sealed class TestCaseStarting : TestCaseMessage, ITestCaseMetadata, IWritableTestCaseMetadata
{
	string? testCaseDisplayName;
	string? testClassName;
	IReadOnlyDictionary<string, IReadOnlyList<string>>? traits;

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
	public required string? TestMethodName { get; set; }

	/// <inheritdoc/>
	public required IReadOnlyDictionary<string, IReadOnlyList<string>> Traits
	{
		get => this.ValidateNullablePropertyValue(traits, nameof(Traits));
		set => traits = Guard.ArgumentNotNull(value, nameof(Traits));
	}

	string ITestCaseMetadata.UniqueID =>
		TestCaseUniqueID;

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		root.DeserializeTestCaseMetadata(this);
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.SerializeTestCaseMetadata(this);
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
