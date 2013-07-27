using System.Collections.Concurrent;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class CollectionPerClassTestCollectionFactory : IXunitTestCollectionFactory
    {
        readonly ConcurrentDictionary<string, ITestCollection> testCollections = new ConcurrentDictionary<string, ITestCollection>();

        public CollectionPerClassTestCollectionFactory(IAssemblyInfo assembly) { }

        public string DisplayName
        {
            get { return "collection-per-class"; }
        }

        public ITestCollection Get(ITypeInfo testClass)
        {
            string collectionName;
            var collectionAttribute = testClass.GetCustomAttributes(typeof(CollectionAttribute)).SingleOrDefault();

            if (collectionAttribute == null)
                collectionName = "Test collection for " + testClass.Name;
            else
                collectionName = (string)collectionAttribute.GetConstructorArguments().First();

            return testCollections.GetOrAdd(collectionName, name => new XunitTestCollection { DisplayName = name });
        }
    }
}