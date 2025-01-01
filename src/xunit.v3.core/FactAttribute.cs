#pragma warning disable CA1813 // This attribute is unsealed because it's an extensibility point

using System;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Attribute that is applied to a method to indicate that it is a fact that should be run
/// by the default test runner.
/// </summary>
[XunitTestCaseDiscoverer(typeof(FactDiscoverer))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FactAttribute : Attribute, IFactAttribute
{
	/// <inheritdoc/>
	public string? DisplayName { get; set; }

	/// <inheritdoc/>
	public bool Explicit { get; set; }

	/// <inheritdoc/>
	public string? Skip { get; set; }

	/// <inheritdoc/>
	public Type[]? SkipExceptions { get; set; }

	/// <inheritdoc/>
	public Type? SkipType { get; set; }

	/// <inheritdoc/>
	public string? SkipUnless { get; set; }

	/// <inheritdoc/>
	public string? SkipWhen { get; set; }

	/// <inheritdoc/>
	public int Timeout { get; set; }
}
