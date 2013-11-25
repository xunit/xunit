using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
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
    /// Default implementation of <see cref="ITestCase"/> for xUnit v2 that supports tests decorated with
    /// both <see cref="FactAttribute"/> and <see cref="TheoryAttribute"/>.
    /// </summary>
    [Serializable]
    public class XunitTestCase : LongLivedMarshalByRefObject, ITestCase, ISerializable
    {
        readonly static object[] EmptyArray = new object[0];
        readonly static MethodInfo EnumerableCast = typeof(Enumerable).GetMethod("Cast");
        readonly static MethodInfo EnumerableToArray = typeof(Enumerable).GetMethod("ToArray");
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
        /// <param name="arguments">The arguments for the test method.</param>
        public XunitTestCase(ITestCollection testCollection, IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute, object[] arguments = null)
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

            Assembly = assembly;
            Class = type;
            Method = method;
            Arguments = arguments;
            DisplayName = GetDisplayNameWithArguments(displayNameBase, arguments);
            SkipReason = factAttribute.GetNamedArgument<string>("Skip");
            Traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            TestCollection = testCollection;

            foreach (IAttributeInfo traitAttribute in Method.GetCustomAttributes(typeof(TraitAttribute)))
            {
                var ctorArgs = traitAttribute.GetConstructorArguments().ToList();
                Traits.Add((string)ctorArgs[0], (string)ctorArgs[1]);
            }

            uniqueID = new Lazy<string>(GetUniqueID, true);
        }

        /// <summary>
        /// The arguments that will be passed to the test method.
        /// </summary>
        public object[] Arguments { get; private set; }

        /// <inheritdoc/>
        public IAssemblyInfo Assembly { get; private set; }

        /// <inheritdoc/>
        public ITypeInfo Class { get; private set; }

        /// <inheritdoc/>
        public string DisplayName { get; private set; }

        /// <inheritdoc/>
        public IMethodInfo Method { get; private set; }

        /// <inheritdoc/>
        public string SkipReason { get; private set; }

        /// <inheritdoc/>
        public SourceInformation SourceInformation { get; internal set; }

        /// <inheritdoc/>
        public ITestCollection TestCollection { get; private set; }

        /// <inheritdoc/>
        public Dictionary<string, List<string>> Traits { get; private set; }

        /// <inheritdoc/>
        public string UniqueID { get { return uniqueID.Value; } }

        /// <summary>
        /// Converts arguments into their target types.
        /// </summary>
        /// <param name="args">The arguments to be converted.</param>
        /// <param name="types">The target types for the conversion.</param>
        /// <returns>The converted arguments.</returns>
        protected object[] ConvertArguments(object[] args, Type[] types)
        {
            if (args.Length == types.Length)
                for (int idx = 0; idx < args.Length; idx++)
                {
                    Type type = types[idx];
                    if (type.IsArray && args[idx] != null && args[idx].GetType() != type)
                    {
                        var elementType = type.GetElementType();
                        var arg = (IEnumerable<object>)args[idx];
                        var castMethod = EnumerableCast.MakeGenericMethod(elementType);
                        var toArrayMethod = EnumerableToArray.MakeGenericMethod(elementType);
                        args[idx] = toArrayMethod.Invoke(null, new object[] { castMethod.Invoke(null, new object[] { arg }) });
                    }
                }

            return args;
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
        /// <returns>The supplemented display name.</returns>
        protected string GetDisplayNameWithArguments(string displayName, object[] arguments)
        {
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
        public void GetObjectData(SerializationInfo info, StreamingContext context)
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
            return Reflector.GetType(Assembly.Name, Class.Name);
        }

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> of the method under test.
        /// </summary>
        /// <param name="type">The type the method is attached to.</param>
        /// <returns>The method under test, if possible; null, if not available.</returns>
        protected MethodInfo GetRuntimeMethod(Type type)
        {
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

        [SecuritySafeCritical]
        protected static bool OnMessage(IMessageSink messageSink, IMessageSinkMessage message)
        {
            var result = messageSink.OnMessage(message);
            RemotingServices.Disconnect((MarshalByRefObject)message);
            return result;
        }

        static string ParameterToDisplayValue(object parameterValue)
        {
            if (parameterValue == null)
                return "null";

            if (parameterValue is char)
                return "'" + parameterValue + "'";

            string stringParameter = parameterValue as string;
            if (stringParameter != null)
            {
                if (stringParameter.Length > 50)
                    return "\"" + stringParameter.Substring(0, 50) + "\"...";

                return "\"" + stringParameter + "\"";
            }

            return Convert.ToString(parameterValue, CultureInfo.CurrentCulture);
        }

        static string ParameterToDisplayValue(string parameterName, object parameterValue)
        {
            return parameterName + ": " + ParameterToDisplayValue(parameterValue);
        }

        /// <summary>
        /// Executes the test case, returning 0 or more result messages through the message sink.
        /// </summary>
        /// <param name="messageSink">The message sink to report results to.</param>
        /// <param name="constructorArguments">The arguments to pass to the constructor.</param>
        /// <param name="aggregator">The error aggregator to use for catching exception.</param>
        /// <param name="cancellationTokenSource">The cancellation token source that indicates whether cancellation has been requested.</param>
        public virtual void Run(IMessageSink messageSink, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            int totalFailed = 0;
            int totalRun = 0;
            int totalSkipped = 0;
            decimal executionTime = 0M;

            if (!OnMessage(messageSink, new TestCaseStarting(this)))
                cancellationTokenSource.Cancel();
            else
            {
                using (var delegatingSink = new DelegatingMessageSink(messageSink, msg =>
                {
                    if (msg is ITestResultMessage)
                    {
                        totalRun++;
                        executionTime += ((ITestResultMessage)msg).ExecutionTime;
                    }
                    if (msg is ITestFailed)
                        totalFailed++;
                    if (msg is ITestSkipped)
                        totalSkipped++;
                }))
                    RunTests(delegatingSink, constructorArguments, aggregator, cancellationTokenSource);
            }

            if (!OnMessage(messageSink, new TestCaseFinished(this, executionTime, totalRun, totalFailed, totalSkipped)))
                cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Run the tests in the test case.
        /// </summary>
        /// <param name="messageSink">The message sink to send results to.</param>
        /// <param name="constructorArguments">The arguments to pass to the constructor.</param>
        /// <param name="aggregator">The error aggregator to use for catching exception.</param>
        /// <param name="cancellationTokenSource">The cancellation token source that indicates whether cancellation has been requested.</param>
        protected virtual void RunTests(IMessageSink messageSink, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            var classUnderTest = GetRuntimeClass();
            var methodUnderTest = GetRuntimeMethod(classUnderTest);
            var beforeAfterAttributes = GetBeforeAfterAttributes(classUnderTest, methodUnderTest).ToList();
            decimal executionTime = 0M;

            RunTestsOnMethod(messageSink, classUnderTest, constructorArguments, methodUnderTest, beforeAfterAttributes, aggregator, cancellationTokenSource, ref executionTime);
        }

        /// <summary>
        /// Runs the tests for a given test method.
        /// </summary>
        /// <param name="messageSink">The message sink to send results to.</param>
        /// <param name="classUnderTest">The class under test.</param>
        /// <param name="constructorArguments">The arguments to pass to the constructor.</param>
        /// <param name="methodUnderTest">The method under test.</param>
        /// <param name="beforeAfterAttributes">The <see cref="BeforeAfterTestAttribute"/> instances attached to the test.</param>
        /// <param name="aggregator">The error aggregator to use for catching exception.</param>
        /// <param name="cancellationTokenSource">The cancellation token source that indicates whether cancellation has been requested.</param>
        /// <param name="executionTime">The time spent executing the tests.</param>
        protected virtual void RunTestsOnMethod(IMessageSink messageSink,
                                                Type classUnderTest,
                                                object[] constructorArguments,
                                                MethodInfo methodUnderTest,
                                                List<BeforeAfterTestAttribute> beforeAfterAttributes,
                                                ExceptionAggregator aggregator,
                                                CancellationTokenSource cancellationTokenSource,
                                                ref decimal executionTime)
        {
            RunTestWithArguments(messageSink, classUnderTest, constructorArguments, methodUnderTest, Arguments, DisplayName, beforeAfterAttributes, aggregator, cancellationTokenSource, ref executionTime);
        }

        /// <summary>
        /// Runs a single test for a given test method.
        /// </summary>
        /// <param name="messageSink">The message sink to send results to.</param>
        /// <param name="classUnderTest">The class under test.</param>
        /// <param name="constructorArguments">The arguments to pass to the constructor.</param>
        /// <param name="methodUnderTest">The method under test.</param>
        /// <param name="testMethodArguments">The arguments to pass to the test method.</param>
        /// <param name="displayName">The display name for the test.</param>
        /// <param name="beforeAfterAttributes">The <see cref="BeforeAfterTestAttribute"/> instances attached to the test.</param>
        /// <param name="parentAggregator">The parent aggregator that contains the exceptions up to this point.</param>
        /// <param name="cancellationTokenSource">The cancellation token source that indicates whether cancellation has been requested.</param>
        /// <param name="executionTime">The time spent executing the tests.</param>
        protected void RunTestWithArguments(IMessageSink messageSink,
                                            Type classUnderTest,
                                            object[] constructorArguments,
                                            MethodInfo methodUnderTest,
                                            object[] testMethodArguments,
                                            string displayName,
                                            List<BeforeAfterTestAttribute> beforeAfterAttributes,
                                            ExceptionAggregator parentAggregator,
                                            CancellationTokenSource cancellationTokenSource,
                                            ref decimal executionTime)
        {
            var aggregator = new ExceptionAggregator(parentAggregator);
            var output = String.Empty;  // TODO: Add output facilities for v2

            if (!OnMessage(messageSink, new TestStarting(this, displayName)))
                cancellationTokenSource.Cancel();
            else
            {
                if (!String.IsNullOrEmpty(SkipReason))
                {
                    if (!OnMessage(messageSink, new TestSkipped(this, displayName, SkipReason)))
                        cancellationTokenSource.Cancel();
                }
                else
                {
                    var beforeAttributesRun = new Stack<BeforeAfterTestAttribute>();
                    var stopwatch = Stopwatch.StartNew();

                    if (!aggregator.HasExceptions)
                        aggregator.Run(() =>
                        {
                            object testClass = null;

                            if (!methodUnderTest.IsStatic)
                            {
                                if (!OnMessage(messageSink, new TestClassConstructionStarting(this, displayName)))
                                    cancellationTokenSource.Cancel();

                                try
                                {
                                    if (!cancellationTokenSource.IsCancellationRequested)
                                        testClass = Activator.CreateInstance(classUnderTest, constructorArguments);
                                }
                                finally
                                {
                                    if (!OnMessage(messageSink, new TestClassConstructionFinished(this, displayName)))
                                        cancellationTokenSource.Cancel();
                                }
                            }

                            if (!cancellationTokenSource.IsCancellationRequested)
                            {
                                aggregator.Run(() =>
                                {
                                    foreach (var beforeAfterAttribute in beforeAfterAttributes)
                                    {
                                        var attributeName = beforeAfterAttribute.GetType().Name;
                                        if (!OnMessage(messageSink, new BeforeTestStarting(this, displayName, attributeName)))
                                            cancellationTokenSource.Cancel();
                                        else
                                        {
                                            try
                                            {
                                                beforeAfterAttribute.Before(methodUnderTest);
                                                beforeAttributesRun.Push(beforeAfterAttribute);
                                            }
                                            finally
                                            {
                                                if (!OnMessage(messageSink, new BeforeTestFinished(this, displayName, attributeName)))
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

                                            aggregator.Run(() =>
                                            {
                                                var result = methodUnderTest.Invoke(testClass, ConvertArguments(testMethodArguments ?? EmptyArray, parameterTypes));
                                                var task = result as Task;
                                                if (task != null)
                                                    task.GetAwaiter().GetResult();
                                                else
                                                {
                                                    var ex = asyncSyncContext.WaitForCompletion();
                                                    if (ex != null)
                                                        aggregator.Add(ex);
                                                }
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
                                    if (!OnMessage(messageSink, new AfterTestStarting(this, displayName, attributeName)))
                                        cancellationTokenSource.Cancel();

                                    aggregator.Run(() => beforeAfterAttribute.After(methodUnderTest));

                                    if (!OnMessage(messageSink, new AfterTestFinished(this, displayName, attributeName)))
                                        cancellationTokenSource.Cancel();
                                }
                            }

                            aggregator.Run(() =>
                            {
                                IDisposable disposable = testClass as IDisposable;
                                if (disposable != null)
                                {
                                    if (!OnMessage(messageSink, new TestClassDisposeStarting(this, displayName)))
                                        cancellationTokenSource.Cancel();

                                    try
                                    {
                                        disposable.Dispose();
                                    }
                                    finally
                                    {
                                        if (!OnMessage(messageSink, new TestClassDisposeFinished(this, displayName)))
                                            cancellationTokenSource.Cancel();
                                    }
                                }
                            });
                        });

                    stopwatch.Stop();

                    if (!cancellationTokenSource.IsCancellationRequested)
                    {
                        executionTime = (decimal)stopwatch.Elapsed.TotalSeconds;

                        var exception = aggregator.ToException();
                        var testResult = exception == null ? (TestResultMessage)new TestPassed(this, displayName, executionTime, output) : new TestFailed(this, displayName, executionTime, output, exception);
                        if (!OnMessage(messageSink, testResult))
                            cancellationTokenSource.Cancel();
                    }
                }
            }

            if (!OnMessage(messageSink, new TestFinished(this, displayName, executionTime, output)))
                cancellationTokenSource.Cancel();
        }

        [SecuritySafeCritical]
        void SetSynchronizationContext(SynchronizationContext context)
        {
            SynchronizationContext.SetSynchronizationContext(context);
        }
    }
}