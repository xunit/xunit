using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IXunitTestCase"/> that supports tests decorated with
    /// both <see cref="FactAttribute"/> and <see cref="TheoryAttribute"/>.
    /// </summary>
    public class XunitTestCase : LongLivedMarshalByRefObject, IXunitTestCase
    {
        readonly static object[] EmptyArray = new object[0];
        readonly static MethodInfo EnumerableCast = typeof(Enumerable).GetMethod("Cast");
        readonly static MethodInfo EnumerableToArray = typeof(Enumerable).GetMethod("ToArray");

        readonly IAssemblyInfo assembly;
        readonly IMethodInfo method;
        readonly ITypeInfo type;

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
            this.assembly = assembly;
            this.type = type;
            this.method = method;

            Arguments = arguments ?? EmptyArray;
            DisplayName = factAttribute.GetPropertyValue<string>("DisplayName") ?? type.Name + "." + method.Name;
            SkipReason = factAttribute.GetPropertyValue<string>("Skip");

            if (arguments != null)
            {
                IParameterInfo[] parameterInfos = method.GetParameters().ToArray();
                string[] displayValues = new string[Math.Max(arguments.Length, parameterInfos.Length)];
                int idx;

                for (idx = 0; idx < arguments.Length; idx++)
                    displayValues[idx] = ParameterToDisplayValue(GetParameterName(parameterInfos, idx), arguments[idx]);

                for (; idx < parameterInfos.Length; idx++)  // Fill-in any missing parameters with "???"
                    displayValues[idx] = GetParameterName(parameterInfos, idx) + ": ???";

                DisplayName = String.Format(CultureInfo.CurrentCulture, "{0}({1})", DisplayName, string.Join(", ", displayValues));
            }

            Traits = new Dictionary<string, string>();

            foreach (IAttributeInfo traitAttribute in method.GetCustomAttributes(typeof(TraitAttribute)))
                Traits.Add(traitAttribute.GetPropertyValue<string>("Name"), traitAttribute.GetPropertyValue<string>("Value"));
        }

        /// <inheritdoc/>
        public object[] Arguments { get; private set; }

        /// <inheritdoc/>
        public Type Class
        {
            get
            {
                var reflectionTypeInfo = type as IReflectionTypeInfo;
                if (reflectionTypeInfo != null)
                    return reflectionTypeInfo.Type;

                return null;
            }
        }

        /// <inheritdoc/>
        public string ClassName
        {
            get { return type.Name; }
        }

        /// <inheritdoc/>
        public string DisplayName { get; private set; }

        /// <inheritdoc/>
        public MethodInfo Method
        {
            get
            {
                var reflectionMethodInfo = method as IReflectionMethodInfo;
                if (reflectionMethodInfo != null)
                    return reflectionMethodInfo.MethodInfo;

                return null;
            }
        }

        /// <inheritdoc/>
        public string MethodName
        {
            get { return method.Name; }
        }

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

        object[] ConvertArguments(object[] args, Type[] types)
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

        static string GetParameterName(IParameterInfo[] parameters, int index)
        {
            if (index >= parameters.Length)
                return "???";

            return parameters[index].Name;
        }

        Type GetRuntimeClass()
        {
            return Reflector.GetType(type.Name, assembly.Name);
        }

        MethodInfo GetRuntimeMethod(Type type)
        {
            if (type == null)
                return null;

            return type.GetMethod(method.Name, method.GetBindingFlags());
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
            bool canceled = false;
            int totalFailed = 0;
            int totalRun = 0;
            int totalSkipped = 0;
            decimal executionTime = 0M;

            if (!messageSink.OnMessage(new TestCaseStarting { TestCase = this }))
                canceled = true;
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

                canceled = RunTests(delegatingSink);
            }

            if (!messageSink.OnMessage(new TestCaseFinished
            {
                ExecutionTime = executionTime,
                TestCase = this,
                TestsRun = totalRun,
                TestsFailed = totalFailed,
                TestsSkipped = totalSkipped
            }))
                canceled = true;

            return canceled;
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
        /// Run the tests in the test case.
        /// </summary>
        /// <param name="messageSink">The message sink to send results to.</param>
        protected virtual bool RunTests(IMessageSink messageSink)
        {
            var canceled = false;
            var classUnderTest = Class ?? GetRuntimeClass();
            var methodUnderTest = Method ?? GetRuntimeMethod(classUnderTest);
            decimal executionTime = 0M;

            if (!messageSink.OnMessage(new TestStarting { TestCase = this, TestDisplayName = DisplayName }))
                canceled = true;
            else
            {

                if (!String.IsNullOrEmpty(SkipReason))
                {
                    if (!messageSink.OnMessage(new TestSkipped { TestCase = this, TestDisplayName = DisplayName, Reason = SkipReason }))
                        canceled = true;
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
                            if (!messageSink.OnMessage(new TestClassConstructionStarting { TestCase = this, TestDisplayName = DisplayName }))
                                canceled = true;

                            try
                            {
                                if (!canceled)
                                    testClass = Activator.CreateInstance(classUnderTest);
                            }
                            finally
                            {
                                if (!messageSink.OnMessage(new TestClassConstructionFinished { TestCase = this, TestDisplayName = DisplayName }))
                                    canceled = true;
                            }
                        }

                        if (!canceled)
                        {
                            var beforeAfterAttributes = GetBeforeAfterAttributes(classUnderTest, methodUnderTest);

                            aggregator.Run(() =>
                            {
                                foreach (var beforeAfterAttribute in beforeAfterAttributes)
                                {
                                    if (!messageSink.OnMessage(new BeforeTestStarting { TestCase = this, TestDisplayName = DisplayName, AttributeName = beforeAfterAttribute.GetType().Name }))
                                        canceled = true;
                                    else
                                    {
                                        try
                                        {
                                            beforeAfterAttribute.Before(methodUnderTest);
                                            beforeAttributesRun.Push(beforeAfterAttribute);
                                        }
                                        finally
                                        {
                                            if (!messageSink.OnMessage(new BeforeTestFinished { TestCase = this, TestDisplayName = DisplayName, AttributeName = beforeAfterAttribute.GetType().Name }))
                                                canceled = true;
                                        }
                                    }

                                    if (canceled)
                                        return;
                                }

                                // REVIEW: This seems like the wrong level... test method should be at a higher scope that individual
                                // test method actions (like construction, before/after, etc.)
                                if (!messageSink.OnMessage(new TestMethodStarting { ClassName = ClassName, MethodName = MethodName }))
                                    canceled = true;

                                if (!canceled)
                                {
                                    var parameterTypes = methodUnderTest.GetParameters().Select(p => p.ParameterType).ToArray();
                                    aggregator.Run(() =>
                                    {
                                        var result = methodUnderTest.Invoke(testClass, ConvertArguments(Arguments, parameterTypes));
                                        var task = result as Task;
                                        if (task != null)
                                            task.GetAwaiter().GetResult();
                                    });
                                }

                                if (!messageSink.OnMessage(new TestMethodFinished { ClassName = ClassName, MethodName = MethodName }))
                                    canceled = true;
                            });

                            foreach (var beforeAfterAttribute in beforeAttributesRun)
                            {
                                if (!messageSink.OnMessage(new AfterTestStarting { TestCase = this, TestDisplayName = DisplayName, AttributeName = beforeAfterAttribute.GetType().Name }))
                                    canceled = true;

                                aggregator.Run(() => beforeAfterAttribute.After(methodUnderTest));

                                if (!messageSink.OnMessage(new AfterTestFinished { TestCase = this, TestDisplayName = DisplayName, AttributeName = beforeAfterAttribute.GetType().Name }))
                                    canceled = true;
                            }
                        }

                        aggregator.Run(() =>
                        {
                            IDisposable disposable = testClass as IDisposable;
                            if (disposable != null)
                            {
                                if (!messageSink.OnMessage(new TestClassDisposeStarting { TestCase = this, TestDisplayName = DisplayName }))
                                    canceled = true;

                                try
                                {
                                    disposable.Dispose();
                                }
                                finally
                                {
                                    if (!messageSink.OnMessage(new TestClassDisposeFinished { TestCase = this, TestDisplayName = DisplayName }))
                                        canceled = true;
                                }
                            }
                        });
                    });

                    stopwatch.Stop();

                    if (!canceled)
                    {
                        executionTime = (decimal)stopwatch.Elapsed.TotalSeconds;

                        var exception = aggregator.ToException();
                        var testResult = exception == null ? (TestResultMessage)new TestPassed() : new TestFailed(exception);
                        testResult.TestCase = this;
                        testResult.TestDisplayName = DisplayName;
                        testResult.ExecutionTime = executionTime;

                        if (!messageSink.OnMessage(testResult))
                            canceled = true;
                    }
                }
            }

            if (!messageSink.OnMessage(new TestFinished { TestCase = this, TestDisplayName = DisplayName, ExecutionTime = executionTime }))
                canceled = true;

            return canceled;
        }
    }
}