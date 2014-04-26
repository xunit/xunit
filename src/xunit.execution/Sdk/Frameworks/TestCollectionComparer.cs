using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestCollectionComparer : IEqualityComparer<ITestCollection>
    {
        public static readonly TestCollectionComparer Instance = new TestCollectionComparer();

        public bool Equals(ITestCollection x, ITestCollection y)
        {
            return x.ID == y.ID;
        }

        public int GetHashCode(ITestCollection obj)
        {
            return obj.ID.GetHashCode();
        }
    }
}
