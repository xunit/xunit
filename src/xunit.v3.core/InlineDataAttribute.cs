#pragma warning disable CA1019 // The attribute arguments are always read via reflection

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Provides a data source for a data theory, with the data coming from inline values.
/// </summary>
[DataDiscoverer(typeof(InlineDataDiscoverer))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class InlineDataAttribute : DataAttribute
{
	readonly object?[] data;

	/// <summary>
	/// Initializes a new instance of the <see cref="InlineDataAttribute"/> class.
	/// </summary>
	/// <param name="data">The data values to pass to the theory.</param>
	public InlineDataAttribute(params object?[] data)
	{
		this.data = data;
	}

	/// <inheritdoc/>
	public override ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(
		MethodInfo testMethod,
		DisposalTracker disposalTracker) =>
			new(new[] { new TheoryDataRow(data) });
}
