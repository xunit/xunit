using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="ITestMethodOrderer"/> that does not change the order.
/// </summary>
[method: Obsolete("Please use the singleton instance available via the Instance property")]
[method: EditorBrowsable(EditorBrowsableState.Never)]
public class UnorderedTestMethodOrderer() : ITestMethodOrderer
{
	/// <summary>
	/// Get the singleton instance of <see cref="UnorderedTestMethodOrderer"/>.
	/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
	public static UnorderedTestMethodOrderer Instance { get; } = new();
#pragma warning restore CS0618 // Type or member is obsolete

	/// <inheritdoc/>
	public IReadOnlyCollection<TTestMethod?> OrderTestMethods<TTestMethod>(IReadOnlyCollection<TTestMethod?> testMethods)
		where TTestMethod : notnull, ITestMethod =>
			testMethods;
}
