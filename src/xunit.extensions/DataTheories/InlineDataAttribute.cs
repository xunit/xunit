using System;
using System.Collections.Generic;
using System.Reflection;

namespace Xunit.Extensions
{
    /// <summary>
    /// Provides a data source for a data theory, with the data coming from inline values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class InlineDataAttribute : DataAttribute
    {
        readonly object[] dataValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineDataAttribute"/> class.
        /// </summary>
        /// <param name="dataValues">The data values to pass to the theory</param>
        public InlineDataAttribute(params object[] dataValues)
        {
            this.dataValues = dataValues ?? new object[] { null };
        }

        /// <summary>
        /// Gets the data values.
        /// </summary>
        public object[] DataValues
        {
            get { return dataValues; }
        }

        /// <summary>
        /// Returns the data to be used to test the theory.
        /// </summary>
        /// <param name="methodUnderTest">The method that is being tested</param>
        /// <param name="parameterTypes">The types of the parameters for the test method</param>
        /// <returns>The theory data, in table form</returns>
        public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
        {
            yield return dataValues;
        }
    }
}