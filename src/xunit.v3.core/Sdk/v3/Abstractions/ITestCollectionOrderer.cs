using System.Collections.Generic;

namespace Xunit.v3
{
	/// <summary>
	/// A class implements this interface to participate in ordering tests
	/// for the test runner. Test collection orderers are applied using the
	/// <see cref="TestCollectionOrdererAttribute"/>, which can be applied at
	/// the assembly level.
	/// </summary>
	public interface ITestCollectionOrderer
	{
		/// <summary>
		/// Orders test collections for execution.
		/// </summary>
		/// <param name="testCollections">The test collections to be ordered.</param>
		/// <returns>The test collections in the order to be run.</returns>
		IReadOnlyCollection<_ITestCollection> OrderTestCollections(IReadOnlyCollection<_ITestCollection> testCollections);
	}
}
