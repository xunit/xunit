using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message indicates that a test class is about to begin executing.
/// </summary>
public class _TestClassStarting : _TestClassMessage, _ITestClassMetadata
{
	string? testClass;

	/// <inheritdoc/>
	public string TestClass
	{
		get => this.ValidateNullablePropertyValue(testClass, nameof(TestClass));
		set => testClass = Guard.ArgumentNotNullOrEmpty(value, nameof(TestClass));
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} class={1}", base.ToString(), testClass.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidateNullableProperty(testClass, nameof(TestClass), invalidProperties);
	}
}
