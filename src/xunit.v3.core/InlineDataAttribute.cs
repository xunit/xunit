using Xunit.v3;

namespace Xunit;

/// <summary>
/// Provides a data source for a data theory, with the data coming from inline values.
/// </summary>
/// <param name="data">The data values to pass to the theory.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed partial class InlineDataAttribute(params object?[]? data) : DataAttribute
{
	/// <summary>
	/// Gets the data to be passed to the test.
	/// </summary>
	// If the user passes null to the constructor, we assume what they meant was a
	// single null value to be passed to the test.
	public object?[] Data { get; } = data ?? [null];
}
