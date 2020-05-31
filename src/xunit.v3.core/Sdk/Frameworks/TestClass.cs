using System;
using System.ComponentModel;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The default implementation of <see cref="ITestClass"/>.
    /// </summary>
    [DebuggerDisplay(@"\{ class = {Class.Name} \}")]
    public class TestClass : LongLivedMarshalByRefObject, ITestClass
    {
        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
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

        /// <inheritdoc/>
        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("TestCollection", TestCollection);
            info.AddValue("ClassAssemblyName", Class.Assembly.Name);
            info.AddValue("ClassTypeName", Class.Name);
        }

        /// <inheritdoc/>
        public void Deserialize(IXunitSerializationInfo info)
        {
            TestCollection = info.GetValue<ITestCollection>("TestCollection");

            var assemblyName = info.GetValue<string>("ClassAssemblyName");
            var typeName = info.GetValue<string>("ClassTypeName");

            Class = Reflector.Wrap(SerializationHelper.GetType(assemblyName, typeName));
        }
    }
}
