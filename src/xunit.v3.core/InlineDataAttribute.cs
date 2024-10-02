using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Provides a data source for a data theory, with the data coming from inline values.
/// </summary>
/// <param name="data">The data values to pass to the theory.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class InlineDataAttribute(params object?[]? data) : DataAttribute
{
	/// <summary>
	/// Gets the data to be passed to the test.
	/// </summary>
	// If the user passes null to the constructor, we assume what they meant was a
	// single null value to be passed to the test.
	public object?[] Data { get; } = data ?? [null];

	/// <inheritdoc/>
	public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
		MethodInfo testMethod,
		DisposalTracker disposalTracker)
	{
		var traits = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
		TestIntrospectionHelper.MergeTraitsInto(traits, Traits);

		return new([
			new TheoryDataRow(Data)
			{
				Explicit = ExplicitAsNullable,
				Skip = Skip,
				TestDisplayName = TestDisplayName,
				Timeout = TimeoutAsNullable,
				Traits = traits,
			}
		]);
	}

	/// <inheritdoc/>
	public override bool SupportsDiscoveryEnumeration() =>
		true;
}
