using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// A base class implementation of <see cref="ITestCase"/> which is based on test cases being
    /// related directly to test methods.
    /// </summary>
    public abstract class TestMethodTestCase : LongLivedMarshalByRefObject, ITestCase, IDisposable
    {
        string displayName;
        DisplayNameFormatter formatter;
        bool initialized;
        IMethodInfo method;
        ITypeInfo[] methodGenericTypes;
        string skipReason;
        Dictionary<string, List<string>> traits;
        volatile string uniqueID;

        /// <summary>
        /// Used for de-serialization.
        /// </summary>
        protected TestMethodTestCase()
        {
            formatter = new DisplayNameFormatter();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethodTestCase"/> class.
        /// </summary>
        /// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
        /// <param name="testMethod">The test method this test case belongs to.</param>
        /// <param name="testMethodArguments">The arguments for the test method.</param>
        [Obsolete("Please call the constructor which takes TestMethodDisplayOptions")]
        protected TestMethodTestCase(TestMethodDisplay defaultMethodDisplay,
                                     ITestMethod testMethod,
                                     object[] testMethodArguments = null)
            : this(defaultMethodDisplay, TestMethodDisplayOptions.None, testMethod, testMethodArguments) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethodTestCase"/> class.
        /// </summary>
        /// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
        /// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
        /// <param name="testMethod">The test method this test case belongs to.</param>
        /// <param name="testMethodArguments">The arguments for the test method.</param>
        protected TestMethodTestCase(TestMethodDisplay defaultMethodDisplay,
                                     TestMethodDisplayOptions defaultMethodDisplayOptions,
                                     ITestMethod testMethod,
                                     object[] testMethodArguments = null)
        {
            DefaultMethodDisplay = defaultMethodDisplay;
            DefaultMethodDisplayOptions = defaultMethodDisplayOptions;
            formatter = new DisplayNameFormatter(defaultMethodDisplay, defaultMethodDisplayOptions);
            TestMethod = testMethod;
            TestMethodArguments = testMethodArguments;
        }

        /// <summary>
        /// Returns the base display name for a test ("TestClassName.MethodName").
        /// </summary>
        protected string BaseDisplayName
        {
            get
            {
                if (DefaultMethodDisplay == TestMethodDisplay.ClassAndMethod)
                    return formatter.Format($"{TestMethod.TestClass.Class.Name}.{TestMethod.Method.Name}");

                return formatter.Format(TestMethod.Method.Name);
            }
        }

        /// <summary>
        /// Returns the default method display to use (when not customized).
        /// </summary>
        protected internal TestMethodDisplay DefaultMethodDisplay { get; private set; }

        /// <summary>
        /// Returns the default method display options to use (when not customized).
        /// </summary>
        protected internal TestMethodDisplayOptions DefaultMethodDisplayOptions { get; private set; }

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

        /// <summary>
        /// Gets or sets the exception that happened during initialization. When this is set, then
        /// the test execution should fail with this exception.
        /// </summary>
        public Exception InitializationException { get; protected set; }

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
                return
                    uniqueID ??
                    Interlocked.CompareExchange(ref uniqueID, GetUniqueID(), null) ??
                    uniqueID;
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
                var assemblyName = TestMethod.TestClass.TestCollection.TestAssembly.Assembly.Name;

                //Get just the assembly name (without version info) when obtained by reflection
                IReflectionAssemblyInfo assembly = TestMethod.TestClass.TestCollection.TestAssembly.Assembly as IReflectionAssemblyInfo;
                if (assembly != null)
                    assemblyName = assembly.Assembly.GetName().Name;

                Write(stream, assemblyName);
                Write(stream, TestMethod.TestClass.Class.Name);
                Write(stream, TestMethod.Method.Name);

                if (TestMethodArguments != null)
                    Write(stream, SerializationHelper.Serialize(TestMethodArguments));

                var genericTypes = MethodGenericTypes;
                if (genericTypes != null)
                    for (var idx = 0; idx < genericTypes.Length; idx++)
                        Write(stream, TypeUtility.ConvertToSimpleTypeName(genericTypes[idx]));

                stream.Position = 0;

                var hash = new byte[20];
                var data = stream.ToArray();

                var hasher = new Sha1Digest();
                hasher.BlockUpdate(data, 0, data.Length);
                hasher.DoFinal(hash, 0);

                return BytesToHexString(hash);
            }
        }

        /// <summary>Converts an array of bytes to its hexadecimal value as a string.</summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>A string containing the hexadecimal representation of the provided bytes.</returns>
        static string BytesToHexString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length * 2];
            int i = 0;
            foreach (byte b in bytes)
            {
                chars[i++] = NibbleToHexChar(b >> 4);
                chars[i++] = NibbleToHexChar(b & 0xF);
            }
            return new string(chars);
        }

        /// <summary>Gets a hexadecimal digit character from the 4-bit value.</summary>
        /// <param name="b">A value in the range [0, 15].</param>
        /// <returns>A character in the range ['0','9'] or ['a','f'].</returns>
        static char NibbleToHexChar(int b)
        {
            Debug.Assert(b < 16);
            return (char)(b < 10 ? b + '0' : (b - 10) + 'a');
        }

        /// <summary>
        /// Called when initializing the test cases, either after constructor or de-serialization.
        /// Override this method to add additional initialization-time work.
        /// </summary>
        protected virtual void Initialize()
        {
            Traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            Method = TestMethod.Method;

            if (TestMethodArguments != null)
            {
                if (Method is IReflectionMethodInfo reflectionMethod)
                {
                    try
                    {
                        TestMethodArguments = reflectionMethod.MethodInfo.ResolveMethodArguments(TestMethodArguments);
                    }
                    catch (Exception ex)
                    {
                        InitializationException = ex;
                        TestMethodArguments = null;
                        displayName = $"{BaseDisplayName}(???)";
                    }
                }
            }

            if (TestMethodArguments != null && method.IsGenericMethodDefinition)
            {
                methodGenericTypes = Method.ResolveGenericTypes(TestMethodArguments);
                Method = Method.MakeGenericMethod(MethodGenericTypes);
            }

            if (displayName == null)
                displayName = Method.GetDisplayNameWithArguments(BaseDisplayName, TestMethodArguments, MethodGenericTypes);
        }

        static void Write(Stream stream, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
            stream.WriteByte(0);
        }

        /// <inheritdoc/>
        public virtual void Serialize(IXunitSerializationInfo data)
        {
            data.AddValue("TestMethod", TestMethod);
            data.AddValue("TestMethodArguments", TestMethodArguments);
            data.AddValue("DefaultMethodDisplay", DefaultMethodDisplay.ToString());
            data.AddValue("DefaultMethodDisplayOptions", DefaultMethodDisplayOptions.ToString());
        }

        /// <inheritdoc/>
        public virtual void Deserialize(IXunitSerializationInfo data)
        {
            TestMethod = data.GetValue<ITestMethod>("TestMethod");
            TestMethodArguments = data.GetValue<object[]>("TestMethodArguments");
            DefaultMethodDisplay = (TestMethodDisplay)Enum.Parse(typeof(TestMethodDisplay), data.GetValue<string>("DefaultMethodDisplay"));
            DefaultMethodDisplayOptions = (TestMethodDisplayOptions)Enum.Parse(typeof(TestMethodDisplayOptions), data.GetValue<string>("DefaultMethodDisplayOptions"));
            formatter = new DisplayNameFormatter(DefaultMethodDisplay, DefaultMethodDisplayOptions);
        }
    }
}
