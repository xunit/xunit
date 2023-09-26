using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="IXunitTestCollectionFactory"/> that creates a single
/// default test collection for the assembly, and places any tests classes without
/// the <see cref="CollectionAttribute"/> into it.
/// </summary>
public class CollectionPerAssemblyTestCollectionFactory : IXunitTestCollectionFactory
{
	readonly Dictionary<string, _ITypeInfo> collectionDefinitions;
	readonly TestCollection defaultCollection;
	readonly _ITestAssembly testAssembly;
	readonly ConcurrentDictionary<string, _ITestCollection> testCollections = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionPerAssemblyTestCollectionFactory" /> class.
	/// </summary>
	/// <param name="testAssembly">The assembly.</param>
	public CollectionPerAssemblyTestCollectionFactory(_ITestAssembly testAssembly)
	{
		this.testAssembly = Guard.ArgumentNotNull(testAssembly);

		defaultCollection = new TestCollection(testAssembly, null, "Test collection for " + Path.GetFileName(testAssembly.Assembly.AssemblyPath));
		collectionDefinitions = TestCollectionFactoryHelper.GetTestCollectionDefinitions(testAssembly.Assembly);
	}

	/// <inheritdoc/>
	public string DisplayName => "collection-per-assembly";

	_ITestCollection CreateTestCollection(string name)
	{
		collectionDefinitions.TryGetValue(name, out var definitionType);
		return new TestCollection(testAssembly, definitionType, name);
	}

	/// <inheritdoc/>
	public _ITestCollection Get(_ITypeInfo testClass)
	{
		Guard.ArgumentNotNull(testClass);

		var collectionAttribute = testClass.GetCustomAttributes(typeof(CollectionAttribute)).SingleOrDefault();
		if (collectionAttribute is null)
			return defaultCollection;

		var collectionName = collectionAttribute.GetConstructorArguments().Cast<string>().Single();
		return testCollections.GetOrAdd(collectionName, CreateTestCollection);
	}
}
