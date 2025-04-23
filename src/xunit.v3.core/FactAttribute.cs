#pragma warning disable CA1813 // This attribute is unsealed because it's an extensibility point

using System;
using System.Runtime.CompilerServices;
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
	/// <summary>
	/// Createa a new instance of <see cref="FactAttribute"/> which provides source information.
	/// </summary>
	public FactAttribute([CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
	{
		SourceFilePath = sourceFilePath;
		SourceLineNumber = sourceLineNumber;
	}

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

	/// <inheritdoc/>
	public string? SourceFilePath { get; }

	/// <inheritdoc/>
	public int SourceLineNumber { get; }
}
