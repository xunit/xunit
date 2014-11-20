using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
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
        IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections);
    }
}
