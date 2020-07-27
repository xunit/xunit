using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
	/// <summary>
	/// Implementation of <see cref="IXunitTestCollectionFactory"/> which creates a new test
	/// collection for each test class that isn't decorated with <see cref="CollectionAttribute"/>.
	/// </summary>
	public class CollectionPerClassTestCollectionFactory : IXunitTestCollectionFactory
	{
		readonly Dictionary<string, ITypeInfo> collectionDefinitions;
		readonly ITestAssembly testAssembly;
		readonly ConcurrentDictionary<string, ITestCollection> testCollections = new ConcurrentDictionary<string, ITestCollection>();

		/// <summary>
		/// Initializes a new instance of the <see cref="CollectionPerClassTestCollectionFactory" /> class.
		/// </summary>
		/// <param name="testAssembly">The assembly info.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="IDiagnosticMessage"/> messages.</param>
		public CollectionPerClassTestCollectionFactory(ITestAssembly testAssembly, IMessageSink diagnosticMessageSink)
		{
			Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);

			this.testAssembly = Guard.ArgumentNotNull(nameof(testAssembly), testAssembly);

			collectionDefinitions = TestCollectionFactoryHelper.GetTestCollectionDefinitions(testAssembly.Assembly, diagnosticMessageSink);
		}

		/// <inheritdoc/>
		public string DisplayName => "collection-per-class";

		ITestCollection CreateCollection(string name)
		{
			collectionDefinitions.TryGetValue(name, out var definitionType);
			return new TestCollection(testAssembly, definitionType, name);
		}

		/// <inheritdoc/>
		public ITestCollection Get(ITypeInfo testClass)
		{
			Guard.ArgumentNotNull(nameof(testClass), testClass);

			string collectionName;
			var collectionAttribute = testClass.GetCustomAttributes(typeof(CollectionAttribute)).SingleOrDefault();

			if (collectionAttribute == null)
				collectionName = "Test collection for " + testClass.Name;
			else
				collectionName = collectionAttribute.GetConstructorArguments().Cast<string>().Single();

			return testCollections.GetOrAdd(collectionName, CreateCollection);
		}
	}
}
