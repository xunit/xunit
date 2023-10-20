using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message indicates that a test is about to start executing.
/// </summary>
public class _TestStarting : _TestMessage, _ITestMetadata
{
	string? testDisplayName;
	IReadOnlyDictionary<string, IReadOnlyList<string>> traits = new Dictionary<string, IReadOnlyList<string>>();

	/// <inheritdoc/>
	public bool Explicit { get; set; }

	/// <inheritdoc/>
	public string TestDisplayName
	{
		get => this.ValidateNullablePropertyValue(testDisplayName, nameof(TestDisplayName));
		set => testDisplayName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestDisplayName));
	}

	/// <inheritdoc/>
	public int Timeout { get; set; }

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits
	{
		get => traits;
		set => traits = value ?? new Dictionary<string, IReadOnlyList<string>>();
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} name={1}", base.ToString(), testDisplayName.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidateNullableProperty(testDisplayName, nameof(TestDisplayName), invalidProperties);
	}
}
