using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="IXunitTestCollectionFactory"/> that creates a single
/// default test collection for the assembly, and places any tests classes without
/// the <see cref="CollectionAttribute"/> into it.
/// </summary>
public class CollectionPerAssemblyTestCollectionFactory : IXunitTestCollectionFactory
{
	readonly Lazy<Dictionary<string, _ITypeInfo>> collectionDefinitions;
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
		collectionDefinitions = new(InitializeCollections);
	}

	/// <inheritdoc/>
	public string DisplayName => "collection-per-assembly";

	_ITestCollection CreateTestCollection(string name)
	{
		collectionDefinitions.Value.TryGetValue(name, out var definitionType);
		return new TestCollection(testAssembly, definitionType, name);
	}

	/// <inheritdoc/>
	public _ITestCollection Get(_ITypeInfo testClass)
	{
		Guard.ArgumentNotNull(testClass);

		var genericCollectionAttribute = testClass.GetCustomAttributes(typeof(CollectionAttribute<>)).SingleOrDefault();
		if (genericCollectionAttribute is not null)
			return GetForType(genericCollectionAttribute.AttributeType.GetGenericArguments()[0]);

		var collectionAttribute = testClass.GetCustomAttributes(typeof(CollectionAttribute)).SingleOrDefault();
		if (collectionAttribute is null)
			return defaultCollection;

		var collectionName = collectionAttribute.GetConstructorArguments().Single();
		if (collectionName is string stringCollection)
			return testCollections.GetOrAdd(stringCollection, CreateTestCollection);

		// From the constructor options, the argument must either be a string or a type
		var typeCollection = (Type)collectionName!;
		return GetForType(new ReflectionTypeInfo(typeCollection));
	}

	_ITestCollection GetForType(_ITypeInfo fixtureType)
	{
		string name = UniqueIDGenerator.ForType(fixtureType);

		if (!collectionDefinitions.Value.ContainsKey(name))
			collectionDefinitions.Value.Add(name, fixtureType);

		return testCollections.GetOrAdd(name, CreateTestCollection);
	}

	Dictionary<string, _ITypeInfo> InitializeCollections() =>
		TestCollectionFactoryHelper.GetTestCollectionDefinitions(testAssembly.Assembly);
}
