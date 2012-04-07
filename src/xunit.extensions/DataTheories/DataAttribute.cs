using System;
using System.Collections.Generic;
using System.Reflection;

namespace Xunit.Extensions
{
    /// <summary>
    /// Abstract attribute which represents a data source for a data theory.
    /// Data source providers derive from this attribute and implement GetData
    /// to return the data for the theory.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class DataAttribute : Attribute
    {
        object typeId = new object();

        /// <summary>
        /// Returns the data to be used to test the theory.
        /// </summary>
        /// <remarks>
        /// The <paramref name="parameterTypes"/> parameter is provided so that the
        /// test data can be converted to the destination parameter type when necessary.
        /// Generally, data should NOT be automatically converted, UNLESS the source data
        /// format does not have rich types (for example, all numbers in Excel spreadsheets
        /// are returned as <see cref="Double"/> even if they are integers). Derivers of
        /// this class should NOT throw exceptions for mismatched types or mismatched number
        /// of parameters; the test framework will throw these exceptions at the correct
        /// time.
        /// </remarks>
        /// <param name="methodUnderTest">The method that is being tested</param>
        /// <param name="parameterTypes">The types of the parameters for the test method</param>
        /// <returns>The theory data</returns>
        public abstract IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes);

        /// <inheritdoc/>
        public override object TypeId
        {
            get { return typeId; }
        }
    }
}