using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message is sent during execution to indicate that the Before method of a
/// <see cref="T:Xunit.Sdk.BeforeAfterTestAttribute"/> is about to execute.
/// </summary>
public class _BeforeTestStarting : _TestMessage
{
	string? attributeName;

	/// <summary>
	/// Gets or sets the fully qualified type name of the <see cref="T:Xunit.Sdk.BeforeAfterTestAttribute"/>.
	/// </summary>
	public string AttributeName
	{
		get => this.ValidateNullablePropertyValue(attributeName, nameof(AttributeName));
		set => attributeName = Guard.ArgumentNotNull(value, nameof(AttributeName));
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} attr={1}", base.ToString(), attributeName.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidateNullableProperty(attributeName, nameof(AttributeName), invalidProperties);
	}
}
