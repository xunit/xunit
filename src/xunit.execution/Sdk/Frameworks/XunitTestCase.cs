using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public class XunitTestCase : LongLivedMarshalByRefObject, IXunitTestCase, ISerializable
    {
        readonly static HashAlgorithm Hasher = new SHA1Managed();

        Lazy<string> uniqueID;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestCase"/> class.
        /// </summary>
        /// <param name="testCollection">The test collection this test case belongs to.</param>
        /// <param name="assembly">The test assembly.</param>
        /// <param name="type">The test class.</param>
        /// <param name="method">The test method.</param>
        /// <param name="factAttribute">The instance of the <see cref="FactAttribute"/>.</param>
        /// <param name="testMethodArguments">The arguments for the test method.</param>
        public XunitTestCase(ITestCollection testCollection,
                             IAssemblyInfo assembly,
                             ITypeInfo type,
                             IMethodInfo method,
                             IAttributeInfo factAttribute,
                             object[] testMethodArguments = null)
        {
            Initialize(testCollection, assembly, type, method, factAttribute, testMethodArguments);
        }

        /// <inheritdoc/>
        protected XunitTestCase(SerializationInfo info, StreamingContext context)
        {
            var assemblyName = info.GetString("AssemblyName");
            var typeName = info.GetString("TypeName");
            var methodName = info.GetString("MethodName");
            var arguments = (object[])info.GetValue("TestMethodArguments", typeof(object[]));
            var testCollection = (ITestCollection)info.GetValue("TestCollection", typeof(ITestCollection));

            var type = Reflector.GetType(assemblyName, typeName);
            var typeInfo = Reflector.Wrap(type);
            var methodInfo = Reflector.Wrap(type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
            var factAttribute = methodInfo.GetCustomAttributes(typeof(FactAttribute)).Single();

            Initialize(testCollection, Reflector.Wrap(type.Assembly), typeInfo, methodInfo, factAttribute, arguments);
        }

        void Initialize(ITestCollection testCollection, IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute, object[] arguments)
        {
            var baseDisplayName = factAttribute.GetNamedArgument<string>("DisplayName") ?? type.Name + "." + method.Name;
            ITypeInfo[] resolvedTypes = null;

            if (arguments != null && method.IsGenericMethodDefinition)
            {
                resolvedTypes = TypeUtility.ResolveGenericTypes(method, arguments);
                method = method.MakeGenericMethod(resolvedTypes);
            }

            Assembly = assembly;
            Class = type;
            Method = method;
            TestMethodArguments = arguments;
            DisplayName = TypeUtility.GetDisplayNameWithArguments(method, baseDisplayName, arguments, resolvedTypes);
            SkipReason = factAttribute.GetNamedArgument<string>("Skip");
            Traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            TestCollection = testCollection;

            foreach (var traitAttribute in Method.GetCustomAttributes(typeof(ITraitAttribute))
                                                 .Concat(Class.GetCustomAttributes(typeof(ITraitAttribute))))
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
        public IAssemblyInfo Assembly { get; private set; }

        /// <inheritdoc/>
        public ITypeInfo Class { get; private set; }

        /// <inheritdoc/>
        public virtual string DisplayName { get; private set; }

        /// <inheritdoc/>
        public IMethodInfo Method { get; private set; }

        /// <inheritdoc/>
        public string SkipReason { get; private set; }

        /// <inheritdoc/>
        public ISourceInformation SourceInformation { get; set; }

        /// <inheritdoc/>
        public ITestCollection TestCollection { get; private set; }

        /// <inheritdoc/>
        public object[] TestMethodArguments { get; private set; }

        /// <inheritdoc/>
        public Dictionary<string, List<string>> Traits { get; private set; }

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
            info.AddValue("AssemblyName", Assembly.Name);
            info.AddValue("TypeName", Class.Name);
            info.AddValue("MethodName", Method.Name);
            info.AddValue("TestMethodArguments", TestMethodArguments);
            info.AddValue("TestCollection", TestCollection);
        }

        string GetUniqueID()
        {
            using (var stream = new MemoryStream())
            {
                Write(stream, Assembly.Name);
                Write(stream, Class.Name);
                Write(stream, Method.Name);

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
