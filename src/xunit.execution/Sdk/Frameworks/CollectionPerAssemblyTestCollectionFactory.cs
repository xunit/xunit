using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

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
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        public CollectionPerAssemblyTestCollectionFactory(ITestAssembly testAssembly, IMessageSink diagnosticMessageSink)
        {
            this.testAssembly = testAssembly;

            defaultCollection = new TestCollection(testAssembly, null, "Test collection for " + Path.GetFileName(testAssembly.Assembly.AssemblyPath));
            collectionDefinitions = TestCollectionFactoryHelper.GetTestCollectionDefinitions(testAssembly.Assembly, diagnosticMessageSink);
        }

        /// <inheritdoc/>
        public string DisplayName
        {
            get { return "collection-per-assembly"; }
        }

        ITestCollection CreateTestCollection(string name)
        {
            ITypeInfo definitionType;
            collectionDefinitions.TryGetValue(name, out definitionType);
            return new TestCollection(testAssembly, definitionType, name);
        }

        /// <inheritdoc/>
        public ITestCollection Get(ITypeInfo testClass)
        {
            var collectionAttribute = testClass.GetCustomAttributes(typeof(CollectionAttribute)).SingleOrDefault();
            if (collectionAttribute == null)
                return defaultCollection;

            var collectionName = (string)collectionAttribute.GetConstructorArguments().First();
            return testCollections.GetOrAdd(collectionName, CreateTestCollection);
        }
    }
}
