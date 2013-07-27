using System;
using Xunit.Sdk;

namespace Xunit
{
    // REVIEW: Does this need the discoverer/interface pattern, or is this attribute fine as-is?
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class CollectionAttribute : AttributeBase
    {
        public CollectionAttribute(string name) { }
    }
}
