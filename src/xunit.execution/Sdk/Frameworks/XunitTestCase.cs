using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IXunitTestCase"/> for xUnit v2 that supports tests decorated with
    /// both <see cref="FactAttribute"/> and <see cref="TheoryAttribute"/>.
    /// </summary>
    [Serializable]
    [DebuggerDisplay(@"\{ class = {TestMethod.TestClass.Class.Name}, method = {TestMethod.Method.Name}, display = {DisplayName}, skip = {SkipReason} \}")]
    public class XunitTestCase : LongLivedMarshalByRefObject, IXunitTestCase, ISerializable
    {
        readonly static HashAlgorithm Hasher = new SHA1Managed();

        Lazy<string> uniqueID;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestCase"/> class.
        /// </summary>
        /// <param name="testMethod">The test method this test case belongs to.</param>
        /// <param name="testMethodArguments">The arguments for the test method.</param>
        public XunitTestCase(ITestMethod testMethod, object[] testMethodArguments = null)
        {
            Initialize(testMethod, testMethodArguments);
        }

        /// <inheritdoc/>
        protected XunitTestCase(SerializationInfo info, StreamingContext context)
        {
            var arguments = info.GetValue<object[]>("TestMethodArguments");
            var testMethod = info.GetValue<ITestMethod>("TestMethod");

            Initialize(testMethod, arguments);
        }

        void Initialize(ITestMethod testMethod, object[] arguments)
        {
            var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();
            var type = testMethod.TestClass.Class;
            var method = testMethod.Method;
            var baseDisplayName = factAttribute.GetNamedArgument<string>("DisplayName") ?? type.Name + "." + method.Name;
            ITypeInfo[] resolvedTypes = null;

            if (arguments != null && method.IsGenericMethodDefinition)
            {
                resolvedTypes = TypeUtility.ResolveGenericTypes(method, arguments);
                method = method.MakeGenericMethod(resolvedTypes);
            }

            Method = method;
            TestMethod = testMethod;
            TestMethodArguments = arguments;
            DisplayName = TypeUtility.GetDisplayNameWithArguments(method, baseDisplayName, arguments, resolvedTypes);
            SkipReason = factAttribute.GetNamedArgument<string>("Skip");
            Traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var traitAttribute in method.GetCustomAttributes(typeof(ITraitAttribute))
                                                 .Concat(type.GetCustomAttributes(typeof(ITraitAttribute))))
            {
                var discovererAttribute = traitAttribute.GetCustomAttributes(typeof(TraitDiscovererAttribute)).First();
                var discoverer = ExtensibilityPointFactory.GetTraitDiscoverer(discovererAttribute);
                if (discoverer != null)
                    foreach (var keyValuePair in discoverer.GetTraits(traitAttribute))
                        Traits.Add(keyValuePair.Key, keyValuePair.Value);
            }

            uniqueID = new Lazy<string>(GetUniqueID, true);
        }

        /// <inheritdoc/>
        public string DisplayName { get; set; }

        /// <inheritdoc/>
        public string SkipReason { get; set; }

        /// <inheritdoc/>
        public ISourceInformation SourceInformation { get; set; }

        /// <inheritdoc/>
        public IMethodInfo Method { get; set; }

        /// <inheritdoc/>
        public ITestMethod TestMethod { get; set; }

        /// <inheritdoc/>
        public object[] TestMethodArguments { get; set; }

        /// <inheritdoc/>
        public Dictionary<string, List<string>> Traits { get; set; }

        /// <inheritdoc/>
        public string UniqueID { get { return uniqueID.Value; } }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (TestMethodArguments != null)
                foreach (var disposable in TestMethodArguments.OfType<IDisposable>())
                    disposable.Dispose();
        }

        /// <inheritdoc/>
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("TestMethod", TestMethod);
            info.AddValue("TestMethodArguments", TestMethodArguments);
        }

        string GetUniqueID()
        {
            using (var stream = new MemoryStream())
            {
                Write(stream, TestMethod.TestClass.TestCollection.TestAssembly.Assembly.Name);
                Write(stream, TestMethod.TestClass.Class.Name);
                Write(stream, TestMethod.Method.Name);

                if (TestMethodArguments != null)
                    Write(stream, SerializationHelper.Serialize(TestMethodArguments));

                stream.Position = 0;
                var hash = Hasher.ComputeHash(stream);
                return String.Join("", hash.Select(x => x.ToString("x2")).ToArray());
            }
        }

        static void Write(Stream stream, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
            stream.WriteByte(0);
        }

        /// <inheritdoc/>
        public virtual Task<RunSummary> RunAsync(IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            return new XunitTestCaseRunner(this, DisplayName, SkipReason, constructorArguments, TestMethodArguments, messageBus, aggregator, cancellationTokenSource).RunAsync();
        }
    }
}
