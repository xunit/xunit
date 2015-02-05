using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    /// </summary>
    [CLSCompliant(false)]
    [DataDiscoverer("Xunit.Sdk.MemberDataDiscoverer", "xunit.core")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class MemberDataAttribute : MemberDataAttributeBase
    {
        /// <inheritdoc/>
        public MemberDataAttribute(string memberName, params object[] parameters)
            : base(memberName, parameters)
        { }

        /// <inheritdoc/>
        protected override object[] ConvertDataItem(MethodInfo testMethod, object item)
        {
            if (item == null)
                return null;

            var array = item as object[];
            if (array == null)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Property {0} on {1} yielded an item that is not an object[]", MemberName, MemberType ?? testMethod.DeclaringType));

            return array;
        }
    }
}