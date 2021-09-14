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
            {
                var attribute = (DataAttribute)reflectionDataAttribute.Attribute;
                try
                {
                    return attribute.GetData(reflectionTestMethod.MethodInfo);
                }
                catch (ArgumentException)
                {
                    // If we couldn't find the data on the base type, check if it is in current type.
                    // This allows base classes to specify data that exists on a sub type, but not on the base type.
                    var memberDataAttribute = attribute as MemberDataAttribute;
                    var reflectionTestMethodType = reflectionTestMethod.Type as IReflectionTypeInfo;
                    if (memberDataAttribute != null && memberDataAttribute.MemberType == null)
                    {
                        memberDataAttribute.MemberType = reflectionTestMethodType.Type;
                    }
                    return attribute.GetData(reflectionTestMethod.MethodInfo);
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public virtual bool SupportsDiscoveryEnumeration(IAttributeInfo dataAttribute, IMethodInfo testMethod)
        {
            return true;
        }
    }
}
