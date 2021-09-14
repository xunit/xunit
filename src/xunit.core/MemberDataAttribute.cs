using System;
using System.Globalization;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Provides a data source for a data theory, with the data coming from one of the following sources:
    /// 1. A static property
    /// 2. A static field
    /// 3. A static method (with parameters)
    /// The member must return something compatible with IEnumerable&lt;object[]&gt; with the test data.
    /// Caution: the property is completely enumerated by .ToList() before any test is run. Hence it should return independent object sets.
    /// </summary>
    [CLSCompliant(false)]
    [DataDiscoverer("Xunit.Sdk.MemberDataDiscoverer", "xunit.core")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class MemberDataAttribute : MemberDataAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberDataAttribute"/> class.
        /// </summary>
        /// <param name="memberName">The name of the public static member on the test class that will provide the test data</param>
        /// <param name="parameters">The parameters for the member (only supported for methods; ignored for everything else)</param>
        public MemberDataAttribute(string memberName, params object[] parameters)
            : base(memberName, parameters) { }

        /// <inheritdoc/>
        protected override object[] ConvertDataItem(MethodInfo testMethod, object item)
        {
            if (item == null)
                return null;

            var array = item as object[];
            if (array == null)
                throw new ArgumentException($"Property {MemberName} on {MemberType ?? testMethod.DeclaringType} yielded an item that is not an object[]");

            return array;
        }
    }
}
