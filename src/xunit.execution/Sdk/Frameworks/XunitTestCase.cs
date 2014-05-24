using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        readonly static ITypeInfo ObjectTypeInfo = Reflector.Wrap(typeof(object));

        Lazy<string> uniqueID;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestCase"/> class.
        /// </summary>
        /// <param name="testCollection">The test collection this test case belongs to.</param>
        /// <param name="assembly">The test assembly.</param>
        /// <param name="type">The test class.</param>
        /// <param name="method">The test method.</param>
        /// <param name="factAttribute">The instance of the <see cref="FactAttribute"/>.</param>
        /// <param name="arguments">The arguments for the test method.</param>
        public XunitTestCase(ITestCollection testCollection,
                             IAssemblyInfo assembly,
                             ITypeInfo type,
                             IMethodInfo method,
                             IAttributeInfo factAttribute,
                             object[] arguments = null)
        {
            Initialize(testCollection, assembly, type, method, factAttribute, arguments);
        }

        /// <inheritdoc/>
        protected XunitTestCase(SerializationInfo info, StreamingContext context)
        {
            string assemblyName = info.GetString("AssemblyName");
            string typeName = info.GetString("TypeName");
            string methodName = info.GetString("MethodName");
            object[] arguments = (object[])info.GetValue("Arguments", typeof(object[]));
            var testCollection = (ITestCollection)info.GetValue("TestCollection", typeof(ITestCollection));

            var type = Reflector.GetType(assemblyName, typeName);
            var typeInfo = Reflector.Wrap(type);
            var methodInfo = Reflector.Wrap(type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
            var factAttribute = methodInfo.GetCustomAttributes(typeof(FactAttribute)).Single();

            Initialize(testCollection, Reflector.Wrap(type.Assembly), typeInfo, methodInfo, factAttribute, arguments);
        }

        void Initialize(ITestCollection testCollection, IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute, object[] arguments)
        {
            string displayNameBase = factAttribute.GetNamedArgument<string>("DisplayName") ?? type.Name + "." + method.Name;
            ITypeInfo[] resolvedTypes = null;

            if (arguments != null && method.IsGenericMethodDefinition)
            {
                resolvedTypes = ResolveGenericTypes(method, arguments);
                method = method.MakeGenericMethod(resolvedTypes);
            }

            Assembly = assembly;
            Class = type;
            Method = method;
            Arguments = arguments;
            DisplayName = GetDisplayNameWithArguments(displayNameBase, arguments, resolvedTypes);
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
        public object[] Arguments { get; private set; }

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
        public Dictionary<string, List<string>> Traits { get; private set; }

        /// <inheritdoc/>
        public string UniqueID { get { return uniqueID.Value; } }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Arguments != null)
                foreach (var disposable in Arguments.OfType<IDisposable>())
                    disposable.Dispose();
        }

        /// <summary>
        /// Gets the <see cref="BeforeAfterTestAttribute"/> instances for a test method.
        /// </summary>
        /// <param name="classUnderTest">The class under test.</param>
        /// <param name="methodUnderTest">The method under test.</param>
        /// <returns>The list of <see cref="BeforeAfterTestAttribute"/> instances.</returns>
        protected virtual IEnumerable<BeforeAfterTestAttribute> GetBeforeAfterAttributes(Type classUnderTest, MethodInfo methodUnderTest)
        {
            return classUnderTest.GetCustomAttributes(typeof(BeforeAfterTestAttribute))
                                 .Concat(methodUnderTest.GetCustomAttributes(typeof(BeforeAfterTestAttribute)))
                                 .Cast<BeforeAfterTestAttribute>();
        }

        /// <summary>
        /// Supplements a display name for a test method with its arguments.
        /// </summary>
        /// <param name="displayName">The base display name.</param>
        /// <param name="arguments">The test method's arguments.</param>
        /// <param name="genericTypes">The generic types for the test method.</param>
        /// <returns>The supplemented display name.</returns>
        protected string GetDisplayNameWithArguments(string displayName, object[] arguments, ITypeInfo[] genericTypes)
        {
            displayName += ResolveGenericDisplay(genericTypes);

            if (arguments == null)
                return displayName;

            IParameterInfo[] parameterInfos = Method.GetParameters().ToArray();
            string[] displayValues = new string[Math.Max(arguments.Length, parameterInfos.Length)];
            int idx;

            for (idx = 0; idx < arguments.Length; idx++)
                displayValues[idx] = ParameterToDisplayValue(GetParameterName(parameterInfos, idx), arguments[idx]);

            for (; idx < parameterInfos.Length; idx++)  // Fill-in any missing parameters with "???"
                displayValues[idx] = GetParameterName(parameterInfos, idx) + ": ???";

            return String.Format(CultureInfo.CurrentCulture, "{0}({1})", displayName, string.Join(", ", displayValues));
        }

        /// <inheritdoc/>
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("AssemblyName", Assembly.Name);
            info.AddValue("TypeName", Class.Name);
            info.AddValue("MethodName", Method.Name);
            info.AddValue("Arguments", Arguments);
            info.AddValue("TestCollection", TestCollection);
        }

        static string GetParameterName(IParameterInfo[] parameters, int index)
        {
            if (index >= parameters.Length)
                return "???";

            return parameters[index].Name;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> of the class under test.
        /// </summary>
        /// <returns>The type under test, if possible; null, if not available.</returns>
        protected Type GetRuntimeClass()
        {
            var reflectionTypeInfo = Class as IReflectionTypeInfo;
            if (reflectionTypeInfo != null)
                return reflectionTypeInfo.Type;

            return Reflector.GetType(Assembly.Name, Class.Name);
        }

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> of the method under test.
        /// </summary>
        /// <param name="type">The type the method is attached to.</param>
        /// <returns>The method under test, if possible; null, if not available.</returns>
        protected MethodInfo GetRuntimeMethod(Type type)
        {
            var reflectionMethodInfo = Method as IReflectionMethodInfo;
            if (reflectionMethodInfo != null)
                return reflectionMethodInfo.MethodInfo;

            if (type == null)
                return null;

            return type.GetMethod(Method.Name, Method.GetBindingFlags());
        }

        string GetUniqueID()
        {
            using (var stream = new MemoryStream())
            {
                Write(stream, Assembly.Name);
                Write(stream, Class.Name);
                Write(stream, Method.Name);

                if (Arguments != null)
                    Write(stream, SerializationHelper.Serialize(Arguments));

                stream.Position = 0;
                byte[] hash = Hasher.ComputeHash(stream);
                return String.Join("", hash.Select(x => x.ToString("x2")).ToArray());
            }
        }

        static void Write(Stream stream, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
            stream.WriteByte(0);
        }

        static string ParameterToDisplayValue(string parameterName, object parameterValue)
        {
            return String.Format("{0}: {1}", parameterName, ArgumentFormatter.Format(parameterValue));
        }

        static string ConvertToSimpleTypeName(ITypeInfo type)
        {
            var baseTypeName = type.Name;

            int backTickIdx = baseTypeName.IndexOf('`');
            if (backTickIdx >= 0)
                baseTypeName = baseTypeName.Substring(0, backTickIdx);

            var lastIndex = baseTypeName.LastIndexOf('.');
            if (lastIndex >= 0)
                baseTypeName = baseTypeName.Substring(lastIndex + 1);

            if (!type.IsGenericType)
                return baseTypeName;

            ITypeInfo[] genericTypes = type.GetGenericArguments().ToArray();
            string[] simpleNames = new string[genericTypes.Length];

            for (int idx = 0; idx < genericTypes.Length; idx++)
                simpleNames[idx] = ConvertToSimpleTypeName(genericTypes[idx]);

            return String.Format("{0}<{1}>", baseTypeName, String.Join(", ", simpleNames));
        }

        static string ResolveGenericDisplay(ITypeInfo[] genericTypes)
        {
            if (genericTypes == null || genericTypes.Length == 0)
                return String.Empty;

            string[] typeNames = new string[genericTypes.Length];
            for (var idx = 0; idx < genericTypes.Length; idx++)
                typeNames[idx] = ConvertToSimpleTypeName(genericTypes[idx]);

            return String.Format("<{0}>", String.Join(", ", typeNames));
        }

        static ITypeInfo ResolveGenericType(ITypeInfo genericType, object[] parameters, IParameterInfo[] parameterInfos)
        {
            bool sawNullValue = false;
            ITypeInfo matchedType = null;

            for (int idx = 0; idx < parameterInfos.Length; ++idx)
            {
                var parameterType = parameterInfos[idx].ParameterType;
                if (parameterType.IsGenericParameter && parameterType.Name == genericType.Name)
                {
                    object parameterValue = parameters[idx];

                    if (parameterValue == null)
                        sawNullValue = true;
                    else if (matchedType == null)
                        matchedType = Reflector.Wrap(parameterValue.GetType());
                    else if (matchedType.Name != parameterValue.GetType().FullName)
                        return ObjectTypeInfo;
                }
            }

            if (matchedType == null)
                return ObjectTypeInfo;

            return sawNullValue && matchedType.IsValueType ? ObjectTypeInfo : matchedType;
        }

        /// <summary>
        /// FOR INTERNAL USE ONLY.
        /// </summary>
        protected static ITypeInfo[] ResolveGenericTypes(IMethodInfo method, object[] parameters)
        {
            ITypeInfo[] genericTypes = method.GetGenericArguments().ToArray();
            ITypeInfo[] resolvedTypes = new ITypeInfo[genericTypes.Length];
            IParameterInfo[] parameterInfos = method.GetParameters().ToArray();

            for (int idx = 0; idx < genericTypes.Length; ++idx)
                resolvedTypes[idx] = ResolveGenericType(genericTypes[idx], parameters, parameterInfos);

            return resolvedTypes;
        }

        /// <inheritdoc/>
        public virtual async Task<RunSummary> RunAsync(IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            var summary = new RunSummary();

            if (!messageBus.QueueMessage(new TestCaseStarting(this)))
                cancellationTokenSource.Cancel();
            else
            {
                using (var delegatingBus = new DelegatingMessageBus(messageBus,
                    msg =>
                    {
                        if (msg is ITestResultMessage)
                        {
                            summary.Total++;
                            summary.Time += ((ITestResultMessage)msg).ExecutionTime;
                        }
                        if (msg is ITestFailed)
                            summary.Failed++;
                        if (msg is ITestSkipped)
                            summary.Skipped++;
                    }))
                {
                    await RunTestsAsync(delegatingBus, constructorArguments, aggregator, cancellationTokenSource);
                }
            }

            if (!messageBus.QueueMessage(new TestCaseFinished(this, summary.Time, summary.Total, summary.Failed, summary.Skipped)))
                cancellationTokenSource.Cancel();

            return summary;
        }

        /// <summary>
        /// Run the tests in the test case.
        /// </summary>
        /// <param name="messageBus">The message bus to send results to.</param>
        /// <param name="constructorArguments">The arguments to pass to the constructor.</param>
        /// <param name="aggregator">The error aggregator to use for catching exception.</param>
        /// <param name="cancellationTokenSource">The cancellation token source that indicates whether cancellation has been requested.</param>
        protected virtual Task RunTestsAsync(IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            var classUnderTest = GetRuntimeClass();
            var methodUnderTest = GetRuntimeMethod(classUnderTest);
            var beforeAfterAttributes = GetBeforeAfterAttributes(classUnderTest, methodUnderTest).ToList();

            return RunTestsOnMethodAsync(messageBus, classUnderTest, constructorArguments, methodUnderTest, beforeAfterAttributes, aggregator, cancellationTokenSource);
        }

        /// <summary>
        /// Runs the tests for a given test method.
        /// </summary>
        /// <param name="messageBus">The message bus to send results to.</param>
        /// <param name="classUnderTest">The class under test.</param>
        /// <param name="constructorArguments">The arguments to pass to the constructor.</param>
        /// <param name="methodUnderTest">The method under test.</param>
        /// <param name="beforeAfterAttributes">The <see cref="BeforeAfterTestAttribute"/> instances attached to the test.</param>
        /// <param name="aggregator">The error aggregator to use for catching exception.</param>
        /// <param name="cancellationTokenSource">The cancellation token source that indicates whether cancellation has been requested.</param>
        protected virtual Task RunTestsOnMethodAsync(IMessageBus messageBus,
                                                     Type classUnderTest,
                                                     object[] constructorArguments,
                                                     MethodInfo methodUnderTest,
                                                     List<BeforeAfterTestAttribute> beforeAfterAttributes,
                                                     ExceptionAggregator aggregator,
                                                     CancellationTokenSource cancellationTokenSource)
        {
            return RunTestWithArgumentsAsync(messageBus, classUnderTest, constructorArguments, methodUnderTest, Arguments, DisplayName, beforeAfterAttributes, aggregator, cancellationTokenSource);
        }

        /// <summary>
        /// Runs a single test for a given test method.
        /// </summary>
        /// <param name="messageBus">The message bus to send results to.</param>
        /// <param name="classUnderTest">The class under test.</param>
        /// <param name="constructorArguments">The arguments to pass to the constructor.</param>
        /// <param name="methodUnderTest">The method under test.</param>
        /// <param name="testMethodArguments">The arguments to pass to the test method.</param>
        /// <param name="displayName">The display name for the test.</param>
        /// <param name="beforeAfterAttributes">The <see cref="BeforeAfterTestAttribute"/> instances attached to the test.</param>
        /// <param name="parentAggregator">The parent aggregator that contains the exceptions up to this point.</param>
        /// <param name="cancellationTokenSource">The cancellation token source that indicates whether cancellation has been requested.</param>
        protected async Task<decimal> RunTestWithArgumentsAsync(IMessageBus messageBus,
                                                                Type classUnderTest,
                                                                object[] constructorArguments,
                                                                MethodInfo methodUnderTest,
                                                                object[] testMethodArguments,
                                                                string displayName,
                                                                List<BeforeAfterTestAttribute> beforeAfterAttributes,
                                                                ExceptionAggregator parentAggregator,
                                                                CancellationTokenSource cancellationTokenSource)
         {
            var executionTimeInSeconds = 0.0m;
            var aggregator = new ExceptionAggregator(parentAggregator);
            var output = String.Empty;  // TODO: Add output facilities for v2

            if (!messageBus.QueueMessage(new TestStarting(this, displayName)))
                cancellationTokenSource.Cancel();
            else
            {
                if (!String.IsNullOrEmpty(SkipReason))
                {
                    if (!messageBus.QueueMessage(new TestSkipped(this, displayName, SkipReason)))
                        cancellationTokenSource.Cancel();
                }
                else
                {
                    var beforeAttributesRun = new Stack<BeforeAfterTestAttribute>();
                    var executionTime = new ExecutionTime();

                    if (!aggregator.HasExceptions)
                        await aggregator.RunAsync(async () =>
                        {
                            object testClass = null;

                            if (!methodUnderTest.IsStatic)
                            {
                                if (!messageBus.QueueMessage(new TestClassConstructionStarting(this, displayName)))
                                    cancellationTokenSource.Cancel();

                                try
                                {
                                    if (!cancellationTokenSource.IsCancellationRequested)
                                        executionTime.Aggregate(() => testClass = Activator.CreateInstance(classUnderTest, constructorArguments));
                                }
                                finally
                                {
                                    if (!messageBus.QueueMessage(new TestClassConstructionFinished(this, displayName)))
                                        cancellationTokenSource.Cancel();
                                }
                            }

                            if (!cancellationTokenSource.IsCancellationRequested)
                            {
                                await aggregator.RunAsync(async () =>
                                {
                                    foreach (var beforeAfterAttribute in beforeAfterAttributes)
                                    {
                                        var attributeName = beforeAfterAttribute.GetType().Name;
                                        if (!messageBus.QueueMessage(new BeforeTestStarting(this, displayName, attributeName)))
                                            cancellationTokenSource.Cancel();
                                        else
                                        {
                                            try
                                            {
                                                executionTime.Aggregate(() => beforeAfterAttribute.Before(methodUnderTest));
                                                beforeAttributesRun.Push(beforeAfterAttribute);
                                            }
                                            finally
                                            {
                                                if (!messageBus.QueueMessage(new BeforeTestFinished(this, displayName, attributeName)))
                                                    cancellationTokenSource.Cancel();
                                            }
                                        }

                                        if (cancellationTokenSource.IsCancellationRequested)
                                            return;
                                    }

                                    if (!cancellationTokenSource.IsCancellationRequested)
                                    {
                                        var parameterTypes = methodUnderTest.GetParameters().Select(p => p.ParameterType).ToArray();
                                        var oldSyncContext = SynchronizationContext.Current;

                                        try
                                        {
                                            var asyncSyncContext = new AsyncTestSyncContext();
                                            SetSynchronizationContext(asyncSyncContext);

                                            await aggregator.RunAsync(async () =>
                                            {
                                                await executionTime.AggregateAsync(async () =>
                                                {
                                                    var result = methodUnderTest.Invoke(testClass, Reflector.ConvertArguments(testMethodArguments, parameterTypes));
                                                    var task = result as Task;
                                                    if (task != null)
                                                        await task;
                                                    else
                                                    {
                                                        var ex = await asyncSyncContext.WaitForCompletionAsync();
                                                        if (ex != null)
                                                            aggregator.Add(ex);
                                                    }
                                                });
                                            });
                                        }
                                        finally
                                        {
                                            SetSynchronizationContext(oldSyncContext);
                                        }
                                    }
                                });

                                foreach (var beforeAfterAttribute in beforeAttributesRun)
                                {
                                    var attributeName = beforeAfterAttribute.GetType().Name;
                                    if (!messageBus.QueueMessage(new AfterTestStarting(this, displayName, attributeName)))
                                        cancellationTokenSource.Cancel();

                                    aggregator.Run(() => executionTime.Aggregate(() => beforeAfterAttribute.After(methodUnderTest)));

                                    if (!messageBus.QueueMessage(new AfterTestFinished(this, displayName, attributeName)))
                                        cancellationTokenSource.Cancel();
                                }
                            }

                            aggregator.Run(() =>
                            {
                                var disposable = testClass as IDisposable;
                                if (disposable != null)
                                {
                                    if (!messageBus.QueueMessage(new TestClassDisposeStarting(this, displayName)))
                                        cancellationTokenSource.Cancel();

                                    try
                                    {
                                        executionTime.Aggregate(disposable.Dispose);
                                    }
                                    finally
                                    {
                                        if (!messageBus.QueueMessage(new TestClassDisposeFinished(this, displayName)))
                                            cancellationTokenSource.Cancel();
                                    }
                                }
                            });
                        });

                    if (!cancellationTokenSource.IsCancellationRequested)
                    {
                        executionTimeInSeconds = (decimal)executionTime.Total.TotalSeconds;

                        var exception = aggregator.ToException();
                        var testResult = exception == null ? (TestResultMessage)new TestPassed(this, displayName, executionTimeInSeconds, output)
                                                           : new TestFailed(this, displayName, executionTimeInSeconds, output, exception);
                        if (!messageBus.QueueMessage(testResult))
                            cancellationTokenSource.Cancel();
                    }
                }
            }

            if (!messageBus.QueueMessage(new TestFinished(this, displayName, executionTimeInSeconds, output)))
                cancellationTokenSource.Cancel();

            return executionTimeInSeconds;
        }

        [SecuritySafeCritical]
        static void SetSynchronizationContext(SynchronizationContext context)
        {
            SynchronizationContext.SetSynchronizationContext(context);
        }
    }
}