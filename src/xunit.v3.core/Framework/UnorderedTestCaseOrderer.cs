using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="ITestCaseOrderer"/> that does not change the order.
/// </summary>
[method: Obsolete("Please use the singleton instance available via the Instance property")]
[method: EditorBrowsable(EditorBrowsableState.Never)]
public class UnorderedTestCaseOrderer() : ITestCaseOrderer
{
	/// <summary>
	/// Get the singleton instance of <see cref="UnorderedTestCaseOrderer"/>.
	/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
	public static UnorderedTestCaseOrderer Instance { get; } = new();
#pragma warning restore CS0618 // Type or member is obsolete

	/// <inheritdoc/>
	public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
		where TTestCase : notnull, ITestCase =>
			testCases;
}
