using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class CollectionPerAssemblyTestCollectionFactory : IXunitTestCollectionFactory
    {
        readonly XunitTestCollection defaultCollection;
        readonly ConcurrentDictionary<string, ITestCollection> testCollections = new ConcurrentDictionary<string, ITestCollection>();

        public CollectionPerAssemblyTestCollectionFactory(IAssemblyInfo assemblyInfo)
        {
            defaultCollection = new XunitTestCollection { DisplayName = "Test collection for " + Path.GetFileName(assemblyInfo.AssemblyPath) };
        }

        public string DisplayName
        {
            get { return "collection-per-assembly"; }
        }

        public ITestCollection Get(ITypeInfo testClass)
        {
            var collectionAttribute = testClass.GetCustomAttributes(typeof(CollectionAttribute)).SingleOrDefault();
            if (collectionAttribute == null)
                return defaultCollection;

            var collectionName = (string)collectionAttribute.GetConstructorArguments().First();
            return testCollections.GetOrAdd(collectionName, name => new XunitTestCollection { DisplayName = name });
        }
    }
}