using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

partial class InlineDataAttribute
{
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
				Label = Label,
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
