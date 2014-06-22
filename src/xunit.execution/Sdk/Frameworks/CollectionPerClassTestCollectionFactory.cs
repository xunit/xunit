﻿using System.Collections.Concurrent;
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
        /// <param name="testAssembly">The assembly.</param>
        public CollectionPerClassTestCollectionFactory(ITestAssembly testAssembly)
            : this(testAssembly, MessageAggregator.Instance) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionPerClassTestCollectionFactory" /> class.
        /// </summary>
        /// <param name="testAssembly">The assembly info.</param>
        /// <param name="messageAggregator">The message aggregator used to report <see cref="EnvironmentalWarning"/> messages.</param>
        public CollectionPerClassTestCollectionFactory(ITestAssembly testAssembly, IMessageAggregator messageAggregator)
        {
            this.testAssembly = testAssembly;

            collectionDefinitions = TestCollectionFactoryHelper.GetTestCollectionDefinitions(testAssembly.Assembly, messageAggregator);
        }

        /// <inheritdoc/>
        public string DisplayName
        {
            get { return "collection-per-class"; }
        }

        ITestCollection CreateCollection(string name)
        {
            ITypeInfo definitionType;
            collectionDefinitions.TryGetValue(name, out definitionType);
            return new XunitTestCollection(testAssembly, definitionType, name);
        }

        /// <inheritdoc/>
        public ITestCollection Get(ITypeInfo testClass)
        {
            string collectionName;
            var collectionAttribute = testClass.GetCustomAttributes(typeof(CollectionAttribute)).SingleOrDefault();

            if (collectionAttribute == null)
                collectionName = "Test collection for " + testClass.Name;
            else
                collectionName = (string)collectionAttribute.GetConstructorArguments().First();

            return testCollections.GetOrAdd(collectionName, CreateCollection);
        }
    }
}