using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="ITestClass"/> that is used by xUnit.net v2.
    /// </summary>
    [Serializable]
    [DebuggerDisplay(@"\{ class = {Class.Name} \}")]
    public class XunitTestClass : LongLivedMarshalByRefObject, ITestClass, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestClass"/> class.
        /// </summary>
        public XunitTestClass(ITestCollection testCollection, ITypeInfo @class)
        {
            Class = @class;
            TestCollection = testCollection;
        }

        /// <inheritdoc/>
        protected XunitTestClass(SerializationInfo info, StreamingContext context)
        {
            TestCollection = info.GetValue<ITestCollection>("TestCollection");

            var assemblyName = info.GetString("ClassAssemblyName");
            var typeName = info.GetString("ClassTypeName");

            Class = Reflector.Wrap(Reflector.GetType(assemblyName, typeName));
        }

        /// <inheritdoc/>
        public ITypeInfo Class { get; set; }

        /// <inheritdoc/>
        public ITestCollection TestCollection { get; set; }

        /// <inheritdoc/>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("TestCollection", TestCollection);
            info.AddValue("ClassAssemblyName", Class.Assembly.Name);
            info.AddValue("ClassTypeName", Class.Name);
        }
    }
}