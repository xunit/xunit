using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base class with common functionality between <see cref="CollectionPerAssemblyTestCollectionFactory"/>
/// and <see cref="CollectionPerClassTestCollectionFactory"/>.
/// </summary>
/// <param name="testAssembly">The test assembly</param>
public abstract class TestCollectionFactoryBase(IXunitTestAssembly testAssembly) : IXunitTestCollectionFactory
{
	readonly ConcurrentDictionary<string, IXunitTestCollection> testCollections = new();

	/// <inheritdoc/>
	public abstract string DisplayName { get; }

	/// <summary>
	/// Gets the test assembly.
	/// </summary>
	protected IXunitTestAssembly TestAssembly { get; } = Guard.ArgumentNotNull(testAssembly);

	IXunitTestCollection CreateCollection(ICollectionAttribute attribute) =>
		TestAssembly.CollectionDefinitions.TryGetValue(attribute.Name, out var definition)
			? new XunitTestCollection(TestAssembly, definition.Type, definition.Attribute.DisableParallelization, attribute.Name)
			: (IXunitTestCollection)new XunitTestCollection(TestAssembly, attribute.Type, disableParallelization: false, attribute.Name);

	/// <inheritdoc/>
	public IXunitTestCollection Get(Type testClass)
	{
		Guard.ArgumentNotNull(testClass);

		var attributes = testClass.GetMatchingCustomAttributes(typeof(ICollectionAttribute));

		return attributes.Count > 1
			? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "More than one collection attribute was found on test class {0}: {1}", testClass.SafeName(), string.Join(", ", attributes.Select(a => a.GetType()).ToCommaSeparatedList())))
			: attributes.FirstOrDefault() is ICollectionAttribute attribute
				? testCollections.GetOrAdd(attribute.Name, _ => CreateCollection(attribute))
				: GetDefaultTestCollection(testClass);
	}

	/// <summary>
	/// Override to provide a test collection when the given test class is not decorated
	/// with any test collection attributes.
	/// </summary>
	/// <param name="testClass">The test class</param>
	protected abstract IXunitTestCollection GetDefaultTestCollection(Type testClass);
}
