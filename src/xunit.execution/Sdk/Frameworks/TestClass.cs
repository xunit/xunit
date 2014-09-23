﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using Xunit.Abstractions;
using Xunit.Serialization;
#if !ASPNETCORE50
using System.Runtime.Serialization;
#endif

namespace Xunit.Sdk
{
    /// <summary>
    /// The default implementation of <see cref="ITestClass"/>.
    /// </summary>
    [Serializable]
    [DebuggerDisplay(@"\{ class = {Class.Name} \}")]
    public class TestClass : LongLivedMarshalByRefObject, ITestClass, ISerializable, IGetTypeData
    {
        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer", error: true)]
        public TestClass() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClass"/> class.
        /// </summary>
        /// <param name="testCollection">The test collection the class belongs to</param>
        /// <param name="class">The test class</param>
        public TestClass(ITestCollection testCollection, ITypeInfo @class)
        {
            Guard.ArgumentNotNull("testCollection", testCollection);
            Guard.ArgumentNotNull("class", @class);

            Class = @class;
            TestCollection = testCollection;
        }

        /// <inheritdoc/>
        public ITypeInfo Class { get; set; }

        /// <inheritdoc/>
        public ITestCollection TestCollection { get; set; }

        // -------------------- Serialization --------------------

        /// <inheritdoc/>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("TestCollection", TestCollection);
            info.AddValue("ClassAssemblyName", Class.Assembly.Name);
            info.AddValue("ClassTypeName", Class.Name);
        }

        /// <inheritdoc/>
        public void GetData(XunitSerializationInfo info)
        {
            info.AddValue("TestCollection", TestCollection);
            info.AddValue("ClassAssemblyName", Class.Assembly.Name);
            info.AddValue("ClassTypeName", Class.Name);
        }

        /// <inheritdoc/>
        protected TestClass(SerializationInfo info, StreamingContext context)
        {
            TestCollection = info.GetValue<ITestCollection>("TestCollection");

            var assemblyName = info.GetString("ClassAssemblyName");
            var typeName = info.GetString("ClassTypeName");

            Class = Reflector.Wrap(Reflector.GetType(assemblyName, typeName));
        }

        /// <inheritdoc/>
        public void SetData(XunitSerializationInfo info)
        {
            TestCollection = info.GetValue<ITestCollection>("TestCollection");

            var assemblyName = info.GetString("ClassAssemblyName");
            var typeName = info.GetString("ClassTypeName");

            Class = Reflector.Wrap(Reflector.GetType(assemblyName, typeName));
        }
    }
}