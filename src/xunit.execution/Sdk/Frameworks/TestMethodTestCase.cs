using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using Xunit.Abstractions;
using Xunit.Serialization;

#if !WINDOWS_PHONE_APP
using System.Security.Cryptography;
#else
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
#endif

namespace Xunit.Sdk
{
    /// <summary>
    /// A base class implementation of <see cref="ITestCase"/> which is based on test cases being
    /// related directly to test methods.
    /// </summary>
    [Serializable]
    public abstract class TestMethodTestCase : LongLivedMarshalByRefObject, ITestCase, ISerializable, IGetTypeData, IDisposable
    {
        private Dictionary<string, List<string>> traits;
        private string skipReason;
        string displayName;
        bool initialized;
        IMethodInfo method;
        ITypeInfo[] methodGenericTypes;
        Lazy<string> uniqueID;

#if !WINDOWS_PHONE_APP
        readonly static HashAlgorithm Hasher = new SHA1Managed();
#else
        readonly static HashAlgorithmProvider Hasher = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
#endif

        /// <summary>
        /// Used for de-serialization.
        /// </summary>
        protected TestMethodTestCase() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethodTestCase"/> class.
        /// </summary>
        /// <param name="testMethod">The test method this test case belongs to.</param>
        /// <param name="testMethodArguments">The arguments for the test method.</param>
        protected TestMethodTestCase(ITestMethod testMethod, object[] testMethodArguments = null)
        {
            TestMethod = testMethod;
            TestMethodArguments = testMethodArguments;
        }

        /// <summary>
        /// Returns the base display name for a test ("TestClassName.MethodName").
        /// </summary>
        protected string BaseDisplayName
        {
            get { return String.Format("{0}.{1}", TestMethod.TestClass.Class.Name, TestMethod.Method.Name); }
        }

        /// <inheritdoc/>
        public string DisplayName
        {
            get
            {
                EnsureInitialized();
                return displayName;
            }
            protected set
            {
                EnsureInitialized();
                displayName = value;
            }
        }

        /// <inheritdoc/>
        public IMethodInfo Method
        {
            get
            {
                EnsureInitialized();
                return method;
            }
            protected set
            {
                EnsureInitialized();
                method = value;
            }
        }

        /// <summary>
        /// Gets the generic types that were used to close the generic test method, if
        /// applicable; <c>null</c>, if the test method was not an open generic.
        /// </summary>
        protected ITypeInfo[] MethodGenericTypes
        {
            get
            {
                EnsureInitialized();
                return methodGenericTypes;
            }
        }

        /// <inheritdoc/>
        public string SkipReason
        {
            get
            {
                EnsureInitialized();
                return skipReason;
            }
            protected set
            {
                EnsureInitialized();
                skipReason = value;
            }
        }

        /// <inheritdoc/>
        public ISourceInformation SourceInformation { get; set; }

        /// <inheritdoc/>
        public ITestMethod TestMethod { get; protected set; }

        /// <inheritdoc/>
        public object[] TestMethodArguments { get; protected set; }

        /// <inheritdoc/>
        public Dictionary<string, List<string>> Traits
        {
            get
            {
                EnsureInitialized();
                return traits;
            }
            protected set
            {
                EnsureInitialized();
                traits = value;
            }
        }

        /// <inheritdoc/>
        public string UniqueID
        {
            get
            {
                EnsureInitialized();
                return uniqueID.Value;
            }
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            if (TestMethodArguments != null)
                foreach (var disposable in TestMethodArguments.OfType<IDisposable>())
                    disposable.Dispose();
        }

        /// <summary>
        /// Call to ensure the object is fully initialized().
        /// </summary>
        protected void EnsureInitialized()
        {
            if (!initialized)
            {
                initialized = true;
                Initialize();
            }
        }

        /// <summary>
        /// Gets the unique ID for the test case.
        /// </summary>
        protected virtual string GetUniqueID()
        {
            using (var stream = new MemoryStream())
            {
                Write(stream, TestMethod.TestClass.TestCollection.TestAssembly.Assembly.Name);
                Write(stream, TestMethod.TestClass.Class.Name);
                Write(stream, TestMethod.Method.Name);

                if (TestMethodArguments != null)
                    Write(stream, SerializationHelper.Serialize(TestMethodArguments));

                stream.Position = 0;

#if !WINDOWS_PHONE_APP
                var hash = Hasher.ComputeHash(stream);
#else
                var buffer = CryptographicBuffer.CreateFromByteArray(stream.ToArray());
                var hash = Hasher.HashData(buffer).ToArray();
#endif

                return String.Join("", hash.Select(x => x.ToString("x2")).ToArray());
            }
        }

        /// <summary>
        /// Called when initializing the test cases, either after constructor or de-serialization.
        /// Override this method to add additional initialization-time work.
        /// </summary>
        protected virtual void Initialize()
        {
            Traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            Method = TestMethod.Method;

            if (TestMethodArguments != null && method.IsGenericMethodDefinition)
            {
                methodGenericTypes = TypeUtility.ResolveGenericTypes(Method, TestMethodArguments);
                Method = Method.MakeGenericMethod(MethodGenericTypes);
            }

            var baseDisplayName = BaseDisplayName;
            displayName = TypeUtility.GetDisplayNameWithArguments(Method, baseDisplayName, TestMethodArguments, MethodGenericTypes);

            uniqueID = new Lazy<string>(GetUniqueID, true);
        }

        static void Write(Stream stream, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
            stream.WriteByte(0);
        }

        // -------------------- Serialization --------------------

        /// <inheritdoc/>
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("TestMethod", TestMethod);
            info.AddValue("TestMethodArguments", TestMethodArguments);
        }

        /// <inheritdoc/>
        public virtual void GetData(XunitSerializationInfo data)
        {
            // TODO: Should throw when TestMethodArguments is not null/empty?
            data.AddValue("TestMethod", TestMethod);
            data.AddValue("TestMethodArguments", TestMethodArguments);
        }

        /// <inheritdoc/>
        protected TestMethodTestCase(SerializationInfo info, StreamingContext context)
        {
            TestMethod = info.GetValue<ITestMethod>("TestMethod");
            TestMethodArguments = info.GetValue<object[]>("TestMethodArguments");
        }

        /// <inheritdoc/>
        public void SetData(XunitSerializationInfo data)
        {
            TestMethod = data.GetValue<ITestMethod>("TestMethod");
            TestMethodArguments = null;
        }
    }
}
