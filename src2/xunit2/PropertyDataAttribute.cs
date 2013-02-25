//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Reflection;
//using Xunit.Sdk;

//namespace Xunit
//{
//    /// <summary>
//    /// Provides a data source for a data theory, with the data coming from a public static property on the test class.
//    /// The property must return IEnumerable&lt;object[]&gt; with the test data.
//    /// </summary>
//    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
//    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed as an extensibility point.")]
//    public class PropertyDataAttribute : DataAttribute
//    {
//        /// <summary>
//        /// Creates a new instance of <see cref="PropertyDataAttribute"/>/
//        /// </summary>
//        /// <param name="propertyName">The name of the public static property on the test class that will provide the test data</param>
//        public PropertyDataAttribute(string propertyName)
//        {
//            PropertyName = propertyName;
//        }

//        /// <summary>
//        /// Gets the property name.
//        /// </summary>
//        public string PropertyName { get; private set; }

//        // Discovery: if reflection data, then return via reflection
//        // GetData: return via reflection

//        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}