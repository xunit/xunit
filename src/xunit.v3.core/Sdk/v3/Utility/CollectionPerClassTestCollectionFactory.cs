using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="IXunitTestCollectionFactory"/> which creates a new test
/// collection for each test class that isn't decorated with <see cref="CollectionAttribute"/>.
/// </summary>
public class CollectionPerClassTestCollectionFactory : IXunitTestCollectionFactory
{
	Dictionary<string, _ITypeInfo>? collectionDefinitions;
	readonly _ITestAssembly testAssembly;
	readonly ConcurrentDictionary<string, _ITestCollection> testCollections = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionPerClassTestCollectionFactory" /> class.
	/// </summary>
	/// <param name="testAssembly">The assembly info.</param>
	public CollectionPerClassTestCollectionFactory(_ITestAssembly testAssembly)
	{
		this.testAssembly = Guard.ArgumentNotNull(testAssembly);
	}

	/// <inheritdoc/>
	public string DisplayName => "collection-per-class";

	_ITestCollection CreateCollection(string name)
	{
		if (collectionDefinitions is null)
			collectionDefinitions = TestCollectionFactoryHelper.GetTestCollectionDefinitions(testAssembly.Assembly);

		collectionDefinitions.TryGetValue(name, out var definitionType);
		return new TestCollection(testAssembly, definitionType, name);
	}

	/// <inheritdoc/>
	public _ITestCollection Get(_ITypeInfo testClass)
	{
		Guard.ArgumentNotNull(testClass);

		string collectionName;
		var collectionAttribute = testClass.GetCustomAttributes(typeof(CollectionAttribute)).SingleOrDefault();

		if (collectionAttribute is null)
			collectionName = "Test collection for " + testClass.Name;
		else
			collectionName = collectionAttribute.GetConstructorArguments().Cast<string>().Single();

		return testCollections.GetOrAdd(collectionName, CreateCollection);
	}
}
