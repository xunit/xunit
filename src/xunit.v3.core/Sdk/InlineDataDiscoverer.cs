using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Implementation of <see cref="IDataDiscoverer"/> used to discover the data
/// provided by <see cref="InlineDataAttribute"/>.
/// </summary>
public class InlineDataDiscoverer : IDataDiscoverer
{
	/// <inheritdoc/>
	public virtual ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(
		_IAttributeInfo dataAttribute,
		_IMethodInfo testMethod,
		DisposalTracker disposalTracker)
	{
		Guard.ArgumentNotNull(dataAttribute);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(disposalTracker);

		// The data from GetConstructorArguments does not maintain its original form (in particular, collections
		// end up as generic IEnumerable<T>). So we end up needing to call .ToArray() on the enumerable so that
		// we can restore the correct argument type from InlineDataAttribute.
		//
		// In addition, [InlineData(null)] gets translated into passing a null array, not a single array with a null
		// value in it, which is why the null coalesce operator is required (this is covered by the acceptance test
		// in Xunit3TheoryAcceptanceTests.InlineDataTests.SingleNullValuesWork).

		var args = dataAttribute.GetConstructorArguments().Single() as IEnumerable<object?> ?? new object?[] { null };
		var testDisplayName = dataAttribute.GetNamedArgument<string>(nameof(DataAttribute.TestDisplayName));
		var timeout = dataAttribute.GetNamedArgument<int?>(nameof(DataAttribute.Timeout));

		var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
		var traitsArray = dataAttribute.GetNamedArgument<string[]>(nameof(DataAttribute.Traits));
		TestIntrospectionHelper.MergeTraitsInto(traits, traitsArray);

		var theoryDataRow = new TheoryDataRow(args.ToArray())
		{
			Explicit = dataAttribute.GetNamedArgument<bool?>(nameof(DataAttribute.Explicit)),
			Skip = dataAttribute.GetNamedArgument<string>(nameof(DataAttribute.Skip)),
			TestDisplayName = testDisplayName,
			Timeout = timeout,
			Traits = traits,
		};

		return new(new[] { theoryDataRow });
	}

	/// <inheritdoc/>
	public virtual bool SupportsDiscoveryEnumeration(
		_IAttributeInfo dataAttribute,
		_IMethodInfo testMethod)
			=> true;
}
