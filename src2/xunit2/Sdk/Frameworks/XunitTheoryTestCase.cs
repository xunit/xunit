//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading.Tasks;
//using Xunit.Abstractions;

//namespace Xunit.Sdk
//{
//    /// <summary>
//    /// Represents a test case which runs multiple tests for theory data, either because the
//    /// data was not enumerable or because the data was not serializable.
//    /// </summary>
//    public class XunitTheoryTestCase : XunitTestCase
//    {
//        public XunitTheoryTestCase(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute)
//            : base(assembly, type, method, factAttribute) { }

//        protected override bool RunTests(IMessageSink messageSink)
//        {
//            var canceled = false;
//            var classUnderTest = GetRuntimeClass();
//            var methodUnderTest = GetRuntimeMethod(classUnderTest);
//            decimal executionTime = 0M;

//            if (!messageSink.OnMessage(new TestStarting { TestCase = this, TestDisplayName = DisplayName }))
//                canceled = true;
//            else
//            {
//                if (!String.IsNullOrEmpty(SkipReason))
//                {
//                    if (!messageSink.OnMessage(new TestSkipped { TestCase = this, TestDisplayName = DisplayName, Reason = SkipReason }))
//                        canceled = true;
//                }
//                else
//                {
//                    var aggregator = new ExceptionAggregator();
//                    var beforeAttributesRun = new Stack<BeforeAfterTestAttribute>();
//                    var stopwatch = Stopwatch.StartNew();

//                    aggregator.Run(() =>
//                    {
//                        object testClass = null;

//                        if (!methodUnderTest.IsStatic)
//                        {
//                            if (!messageSink.OnMessage(new TestClassConstructionStarting { TestCase = this, TestDisplayName = DisplayName }))
//                                canceled = true;

//                            try
//                            {
//                                if (!canceled)
//                                    testClass = Activator.CreateInstance(classUnderTest);
//                            }
//                            finally
//                            {
//                                if (!messageSink.OnMessage(new TestClassConstructionFinished { TestCase = this, TestDisplayName = DisplayName }))
//                                    canceled = true;
//                            }
//                        }

//                        if (!canceled)
//                        {
//                            var beforeAfterAttributes = GetBeforeAfterAttributes(classUnderTest, methodUnderTest);

//                            aggregator.Run(() =>
//                            {
//                                foreach (var beforeAfterAttribute in beforeAfterAttributes)
//                                {
//                                    if (!messageSink.OnMessage(new BeforeTestStarting { TestCase = this, TestDisplayName = DisplayName, AttributeName = beforeAfterAttribute.GetType().Name }))
//                                        canceled = true;
//                                    else
//                                    {
//                                        try
//                                        {
//                                            beforeAfterAttribute.Before(methodUnderTest);
//                                            beforeAttributesRun.Push(beforeAfterAttribute);
//                                        }
//                                        finally
//                                        {
//                                            if (!messageSink.OnMessage(new BeforeTestFinished { TestCase = this, TestDisplayName = DisplayName, AttributeName = beforeAfterAttribute.GetType().Name }))
//                                                canceled = true;
//                                        }
//                                    }

//                                    if (canceled)
//                                        return;
//                                }

//                                if (!canceled)
//                                {
//                                    var parameterTypes = methodUnderTest.GetParameters().Select(p => p.ParameterType).ToArray();
//                                    aggregator.Run(() =>
//                                    {
//                                        var result = methodUnderTest.Invoke(testClass, ConvertArguments(Arguments, parameterTypes));
//                                        var task = result as Task;
//                                        if (task != null)
//                                            task.GetAwaiter().GetResult();
//                                    });
//                                }

//                                if (!messageSink.OnMessage(new TestMethodFinished { ClassName = ClassName, MethodName = MethodName }))
//                                    canceled = true;
//                            });

//                            foreach (var beforeAfterAttribute in beforeAttributesRun)
//                            {
//                                if (!messageSink.OnMessage(new AfterTestStarting { TestCase = this, TestDisplayName = DisplayName, AttributeName = beforeAfterAttribute.GetType().Name }))
//                                    canceled = true;

//                                aggregator.Run(() => beforeAfterAttribute.After(methodUnderTest));

//                                if (!messageSink.OnMessage(new AfterTestFinished { TestCase = this, TestDisplayName = DisplayName, AttributeName = beforeAfterAttribute.GetType().Name }))
//                                    canceled = true;
//                            }
//                        }

//                        aggregator.Run(() =>
//                        {
//                            IDisposable disposable = testClass as IDisposable;
//                            if (disposable != null)
//                            {
//                                if (!messageSink.OnMessage(new TestClassDisposeStarting { TestCase = this, TestDisplayName = DisplayName }))
//                                    canceled = true;

//                                try
//                                {
//                                    disposable.Dispose();
//                                }
//                                finally
//                                {
//                                    if (!messageSink.OnMessage(new TestClassDisposeFinished { TestCase = this, TestDisplayName = DisplayName }))
//                                        canceled = true;
//                                }
//                            }
//                        });
//                    });

//                    stopwatch.Stop();

//                    if (!canceled)
//                    {
//                        executionTime = (decimal)stopwatch.Elapsed.TotalSeconds;

//                        var exception = aggregator.ToException();
//                        var testResult = exception == null ? (TestResultMessage)new TestPassed() : new TestFailed(exception);
//                        testResult.TestCase = this;
//                        testResult.TestDisplayName = DisplayName;
//                        testResult.ExecutionTime = executionTime;

//                        if (!messageSink.OnMessage(testResult))
//                            canceled = true;
//                    }
//                }
//            }

//            if (!messageSink.OnMessage(new TestFinished { TestCase = this, TestDisplayName = DisplayName, ExecutionTime = executionTime }))
//                canceled = true;

//            return canceled;
//        }
//    }
//}