using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class XunitTestCase : LongLivedMarshalByRefObject, IXunitTestCase
    {
        readonly IAssemblyInfo assembly;
        readonly IMethodInfo method;
        readonly ITypeInfo type;

        public XunitTestCase(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute, IEnumerable<object> arguments = null)
        {
            this.assembly = assembly;
            this.type = type;
            this.method = method;

            Arguments = arguments ?? Enumerable.Empty<object>();
            DisplayName = factAttribute.GetPropertyValue<string>("DisplayName") ?? type.Name + "." + method.Name;
            SkipReason = factAttribute.GetPropertyValue<string>("Skip");

            if (arguments != null)
            {
                var Parameters = arguments.ToArray();

                IParameterInfo[] parameterInfos = method.GetParameters().ToArray();
                string[] displayValues = new string[Math.Max(Parameters.Length, parameterInfos.Length)];
                int idx;

                for (idx = 0; idx < Parameters.Length; idx++)
                    displayValues[idx] = ParameterToDisplayValue(GetParameterName(parameterInfos, idx), Parameters[idx]);

                for (; idx < parameterInfos.Length; idx++)  // Fill-in any missing parameters with "???"
                    displayValues[idx] = GetParameterName(parameterInfos, idx) + ": ???";

                DisplayName = String.Format(CultureInfo.CurrentCulture, "{0}({1})", DisplayName, string.Join(", ", displayValues));
            }

            Traits = new Dictionary<string, string>();

            foreach (IAttributeInfo traitAttribute in method.GetCustomAttributes(typeof(TraitAttribute)))
                Traits.Add(traitAttribute.GetPropertyValue<string>("Name"), traitAttribute.GetPropertyValue<string>("Value"));
        }

        public IEnumerable<object> Arguments { get; private set; }

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

        public string DisplayName { get; private set; }

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

        public string SkipReason { get; private set; }

        public int? SourceFileLine { get; internal set; }

        public string SourceFileName { get; internal set; }

        public ITestCollection TestCollection { get; private set; }

        public IDictionary<string, string> Traits { get; private set; }

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

        private Type GetRuntimeClass()
        {
            return Reflector.GetType(type.Name, assembly.Name);
        }

        private MethodInfo GetRuntimeMethod(Type type)
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
                    if (msg is ITestFinished)
                    {
                        totalRun++;
                        executionTime += ((ITestFinished)msg).ExecutionTime;
                    }
                    else if (msg is ITestFailed)
                        totalFailed++;
                    else if (msg is ITestSkipped)
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
            var cancelled = false;
            var classUnderTest = Class ?? GetRuntimeClass();
            var methodUnderTest = Method ?? GetRuntimeMethod(classUnderTest);
            decimal executionTime = 0M;

            if (!messageSink.OnMessage(new TestStarting { TestCase = this, TestDisplayName = DisplayName }))
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
                            if (!messageSink.OnMessage(new TestClassConstructionStarting { TestCase = this, TestDisplayName = DisplayName }))
                                cancelled = true;

                            try
                            {
                                if (!cancelled)
                                    testClass = Activator.CreateInstance(classUnderTest);
                            }
                            finally
                            {
                                if (!messageSink.OnMessage(new TestClassConstructionFinished { TestCase = this, TestDisplayName = DisplayName }))
                                    cancelled = true;
                            }
                        }

                        if (!cancelled)
                        {
                            IEnumerable<BeforeAfterTestAttribute> beforeAfterAttributes =
                                GetBeforeAfterAttributes(classUnderTest, methodUnderTest);

                            aggregator.Run(() =>
                            {
                                foreach (var beforeAfterAttribute in beforeAfterAttributes)
                                {
                                    if (!messageSink.OnMessage(new BeforeTestStarting { TestCase = this, TestDisplayName = DisplayName, AttributeName = beforeAfterAttribute.GetType().Name }))
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
                                            if (!messageSink.OnMessage(new BeforeTestFinished { TestCase = this, TestDisplayName = DisplayName, AttributeName = beforeAfterAttribute.GetType().Name }))
                                                cancelled = true;
                                        }
                                    }

                                    if (cancelled)
                                        return;
                                }

                                if (!messageSink.OnMessage(new TestMethodStarting { TestCase = this, TestDisplayName = DisplayName }))
                                    cancelled = true;

                                if (!cancelled)
                                    aggregator.Run(() => methodUnderTest.Invoke(testClass, Arguments.ToArray()));

                                if (!messageSink.OnMessage(new TestMethodFinished { TestCase = this, TestDisplayName = DisplayName }))
                                    cancelled = true;
                            });

                            foreach (var beforeAfterAttribute in beforeAttributesRun)
                            {
                                if (!messageSink.OnMessage(new AfterTestStarting { TestCase = this, TestDisplayName = DisplayName, AttributeName = beforeAfterAttribute.GetType().Name }))
                                    cancelled = true;

                                aggregator.Run(() => beforeAfterAttribute.After(methodUnderTest));

                                if (!messageSink.OnMessage(new AfterTestFinished { TestCase = this, TestDisplayName = DisplayName, AttributeName = beforeAfterAttribute.GetType().Name }))
                                    cancelled = true;
                            }
                        }

                        aggregator.Run(() =>
                        {
                            IDisposable disposable = testClass as IDisposable;
                            if (disposable != null)
                            {
                                if (!messageSink.OnMessage(new TestClassDisposeStarting { TestCase = this, TestDisplayName = DisplayName }))
                                    cancelled = true;

                                try
                                {
                                    disposable.Dispose();
                                }
                                finally
                                {
                                    if (!messageSink.OnMessage(new TestClassDisposeFinished { TestCase = this, TestDisplayName = DisplayName }))
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
                        testResult.TestDisplayName = DisplayName;
                        testResult.ExecutionTime = executionTime;

                        if (!messageSink.OnMessage(testResult))
                            cancelled = true;
                    }
                }
            }

            if (!messageSink.OnMessage(new TestFinished { TestCase = this, TestDisplayName = DisplayName, ExecutionTime = executionTime }))
                cancelled = true;

            return cancelled;
        }

        class ExceptionAggregator
        {
            List<Exception> exceptions = new List<Exception>();

            public void Run(Action code)
            {
                try
                {
                    code();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex.Unwrap());
                }
            }

            public Exception ToException()
            {
                if (exceptions.Count == 0)
                    return null;
                if (exceptions.Count == 1)
                    return exceptions[0];
                return new AggregateException(exceptions);
            }
        }
    }
}