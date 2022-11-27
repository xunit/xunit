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
		$"{base.ToString()} class={testClass.Quoted()}";
}
