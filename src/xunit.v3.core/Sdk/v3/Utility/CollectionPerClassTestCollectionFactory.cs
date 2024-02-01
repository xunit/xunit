using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="IXunitTestCollectionFactory"/> which creates a new test
/// collection for each test class that isn't decorated with <see cref="CollectionAttribute"/>
/// or <see cref="CollectionAttribute{TCollectionDefinition}"/>.
/// </summary>
public class CollectionPerClassTestCollectionFactory : IXunitTestCollectionFactory
{
	readonly Lazy<Dictionary<string, _ITypeInfo>> collectionDefinitions;
	readonly _ITestAssembly testAssembly;
	readonly ConcurrentDictionary<string, _ITestCollection> testCollections = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionPerClassTestCollectionFactory" /> class.
	/// </summary>
	/// <param name="testAssembly">The assembly info.</param>
	public CollectionPerClassTestCollectionFactory(_ITestAssembly testAssembly)
	{
		this.testAssembly = Guard.ArgumentNotNull(testAssembly);
		collectionDefinitions = new(InitializeCollections);
	}

	/// <inheritdoc/>
	public string DisplayName => "collection-per-class";

	_ITestCollection CreateCollection(string name)
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
			return testCollections.GetOrAdd("Test collection for " + testClass.Name, CreateCollection);

		var collectionName = collectionAttribute.GetConstructorArguments().Single();
		if (collectionName is string stringCollection)
			return testCollections.GetOrAdd(stringCollection, CreateCollection);

		// From the constructor options, the argument must either be a string or a type
		var typeCollection = (Type)collectionName!;
		return GetForType(new ReflectionTypeInfo(typeCollection));
	}

	_ITestCollection GetForType(_ITypeInfo fixtureType)
	{
		string name = UniqueIDGenerator.ForType(
			fixtureType.Assembly.Name, fixtureType.Namespace, fixtureType.Name);

		if (!collectionDefinitions.Value.ContainsKey(name))
			collectionDefinitions.Value.Add(name, fixtureType);

		return testCollections.GetOrAdd(name, CreateCollection);
	}

	Dictionary<string, _ITypeInfo> InitializeCollections() =>
		TestCollectionFactoryHelper.GetTestCollectionDefinitions(testAssembly.Assembly);
}
