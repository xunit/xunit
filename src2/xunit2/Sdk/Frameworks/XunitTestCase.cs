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
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IXunitTestCase"/> that supports tests decorated with
    /// both <see cref="FactAttribute"/> and <see cref="TheoryAttribute"/>.
    /// </summary>
    [Serializable]
    public class XunitTestCase : LongLivedMarshalByRefObject, IXunitTestCase, ISerializable
    {
        readonly static HashAlgorithm hasher = new SHA1Managed();

        readonly static object[] EmptyArray = new object[0];
        readonly static MethodInfo EnumerableCast = typeof(Enumerable).GetMethod("Cast");
        readonly static MethodInfo EnumerableToArray = typeof(Enumerable).GetMethod("ToArray");

        Lazy<string> uniqueID;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestCase"/> class.
        /// </summary>
        /// <param name="assembly">The test assembly.</param>
        /// <param name="type">The test class.</param>
        /// <param name="method">The test method.</param>
        /// <param name="factAttribute">The instance of the <see cref="FactAttribute"/>.</param>
        /// <param name="arguments">The arguments for the test method.</param>
        public XunitTestCase(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute, object[] arguments = null)
        {
            Initialize(assembly, type, method, factAttribute, arguments);
        }

        /// <inheritdoc/>
        protected XunitTestCase(SerializationInfo info, StreamingContext context)
        {
            string assemblyName = info.GetString("AssemblyName");
            string typeName = info.GetString("TypeName");
            string methodName = info.GetString("MethodName");
            object[] arguments = (object[])info.GetValue("Arguments", typeof(object[]));

            var type = Reflector.GetType(typeName, assemblyName);
            var typeInfo = Reflector.Wrap(type);
            var methodInfo = Reflector.Wrap(type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
            var factAttribute = methodInfo.GetCustomAttributes(typeof(FactAttribute)).Single();

            Initialize(Reflector.Wrap(type.Assembly), typeInfo, methodInfo, factAttribute, arguments);
        }

        void Initialize(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute, object[] arguments)
        {
            string displayNameBase = factAttribute.GetNamedArgument<string>("DisplayName") ?? type.Name + "." + method.Name;

            Assembly = assembly;
            Class = type;
            Method = method;
            Arguments = arguments;
            DisplayName = GetDisplayNameWithArguments(displayNameBase, arguments);
            SkipReason = factAttribute.GetNamedArgument<string>("Skip");
            Traits = new Dictionary<string, string>();

            foreach (IAttributeInfo traitAttribute in Method.GetCustomAttributes(typeof(TraitAttribute)))
            {
                var ctorArgs = traitAttribute.GetConstructorArguments().ToList();
                Traits.Add((string)ctorArgs[0], (string)ctorArgs[1]);
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
        public string DisplayName { get; private set; }

        /// <inheritdoc/>
        public IMethodInfo Method { get; private set; }

        /// <inheritdoc/>
        public string SkipReason { get; private set; }

        /// <inheritdoc/>
        public int? SourceFileLine { get; internal set; }

        /// <inheritdoc/>
        public string SourceFileName { get; internal set; }

        /// <inheritdoc/>
        public ITestCollection TestCollection { get; private set; }

        /// <inheritdoc/>
        public IDictionary<string, string> Traits { get; private set; }

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

        static string ConvertToSimpleTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            Type[] genericTypes = type.GetGenericArguments();
            string[] simpleNames = new string[genericTypes.Length];

            for (int idx = 0; idx < genericTypes.Length; idx++)
                simpleNames[idx] = ConvertToSimpleTypeName(genericTypes[idx]);

            string baseTypeName = type.Name;
            int backTickIdx = type.Name.IndexOf('`');

            return baseTypeName.Substring(0, backTickIdx) + "<" + String.Join(", ", simpleNames) + ">";
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
            return Reflector.GetType(Class.Name, Assembly.Name);
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
                byte[] hash = hasher.ComputeHash(stream);
                return String.Join("", hash.Select(x => x.ToString("x2")).ToArray());
            }
        }

        static void Write(Stream stream, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
            stream.WriteByte(0);
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

        /// <inheritdoc/>
        public virtual bool Run(IMessageSink messageSink)
        {
            bool cancelled = false;
            int totalFailed = 0;
            int totalRun = 0;
            int totalSkipped = 0;
            decimal executionTime = 0M;

            if (!messageSink.OnMessage(new TestCaseStarting { TestCase = this }))
                cancelled = true;
            else
            {
                var delegatingSink = new DelegatingMessageSink(messageSink, msg =>
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
                });

                cancelled = RunTests(delegatingSink);
            }

            if (!messageSink.OnMessage(new TestCaseFinished
            {
                ExecutionTime = executionTime,
                TestCase = this,
                TestsRun = totalRun,
                TestsFailed = totalFailed,
                TestsSkipped = totalSkipped
            }))
                cancelled = true;

            return cancelled;
        }

        /// <summary>
        /// Run the tests in the test case.
        /// </summary>
        /// <param name="messageSink">The message sink to send results to.</param>
        protected virtual bool RunTests(IMessageSink messageSink)
        {
            var classUnderTest = GetRuntimeClass();
            var methodUnderTest = GetRuntimeMethod(classUnderTest);
            var beforeAfterAttributes = GetBeforeAfterAttributes(classUnderTest, methodUnderTest).ToList();
            decimal executionTime = 0M;

            return RunTestsOnMethod(messageSink, classUnderTest, methodUnderTest, beforeAfterAttributes, ref executionTime);
        }

        /// <summary>
        /// Runs the tests for a given test method.
        /// </summary>
        /// <param name="messageSink">The message sink to send results to.</param>
        /// <param name="classUnderTest">The class under test.</param>
        /// <param name="methodUnderTest">The method under test.</param>
        /// <param name="beforeAfterAttributes">The <see cref="BeforeAfterTestAttribute"/> instances attached to the test.</param>
        /// <param name="executionTime">The time spent executing the tests.</param>
        protected virtual bool RunTestsOnMethod(IMessageSink messageSink,
                                                Type classUnderTest,
                                                MethodInfo methodUnderTest,
                                                List<BeforeAfterTestAttribute> beforeAfterAttributes,
                                                ref decimal executionTime)
        {
            return RunTestWithArguments(messageSink, classUnderTest, methodUnderTest, Arguments, DisplayName, beforeAfterAttributes, ref executionTime);
        }

        /// <summary>
        /// Runs a single test for a given test method.
        /// </summary>
        /// <param name="messageSink">The message sink to send results to.</param>
        /// <param name="classUnderTest">The class under test.</param>
        /// <param name="methodUnderTest">The method under test.</param>
        /// <param name="arguments">The arguments to pass to the test method.</param>
        /// <param name="displayName">The display name for the test.</param>
        /// <param name="beforeAfterAttributes">The <see cref="BeforeAfterTestAttribute"/> instances attached to the test.</param>
        /// <param name="executionTime">The time spent executing the tests.</param>
        protected bool RunTestWithArguments(IMessageSink messageSink,
                                            Type classUnderTest,
                                            MethodInfo methodUnderTest,
                                            object[] arguments,
                                            string displayName,
                                            List<BeforeAfterTestAttribute> beforeAfterAttributes,
                                            ref decimal executionTime)
        {
            bool cancelled = false;

            if (!messageSink.OnMessage(new TestStarting { TestCase = this, TestDisplayName = displayName }))
                cancelled = true;
            else
            {
                if (!String.IsNullOrEmpty(SkipReason))
                {
                    if (!messageSink.OnMessage(new TestSkipped { TestCase = this, TestDisplayName = DisplayName, Reason = SkipReason }))
                        cancelled = true;
                }
                else
                {
                    var aggregator = new ExceptionAggregator();
                    var beforeAttributesRun = new Stack<BeforeAfterTestAttribute>();
                    var stopwatch = Stopwatch.StartNew();

                    aggregator.Run(() =>
                    {
                        object testClass = null;

                        if (!methodUnderTest.IsStatic)
                        {
                            if (!messageSink.OnMessage(new TestClassConstructionStarting { TestCase = this, TestDisplayName = displayName }))
                                cancelled = true;

                            try
                            {
                                if (!cancelled)
                                    testClass = Activator.CreateInstance(classUnderTest);
                            }
                            finally
                            {
                                if (!messageSink.OnMessage(new TestClassConstructionFinished { TestCase = this, TestDisplayName = displayName }))
                                    cancelled = true;
                            }
                        }

                        if (!cancelled)
                        {
                            aggregator.Run(() =>
                            {
                                foreach (var beforeAfterAttribute in beforeAfterAttributes)
                                {
                                    if (!messageSink.OnMessage(new BeforeTestStarting { TestCase = this, TestDisplayName = displayName, AttributeName = beforeAfterAttribute.GetType().Name }))
                                        cancelled = true;
                                    else
                                    {
                                        try
                                        {
                                            beforeAfterAttribute.Before(methodUnderTest);
                                            beforeAttributesRun.Push(beforeAfterAttribute);
                                        }
                                        finally
                                        {
                                            if (!messageSink.OnMessage(new BeforeTestFinished { TestCase = this, TestDisplayName = displayName, AttributeName = beforeAfterAttribute.GetType().Name }))
                                                cancelled = true;
                                        }
                                    }

                                    if (cancelled)
                                        return;
                                }

                                if (!cancelled)
                                {
                                    var parameterTypes = methodUnderTest.GetParameters().Select(p => p.ParameterType).ToArray();
                                    aggregator.Run(() =>
                                    {
                                        var result = methodUnderTest.Invoke(testClass, ConvertArguments(arguments ?? EmptyArray, parameterTypes));
                                        var task = result as Task;
                                        if (task != null)
                                            task.GetAwaiter().GetResult();
                                    });
                                }
                            });

                            foreach (var beforeAfterAttribute in beforeAttributesRun)
                            {
                                if (!messageSink.OnMessage(new AfterTestStarting { TestCase = this, TestDisplayName = displayName, AttributeName = beforeAfterAttribute.GetType().Name }))
                                    cancelled = true;

                                aggregator.Run(() => beforeAfterAttribute.After(methodUnderTest));

                                if (!messageSink.OnMessage(new AfterTestFinished { TestCase = this, TestDisplayName = displayName, AttributeName = beforeAfterAttribute.GetType().Name }))
                                    cancelled = true;
                            }
                        }

                        aggregator.Run(() =>
                        {
                            IDisposable disposable = testClass as IDisposable;
                            if (disposable != null)
                            {
                                if (!messageSink.OnMessage(new TestClassDisposeStarting { TestCase = this, TestDisplayName = displayName }))
                                    cancelled = true;

                                try
                                {
                                    disposable.Dispose();
                                }
                                finally
                                {
                                    if (!messageSink.OnMessage(new TestClassDisposeFinished { TestCase = this, TestDisplayName = displayName }))
                                        cancelled = true;
                                }
                            }
                        });
                    });

                    stopwatch.Stop();

                    if (!cancelled)
                    {
                        executionTime = (decimal)stopwatch.Elapsed.TotalSeconds;

                        var exception = aggregator.ToException();
                        var testResult = exception == null ? (TestResultMessage)new TestPassed() : new TestFailed(exception);
                        testResult.TestCase = this;
                        testResult.TestDisplayName = displayName;
                        testResult.ExecutionTime = executionTime;

                        if (!messageSink.OnMessage(testResult))
                            cancelled = true;
                    }
                }
            }

            if (!messageSink.OnMessage(new TestFinished { TestCase = this, TestDisplayName = displayName, ExecutionTime = executionTime }))
                cancelled = true;

            return cancelled;
        }
    }
}