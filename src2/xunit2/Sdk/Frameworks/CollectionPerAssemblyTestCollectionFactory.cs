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
        readonly XunitTestCollection defaultCollection;
        readonly ConcurrentDictionary<string, ITestCollection> testCollections = new ConcurrentDictionary<string, ITestCollection>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionPerAssemblyTestCollectionFactory" /> class.
        /// </summary>
        /// <param name="assemblyInfo">The assembly.</param>
        public CollectionPerAssemblyTestCollectionFactory(IAssemblyInfo assemblyInfo)
            : this(assemblyInfo, MessageAggregator.Instance) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionPerAssemblyTestCollectionFactory" /> class.
        /// </summary>
        /// <param name="assemblyInfo">The assembly.</param>
        /// <param name="messageAggregator">The message aggregator used to report <see cref="EnvironmentalWarning"/> messages.</param>
        public CollectionPerAssemblyTestCollectionFactory(IAssemblyInfo assemblyInfo, IMessageAggregator messageAggregator)
        {
            defaultCollection = new XunitTestCollection { DisplayName = "Test collection for " + Path.GetFileName(assemblyInfo.AssemblyPath) };
            collectionDefinitions = TestCollectionFactoryHelper.GetTestCollectionDefinitions(assemblyInfo, messageAggregator);
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
            return new XunitTestCollection { CollectionDefinition = definitionType, DisplayName = name };
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