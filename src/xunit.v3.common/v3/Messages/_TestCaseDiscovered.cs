using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message indicates that a test case had been found during the discovery process.
/// </summary>
public class _TestCaseDiscovered : _TestCaseMessage, _ITestCaseMetadata
{
	string? serialization;
	string? testCaseDisplayName;
	string? testClassName;
	string? testClassNameWithNamespace;
	IReadOnlyDictionary<string, IReadOnlyList<string>> traits = new Dictionary<string, IReadOnlyList<string>>();

	/// <summary>
	/// Gets the serialized value of the test case, which allows it to be transferred across
	/// process boundaries.
	/// </summary>
	public string Serialization
	{
		get => this.ValidateNullablePropertyValue(serialization, nameof(Serialization));
		set => serialization = Guard.ArgumentNotNull(value, nameof(Serialization));
	}

	/// <inheritdoc/>
	public string? SkipReason { get; set; }

	/// <inheritdoc/>
	public string? SourceFilePath { get; set; }

	/// <inheritdoc/>
	public int? SourceLineNumber { get; set; }

	/// <inheritdoc/>
	public string TestCaseDisplayName
	{
		get => this.ValidateNullablePropertyValue(testCaseDisplayName, nameof(TestCaseDisplayName));
		set => testCaseDisplayName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestCaseDisplayName));
	}

	/// <inheritdoc/>
	[NotNullIfNotNull(nameof(TestMethodName))]
	public string? TestClassName
	{
		get
		{
			if (testClassName is null && TestMethodName is not null)
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Illegal null {0} on an instance of '{1}' when {2} is not null", nameof(TestClassName), GetType().FullName, nameof(TestMethodName)));

			return testClassName;
		}
		set => testClassName = value;
	}

	/// <inheritdoc/>
	public string? TestClassNamespace { get; set; }

	/// <inheritdoc/>
	[NotNullIfNotNull(nameof(TestClassName))]
	public string? TestClassNameWithNamespace
	{
		get
		{
			if (testClassNameWithNamespace is null && testClassName is not null)
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Illegal null {0} on an instance of '{1}' when {2} is not null", nameof(TestClassNameWithNamespace), GetType().FullName, nameof(TestClassName)));

			return testClassNameWithNamespace;
		}
		set => testClassNameWithNamespace = value;
	}

	/// <inheritdoc/>
	public string? TestMethodName { get; set; }

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits
	{
		get => traits;
		set => traits = value ?? new Dictionary<string, IReadOnlyList<string>>();
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} name={1}", base.ToString(), testCaseDisplayName.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidateNullableProperty(serialization, nameof(Serialization), invalidProperties);
		ValidateNullableProperty(testCaseDisplayName, nameof(TestCaseDisplayName), invalidProperties);

		if (TestMethodName is not null)
			ValidateNullableProperty(testClassName, nameof(TestClassName), invalidProperties);
		if (testClassName is not null)
			ValidateNullableProperty(testClassNameWithNamespace, nameof(TestClassNameWithNamespace), invalidProperties);
	}
}
