using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="ITestMethod"/> that is used by xUnit.net v2.
    /// </summary>
    [Serializable]
    [DebuggerDisplay(@"\{ class = {TestClass.Class.Name}, method = {Method.Name} \}")]
    public class XunitTestMethod : LongLivedMarshalByRefObject, ITestMethod, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestMethod"/> class.
        /// </summary>
        public XunitTestMethod(ITestClass testClass, IMethodInfo method)
        {
            Method = method;
            TestClass = testClass;
        }

        /// <inheritdoc/>
        protected XunitTestMethod(SerializationInfo info, StreamingContext context)
        {
            TestClass = info.GetValue<ITestClass>("TestClass");

            var methodName = info.GetString("MethodName");

            Method = TestClass.Class.GetMethod(methodName, includePrivateMethod: false);
        }

        /// <inheritdoc/>
        public IMethodInfo Method { get; set; }

        /// <inheritdoc/>
        public ITestClass TestClass { get; set; }

        /// <inheritdoc/>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("MethodName", Method.Name);
            info.AddValue("TestClass", TestClass);
        }
    }
}