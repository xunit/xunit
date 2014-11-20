using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IDataDiscoverer"/>. Uses reflection to find the
    /// data associated with <see cref="DataAttribute"/>; may return <c>null</c> when called
    /// without reflection-based abstraction implementations.
    /// </summary>
    public class DataDiscoverer : IDataDiscoverer
    {
        /// <inheritdoc/>
        public virtual IEnumerable<object[]> GetData(IAttributeInfo dataAttribute, IMethodInfo testMethod)
        {
            var reflectionDataAttribute = dataAttribute as IReflectionAttributeInfo;
            var reflectionTestMethod = testMethod as IReflectionMethodInfo;

            if (reflectionDataAttribute != null && reflectionTestMethod != null)
                return ((DataAttribute)reflectionDataAttribute.Attribute).GetData(reflectionTestMethod.MethodInfo);

            return null;
        }

        /// <inheritdoc/>
        public virtual bool SupportsDiscoveryEnumeration(IAttributeInfo dataAttribute, IMethodInfo testMethod)
        {
            return true;
        }
    }
}