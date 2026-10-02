using System.Reflection;
using Xunit.Sdk;

namespace Xunit;

partial class InlineDataAttribute
{
	/// <inheritdoc/>
	public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
		MethodInfo testMethod,
		DisposalTracker disposalTracker) =>
			new([ConvertDataRow(Data)]);

	/// <inheritdoc/>
	public override bool SupportsDiscoveryEnumeration() =>
		true;
}
