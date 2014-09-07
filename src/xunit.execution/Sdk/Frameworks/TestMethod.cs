using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using Xunit.Abstractions;
using Xunit.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// The default implementation of <see cref="ITestMethod"/>.
    /// </summary>
    [Serializable]
    [DebuggerDisplay(@"\{ class = {TestClass.Class.Name}, method = {Method.Name} \}")]
    public class TestMethod : LongLivedMarshalByRefObject, ITestMethod, ISerializable, IGetTypeData
    {
        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer", error: true)]
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

        // -------------------- Serialization --------------------

        /// <inheritdoc/>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("MethodName", Method.Name);
            info.AddValue("TestClass", TestClass);
        }

        /// <inheritdoc/>
        public void GetData(XunitSerializationInfo info)
        {
            info.AddValue("MethodName", Method.Name);
            info.AddValue("TestClass", TestClass);
        }

        /// <inheritdoc/>
        protected TestMethod(SerializationInfo info, StreamingContext context)
        {
            TestClass = info.GetValue<ITestClass>("TestClass");

            var methodName = info.GetString("MethodName");

            Method = TestClass.Class.GetMethod(methodName, includePrivateMethod: false);
        }

        /// <inheritdoc/>
        public void SetData(XunitSerializationInfo info)
        {
            TestClass = info.GetValue<ITestClass>("TestClass");

            var methodName = info.GetString("MethodName");

            Method = TestClass.Class.GetMethod(methodName, includePrivateMethod: false);
        }
    }
}