using System.Collections.Generic;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A class implements this interface to participate in ordering tests for the test runner.
/// Test collection orderers are applied using the and implementation of
/// <see cref="ITestCollectionOrdererAttribute"/> (most commonly <see cref="TestCollectionOrdererAttribute"/>),
/// which can be applied at the assembly level.
/// </summary>
public interface ITestCollectionOrderer
{
	/// <summary>
	/// Orders test collections for execution.
	/// </summary>
	/// <typeparam name="TTestCollection">The type of the test collection to be ordered. Must derive
	/// from <see cref="ITestCollection"/>.</typeparam>
	/// <param name="testCollections">The test collections to be ordered.</param>
	/// <returns>The test collections in the order to be run.</returns>
	IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections)
		where TTestCollection : ITestCollection;
}
