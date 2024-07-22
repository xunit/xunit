using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

public partial class TestCaseDiscovered
{
	string? serialization;
	string? testCaseDisplayName;
	string? testClassName;
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits;

	/// <inheritdoc/>
	public required string Serialization
	{
		get => this.ValidateNullablePropertyValue(serialization, nameof(Serialization));
		set => serialization = Guard.ArgumentNotNull(value, nameof(Serialization));
	}

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

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(serialization, nameof(Serialization), invalidProperties);
		ValidatePropertyIsNotNull(testCaseDisplayName, nameof(TestCaseDisplayName), invalidProperties);
		ValidatePropertyIsNotNull(traits, nameof(Traits), invalidProperties);

		if (TestMethodName is not null)
			ValidatePropertyIsNotNull(testClassName, nameof(TestClassName), invalidProperties);
	}
}
