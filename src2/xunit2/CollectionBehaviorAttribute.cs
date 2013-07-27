using System;
using Xunit.Sdk;

namespace Xunit
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class CollectionBehaviorAttribute : AttributeBase
    {
        public CollectionBehaviorAttribute() { }

        public CollectionBehaviorAttribute(CollectionBehavior collectionBehavior) { }

        public CollectionBehaviorAttribute(string factoryTypeName, string factoryAssemblyName) { }

        public bool RunCollectionsInParallel { get; set; }
    }
}