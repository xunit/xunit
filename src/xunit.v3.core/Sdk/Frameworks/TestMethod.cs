using System;
using System.ComponentModel;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The default implementation of <see cref="ITestMethod"/>.
    /// </summary>
    [DebuggerDisplay(@"\{ class = {TestClass.Class.Name}, method = {Method.Name} \}")]
    public class TestMethod : LongLivedMarshalByRefObject, ITestMethod
    {
        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public TestMethod() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethod"/> class.
        /// </summary>
        /// <param name="class">The test class</param>
        /// <param name="method">The test method</param>
        public TestMethod(ITestClass @class, IMethodInfo method)
        {
            Guard.ArgumentNotNull("class", @class);
            Guard.ArgumentNotNull("method", method);

            Method = method;
            TestClass = @class;
        }

        /// <inheritdoc/>
        public IMethodInfo Method { get; set; }

        /// <inheritdoc/>
        public ITestClass TestClass { get; set; }

        /// <inheritdoc/>
        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("MethodName", Method.Name);
            info.AddValue("TestClass", TestClass);
        }

        /// <inheritdoc/>
        public void Deserialize(IXunitSerializationInfo info)
        {
            TestClass = info.GetValue<ITestClass>("TestClass");

            var methodName = info.GetValue<string>("MethodName");

            Method = TestClass.Class.GetMethod(methodName, true);
        }
    }
}
