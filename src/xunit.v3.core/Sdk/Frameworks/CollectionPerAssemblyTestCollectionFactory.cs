using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// Implementation of <see cref="IXunitTestCollectionFactory"/> that creates a single
	/// default test collection for the assembly, and places any tests classes without
	/// the <see cref="CollectionAttribute"/> into it.
	/// </summary>
	public class CollectionPerAssemblyTestCollectionFactory : IXunitTestCollectionFactory
	{
		readonly Dictionary<string, ITypeInfo> collectionDefinitions;
		readonly TestCollection defaultCollection;
		readonly ITestAssembly testAssembly;
		readonly ConcurrentDictionary<string, ITestCollection> testCollections = new ConcurrentDictionary<string, ITestCollection>();

		/// <summary>
		/// Initializes a new instance of the <see cref="CollectionPerAssemblyTestCollectionFactory" /> class.
		/// </summary>
		/// <param name="testAssembly">The assembly.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="IDiagnosticMessage"/> messages.</param>
		public CollectionPerAssemblyTestCollectionFactory(
			ITestAssembly testAssembly,
			_IMessageSink diagnosticMessageSink)
		{
			Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);

			this.testAssembly = Guard.ArgumentNotNull(nameof(testAssembly), testAssembly);

			defaultCollection = new TestCollection(testAssembly, null, "Test collection for " + Path.GetFileName(testAssembly.Assembly.AssemblyPath));
			collectionDefinitions = TestCollectionFactoryHelper.GetTestCollectionDefinitions(testAssembly.Assembly, diagnosticMessageSink);
		}

		/// <inheritdoc/>
		public string DisplayName => "collection-per-assembly";

		ITestCollection CreateTestCollection(string name)
		{
			collectionDefinitions.TryGetValue(name, out var definitionType);
			return new TestCollection(testAssembly, definitionType, name);
		}

		/// <inheritdoc/>
		public ITestCollection Get(ITypeInfo testClass)
		{
			Guard.ArgumentNotNull(nameof(testClass), testClass);

			var collectionAttribute = testClass.GetCustomAttributes(typeof(CollectionAttribute)).SingleOrDefault();
			if (collectionAttribute == null)
				return defaultCollection;

			var collectionName = collectionAttribute.GetConstructorArguments().Cast<string>().Single();
			return testCollections.GetOrAdd(collectionName, CreateTestCollection);
		}
	}
}
