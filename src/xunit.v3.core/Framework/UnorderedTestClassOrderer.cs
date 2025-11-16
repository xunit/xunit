using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="ITestClassOrderer"/> that does not change the order.
/// </summary>
[method: Obsolete("Please use the singleton instance available via the Instance property")]
[method: EditorBrowsable(EditorBrowsableState.Never)]
public class UnorderedTestClassOrderer() : ITestClassOrderer
{
	/// <summary>
	/// Get the singleton instance of <see cref="UnorderedTestClassOrderer"/>.
	/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
	public static UnorderedTestClassOrderer Instance { get; } = new();
#pragma warning restore CS0618 // Type or member is obsolete

	/// <inheritdoc/>
	public IReadOnlyCollection<TTestClass?> OrderTestClasses<TTestClass>(IReadOnlyCollection<TTestClass?> testClasses)
		where TTestClass : notnull, ITestClass =>
			testClasses;
}
