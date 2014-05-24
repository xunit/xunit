using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public class Xunit1Tests
{
    public class Constructor
    {
        [Fact]
        public void UsesConstructorArgumentsToCreateExecutor()
        {
            var xunit1 = new TestableXunit1("AssemblyName.dll", "ConfigFile.config", shadowCopy: true);

            Assert.Equal("AssemblyName.dll", xunit1.Executor_TestAssemblyFileName);
            Assert.Equal("ConfigFile.config", xunit1.Executor_ConfigFileName);
            Assert.True(xunit1.Executor_ShadowCopy);
        }
    }

    public class Dispose
    {
        [Fact]
        public void DisposesExecutor()
        {
            var xunit1 = new TestableXunit1();

            xunit1.Dispose();

            xunit1.Executor.Received(1).Dispose();
        }
    }

    public class TestFrameworkDisplayName
    {
        [Fact]
        public void ReturnsDisplayNameFromExecutor()
        {
            var xunit1 = new TestableXunit1();
            xunit1.Executor.TestFrameworkDisplayName.Returns("Test Framework Display Name");

            var result = xunit1.TestFrameworkDisplayName;

            Assert.Equal("Test Framework Display Name", result);
        }
    }

    public class Find
    {
        [Fact]
        public void FindByAssemblyReturnsAllTestMethodsFromExecutorXml()
        {
            var xml = @"
<assembly>
    <class name='Type1'>
        <method name='Method1 Display Name' type='Type1' method='Method1'/>
    </class>
    <class name='SpecialType'>
        <method name='SpecialType.SkippedMethod' type='SpecialType' method='SkippedMethod' skip='I am not run'/>
        <method name='SpecialType.MethodWithTraits' type='SpecialType' method='MethodWithTraits'>
            <traits>
                <trait name='Trait1' value='Value1'/>
                <trait name='Trait2' value='Value2'/>
            </traits>
        </method>
    </class>
</assembly>";
            var xunit1 = new TestableXunit1();
            xunit1.Executor
                  .WhenForAnyArgs(x => x.EnumerateTests(null))
                  .Do(callInfo => callInfo.Arg<ICallbackEventHandler>().RaiseCallbackEvent(xml));
            var sink = new TestDiscoveryVisitor();

            xunit1.Find(false, sink);
            sink.Finished.WaitOne();

            Assert.Collection(sink.TestCases,
                testCase =>
                {
                    Assert.Equal("Type1", testCase.Class.Name);
                    Assert.Equal("Method1", testCase.Method.Name);
                    Assert.Equal("Method1 Display Name", testCase.DisplayName);
                    Assert.Null(testCase.SkipReason);
                    Assert.Empty(testCase.Traits);
                },
                testCase =>
                {
                    Assert.Equal("SpecialType", testCase.Class.Name);
                    Assert.Equal("SkippedMethod", testCase.Method.Name);
                    Assert.Equal("SpecialType.SkippedMethod", testCase.DisplayName);
                    Assert.Equal("I am not run", testCase.SkipReason);
                },
                testCase =>
                {
                    Assert.Equal("SpecialType", testCase.Class.Name);
                    Assert.Equal("MethodWithTraits", testCase.Method.Name);
                    Assert.Equal("SpecialType.MethodWithTraits", testCase.DisplayName);
                    Assert.Collection(testCase.Traits.Keys,
                        key =>
                        {
                            Assert.Equal("Trait1", key);
                            Assert.Collection(testCase.Traits[key],
                                value => Assert.Equal("Value1", value)
                            );
                        },
                        key =>
                        {
                            Assert.Equal("Trait2", key);
                            Assert.Collection(testCase.Traits[key],
                                value => Assert.Equal("Value2", value)
                            );
                        }
                    );
                }
            );
        }

        [Fact]
        public void FindByTypesReturnsOnlyMethodsInTheGivenType()
        {
            var xml = @"
<assembly>
    <class name='Type1'>
        <method name='Method1 Display Name' type='Type1' method='Method1'/>
    </class>
    <class name='Type2'>
        <method name='Type2.Method1' type='Type2' method='Method1'/>
        <method name='Type2.Method2' type='Type2' method='Method2'/>
    </class>
</assembly>";
            var xunit1 = new TestableXunit1();
            xunit1.Executor
                  .WhenForAnyArgs(x => x.EnumerateTests(null))
                  .Do(callInfo => callInfo.Arg<ICallbackEventHandler>().RaiseCallbackEvent(xml));
            var sink = new TestDiscoveryVisitor();

            xunit1.Find("Type2", false, sink);
            sink.Finished.WaitOne();

            Assert.Collection(sink.TestCases,
                testCase => Assert.Equal("Type2.Method1", testCase.DisplayName),
                testCase => Assert.Equal("Type2.Method2", testCase.DisplayName)
            );
        }

        [Fact]
        public void TestCasesUseInformationFromSourceInformationProvider()
        {
            var xml = @"
<assembly>
    <class name='Type2'>
        <method name='Type2.Method1' type='Type2' method='Method1'/>
        <method name='Type2.Method2' type='Type2' method='Method2'/>
    </class>
</assembly>";
            var xunit1 = new TestableXunit1();
            xunit1.Executor
                  .WhenForAnyArgs(x => x.EnumerateTests(null))
                  .Do(callInfo => callInfo.Arg<ICallbackEventHandler>().RaiseCallbackEvent(xml));
            xunit1.SourceInformationProvider
                  .GetSourceInformation(null)
                  .ReturnsForAnyArgs(callInfo => new SourceInformation { FileName = "File for " + callInfo.Arg<ITestCase>().DisplayName });
            var sink = new TestDiscoveryVisitor();

            xunit1.Find(true, sink);
            sink.Finished.WaitOne();

            Assert.Collection(sink.TestCases,
                testCase => Assert.Equal("File for Type2.Method1", testCase.SourceInformation.FileName),
                testCase => Assert.Equal("File for Type2.Method2", testCase.SourceInformation.FileName)
            );
        }
    }

    public class Run
    {
        [Fact]
        public void RunWithTestCases()
        {
            var testCollection = new Xunit1TestCollection("AssemblyName.dll");
            var testCases = new[] {
                new Xunit1TestCase("assembly", "type1", "passing", "type1.passing") { TestCollection = testCollection },
                new Xunit1TestCase("assembly", "type1", "failing", "type1.failing") { TestCollection = testCollection },
                new Xunit1TestCase("assembly", "type2", "skipping", "type2.skipping") { TestCollection = testCollection },
                new Xunit1TestCase("assembly", "type2", "skipping_with_start", "type2.skipping_with_start") { TestCollection = testCollection }
            };
            var xunit1 = new TestableXunit1("AssemblyName.dll", "ConfigFile.config");
            xunit1.Executor.TestFrameworkDisplayName.Returns("Test framework display name");
            xunit1.Executor
                  .When(x => x.RunTests("type1", Arg.Any<List<string>>(), Arg.Any<ICallbackEventHandler>()))
                  .Do(callInfo =>
                  {
                      var callback = callInfo.Arg<ICallbackEventHandler>();
                      callback.RaiseCallbackEvent("<start name='type1.passing' type='type1' method='passing'/>");
                      callback.RaiseCallbackEvent("<test name='type1.passing' type='type1' method='passing' result='Pass' time='1.000'/>");
                      callback.RaiseCallbackEvent("<start name='type1.failing' type='type1' method='failing'/>");
                      callback.RaiseCallbackEvent("<test name='type1.failing' type='type1' method='failing' result='Fail' time='0.234'><failure exception-type='Xunit.MockFailureException'><message>Failure message</message><stack-trace>Stack trace</stack-trace></failure></test>");
                      callback.RaiseCallbackEvent("<class name='type1' time='1.234' total='2' failed='1' skipped='0'/>");
                  });
            xunit1.Executor
                  .When(x => x.RunTests("type2", Arg.Any<List<string>>(), Arg.Any<ICallbackEventHandler>()))
                  .Do(callInfo =>
                  {
                      var callback = callInfo.Arg<ICallbackEventHandler>();
                      // Note. Skip does not send a start packet, unless you use a custom Fact
                      callback.RaiseCallbackEvent("<test name='type2.skipping' type='type2' method='skipping' result='Skip'><reason><message>Skip message</message></reason></test>");
                      callback.RaiseCallbackEvent("<start name='type2.skipping_with_start' type='type2' method='skipping_with_start'/>");
                      callback.RaiseCallbackEvent("<test name='type2.skipping_with_start' type='type2' method='skipping_with_start' result='Skip'><reason><message>Skip message</message></reason></test>");
                      callback.RaiseCallbackEvent("<class name='type2' time='0.000' total='1' failed='0' skipped='1'/>");
                  });
            var sink = new SpyMessageSink<ITestAssemblyFinished>();

            xunit1.Run(testCases, sink);
            sink.Finished.WaitOne();

            Assert.Collection(sink.Messages,
                message =>
                {
                    var assemblyStarting = Assert.IsAssignableFrom<ITestAssemblyStarting>(message);
                    Assert.Equal("AssemblyName.dll", assemblyStarting.AssemblyFileName);
                    Assert.Equal("ConfigFile.config", assemblyStarting.ConfigFileName);
                    Assert.Contains("-bit .NET ", assemblyStarting.TestEnvironment);
                    Assert.Equal("Test framework display name", assemblyStarting.TestFrameworkDisplayName);
                },
                message =>
                {
                    var testCollectionStarting = Assert.IsAssignableFrom<ITestCollectionStarting>(message);
                    Assert.Same(testCollection, testCollectionStarting.TestCollection);
                },
                message =>
                {
                    var testClassStarting = Assert.IsAssignableFrom<ITestClassStarting>(message);
                    Assert.Equal("type1", testClassStarting.ClassName);
                    Assert.Same(testCollection, testClassStarting.TestCollection);
                },
                message =>
                {
                    var testMethodStarting = Assert.IsAssignableFrom<ITestMethodStarting>(message);
                    Assert.Equal("type1", testMethodStarting.ClassName);
                    Assert.Equal("passing", testMethodStarting.MethodName);
                },
                message =>
                {
                    var testCaseStarting = Assert.IsAssignableFrom<ITestCaseStarting>(message);
                    Assert.Equal("type1.passing", testCaseStarting.TestCase.DisplayName);
                },
                message =>
                {
                    var testStarting = Assert.IsAssignableFrom<ITestStarting>(message);
                    Assert.Equal("type1.passing", testStarting.TestCase.DisplayName);
                    Assert.Equal("type1", testStarting.TestCase.Class.Name);
                    Assert.Equal("passing", testStarting.TestCase.Method.Name);
                    Assert.Same(testCollection, testStarting.TestCase.TestCollection);
                },
                message =>
                {
                    var testPassed = Assert.IsAssignableFrom<ITestPassed>(message);
                    Assert.Equal("type1.passing", testPassed.TestCase.DisplayName);
                    Assert.Equal(1M, testPassed.ExecutionTime);
                },
                message =>
                {
                    var testFinished = Assert.IsAssignableFrom<ITestFinished>(message);
                    Assert.Equal("type1.passing", testFinished.TestCase.DisplayName);
                },
                message =>
                {
                    var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(message);
                    Assert.Equal("type1.passing", testCaseFinished.TestCase.DisplayName);
                    Assert.Equal(1M, testCaseFinished.ExecutionTime);
                    Assert.Equal(0, testCaseFinished.TestsFailed);
                    Assert.Equal(1, testCaseFinished.TestsRun);
                    Assert.Equal(0, testCaseFinished.TestsSkipped);
                },
                message =>
                {
                    var testMethodFinished = Assert.IsAssignableFrom<ITestMethodFinished>(message);
                    Assert.Equal("type1", testMethodFinished.ClassName);
                    Assert.Equal("passing", testMethodFinished.MethodName);
                },
                message =>
                {
                    var testMethodStarting = Assert.IsAssignableFrom<ITestMethodStarting>(message);
                    Assert.Equal("type1", testMethodStarting.ClassName);
                    Assert.Equal("failing", testMethodStarting.MethodName);
                },
                message =>
                {
                    var testCaseStarting = Assert.IsAssignableFrom<ITestCaseStarting>(message);
                    Assert.Equal("type1.failing", testCaseStarting.TestCase.DisplayName);
                },
                message =>
                {
                    var testStarting = Assert.IsAssignableFrom<ITestStarting>(message);
                    Assert.Equal("type1.failing", testStarting.TestCase.DisplayName);
                    Assert.Equal("type1", testStarting.TestCase.Class.Name);
                    Assert.Equal("failing", testStarting.TestCase.Method.Name);
                    Assert.Same(testCollection, testStarting.TestCase.TestCollection);
                },
                message =>
                {
                    var testFailed = Assert.IsAssignableFrom<ITestFailed>(message);
                    Assert.Equal("type1.failing", testFailed.TestCase.DisplayName);
                    Assert.Equal(0.234M, testFailed.ExecutionTime);
                    Assert.Equal("Xunit.MockFailureException", testFailed.ExceptionTypes.Single());
                    Assert.Equal("Failure message", testFailed.Messages.Single());
                    Assert.Equal("Stack trace", testFailed.StackTraces.Single());
                },
                message =>
                {
                    var testFinished = Assert.IsAssignableFrom<ITestFinished>(message);
                    Assert.Equal("type1.failing", testFinished.TestCase.DisplayName);
                },
                message =>
                {
                    var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(message);
                    Assert.Equal("type1.failing", testCaseFinished.TestCase.DisplayName);
                    Assert.Equal(0.234M, testCaseFinished.ExecutionTime);
                    Assert.Equal(1, testCaseFinished.TestsFailed);
                    Assert.Equal(1, testCaseFinished.TestsRun);
                    Assert.Equal(0, testCaseFinished.TestsSkipped);
                },
                message =>
                {
                    var testMethodFinished = Assert.IsAssignableFrom<ITestMethodFinished>(message);
                    Assert.Equal("type1", testMethodFinished.ClassName);
                    Assert.Equal("failing", testMethodFinished.MethodName);
                },
                message =>
                {
                    var testClassFinished = Assert.IsAssignableFrom<ITestClassFinished>(message);
                    Assert.Equal("type1", testClassFinished.ClassName);
                    Assert.Equal(1.234M, testClassFinished.ExecutionTime);
                    Assert.Equal(1, testClassFinished.TestsFailed);
                    Assert.Equal(2, testClassFinished.TestsRun);
                    Assert.Equal(0, testClassFinished.TestsSkipped);
                },
                message =>
                {
                    var testClassStarting = Assert.IsAssignableFrom<ITestClassStarting>(message);
                    Assert.Equal("type2", testClassStarting.ClassName);
                    Assert.Same(testCollection, testClassStarting.TestCollection);
                },
                message =>
                {
                    var testMethodStarting = Assert.IsAssignableFrom<ITestMethodStarting>(message);
                    Assert.Equal("type2", testMethodStarting.ClassName);
                    Assert.Equal("skipping", testMethodStarting.MethodName);
                },
                message =>
                {
                    var testCaseStarting = Assert.IsAssignableFrom<ITestCaseStarting>(message);
                    Assert.Equal("type2.skipping", testCaseStarting.TestCase.DisplayName);
                },
                message =>
                {
                    var testStarting = Assert.IsAssignableFrom<ITestStarting>(message);
                    Assert.Equal("type2.skipping", testStarting.TestCase.DisplayName);
                    Assert.Equal("type2", testStarting.TestCase.Class.Name);
                    Assert.Equal("skipping", testStarting.TestCase.Method.Name);
                    Assert.Same(testCollection, testStarting.TestCase.TestCollection);
                },
                message =>
                {
                    var testSkipped = Assert.IsAssignableFrom<ITestSkipped>(message);
                    Assert.Equal("type2.skipping", testSkipped.TestCase.DisplayName);
                    Assert.Equal(0M, testSkipped.ExecutionTime);
                    Assert.Equal("Skip message", testSkipped.Reason);
                },
                message =>
                {
                    var testFinished = Assert.IsAssignableFrom<ITestFinished>(message);
                    Assert.Equal("type2.skipping", testFinished.TestCase.DisplayName);
                },
                message =>
                {
                    var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(message);
                    Assert.Equal("type2.skipping", testCaseFinished.TestCase.DisplayName);
                    Assert.Equal(0M, testCaseFinished.ExecutionTime);
                    Assert.Equal(0, testCaseFinished.TestsFailed);
                    Assert.Equal(1, testCaseFinished.TestsRun);
                    Assert.Equal(1, testCaseFinished.TestsSkipped);
                },
                message =>
                {
                    var testMethodFinished = Assert.IsAssignableFrom<ITestMethodFinished>(message);
                    Assert.Equal("type2", testMethodFinished.ClassName);
                    Assert.Equal("skipping", testMethodFinished.MethodName);
                },
                message =>
                {
                    var testMethodStarting = Assert.IsAssignableFrom<ITestMethodStarting>(message);
                    Assert.Equal("type2", testMethodStarting.ClassName);
                    Assert.Equal("skipping_with_start", testMethodStarting.MethodName);
                },
                message =>
                {
                    var testCaseStarting = Assert.IsAssignableFrom<ITestCaseStarting>(message);
                    Assert.Equal("type2.skipping_with_start", testCaseStarting.TestCase.DisplayName);
                },
                message =>
                {
                    var testStarting = Assert.IsAssignableFrom<ITestStarting>(message);
                    Assert.Equal("type2.skipping_with_start", testStarting.TestCase.DisplayName);
                    Assert.Equal("type2", testStarting.TestCase.Class.Name);
                    Assert.Equal("skipping_with_start", testStarting.TestCase.Method.Name);
                    Assert.Same(testCollection, testStarting.TestCase.TestCollection);
                },
                message =>
                {
                    var testSkipped = Assert.IsAssignableFrom<ITestSkipped>(message);
                    Assert.Equal("type2.skipping_with_start", testSkipped.TestCase.DisplayName);
                    Assert.Equal(0M, testSkipped.ExecutionTime);
                    Assert.Equal("Skip message", testSkipped.Reason);
                },
                message =>
                {
                    var testFinished = Assert.IsAssignableFrom<ITestFinished>(message);
                    Assert.Equal("type2.skipping_with_start", testFinished.TestCase.DisplayName);
                },
                message =>
                {
                    var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(message);
                    Assert.Equal("type2.skipping_with_start", testCaseFinished.TestCase.DisplayName);
                    Assert.Equal(0M, testCaseFinished.ExecutionTime);
                    Assert.Equal(0, testCaseFinished.TestsFailed);
                    Assert.Equal(1, testCaseFinished.TestsRun);
                    Assert.Equal(1, testCaseFinished.TestsSkipped);
                },
                message =>
                {
                    var testMethodFinished = Assert.IsAssignableFrom<ITestMethodFinished>(message);
                    Assert.Equal("type2", testMethodFinished.ClassName);
                    Assert.Equal("skipping_with_start", testMethodFinished.MethodName);
                },
                message =>
                {
                    var testClassFinished = Assert.IsAssignableFrom<ITestClassFinished>(message);
                    Assert.Equal("type2", testClassFinished.ClassName);
                    Assert.Equal(0M, testClassFinished.ExecutionTime);
                    Assert.Equal(0, testClassFinished.TestsFailed);
                    Assert.Equal(1, testClassFinished.TestsRun);
                    Assert.Equal(1, testClassFinished.TestsSkipped);
                },
                message =>
                {
                    var testCollectionFinished = Assert.IsAssignableFrom<ITestCollectionFinished>(message);
                    Assert.Equal(1.234M, testCollectionFinished.ExecutionTime);
                    Assert.Equal(1, testCollectionFinished.TestsFailed);
                    Assert.Equal(3, testCollectionFinished.TestsRun);
                    Assert.Equal(1, testCollectionFinished.TestsSkipped);
                    Assert.Same(testCollection, testCollectionFinished.TestCollection);
                },
                message =>
                {
                    var assemblyFinished = Assert.IsAssignableFrom<ITestAssemblyFinished>(message);
                    Assert.Equal(1.234M, assemblyFinished.ExecutionTime);
                    Assert.Equal(1, assemblyFinished.TestsFailed);
                    Assert.Equal(3, assemblyFinished.TestsRun);
                    Assert.Equal(1, assemblyFinished.TestsSkipped);
                }
            );
        }

        [Fact]
        public void ExceptionThrownDuringRunTests_ResultsInErrorMessage()
        {
            var testCollection = new Xunit1TestCollection("AssemblyName.dll");
            var testCases = new[] {
                new Xunit1TestCase("assembly", "type1", "passing", "type1.passing") { TestCollection = testCollection }
            };
            var exception = new DivideByZeroException();
            var xunit1 = new TestableXunit1("AssemblyName.dll", "ConfigFile.config");
            xunit1.Executor.TestFrameworkDisplayName.Returns("Test framework display name");
            xunit1.Executor
                  .WhenForAnyArgs(x => x.RunTests(null, null, null))
                  .Do(callInfo => { throw exception; });
            var sink = new SpyMessageSink<ITestAssemblyFinished>();

            xunit1.Run(testCases, sink);
            sink.Finished.WaitOne();

            var errorMessage = Assert.Single(sink.Messages.OfType<IErrorMessage>());
            Assert.Equal("System.DivideByZeroException", errorMessage.ExceptionTypes.Single());
            Assert.Equal("Attempted to divide by zero.", errorMessage.Messages.Single());
            Assert.Equal(exception.StackTrace, errorMessage.StackTraces.Single());
        }

        [Fact]
        public void NestedExceptionsThrownDuringRunTests_ResultsInErrorMessage()
        {
            var testCollection = new Xunit1TestCollection("AssemblyName.dll");
            var testCases = new[] {
                new Xunit1TestCase("assembly", "type1", "passing", "type1.passing") { TestCollection = testCollection }
            };
            var exception = GetNestedExceptions();
            var xunit1 = new TestableXunit1("AssemblyName.dll", "ConfigFile.config");
            xunit1.Executor.TestFrameworkDisplayName.Returns("Test framework display name");
            xunit1.Executor
                  .WhenForAnyArgs(x => x.RunTests(null, null, null))
                  .Do(callInfo => { throw exception; });
            var sink = new SpyMessageSink<ITestAssemblyFinished>();

            xunit1.Run(testCases, sink);
            sink.Finished.WaitOne();

            var errorMessage = Assert.Single(sink.Messages.OfType<IErrorMessage>());
            Assert.Equal(exception.GetType().FullName, errorMessage.ExceptionTypes[0]);
            Assert.Equal(exception.InnerException.GetType().FullName, errorMessage.ExceptionTypes[1]);
            Assert.Equal(exception.Message, errorMessage.Messages[0]);
            Assert.Equal(exception.InnerException.Message, errorMessage.Messages[1]);
            Assert.Equal(exception.StackTrace, errorMessage.StackTraces[0]);
            Assert.Equal(exception.InnerException.StackTrace, errorMessage.StackTraces[1]);
        }

        [Fact]
        public void NestedExceptionResultFromTests_ResultsInErrorMessage()
        {
            var testCollection = new Xunit1TestCollection("AssemblyName.dll");
            var testCases = new[] {
                new Xunit1TestCase("assembly", "type1", "failing", "type1.failing") { TestCollection = testCollection }
            };
            var exception = GetNestedExceptions();
            var xunit1 = new TestableXunit1("AssemblyName.dll", "ConfigFile.config");
            xunit1.Executor.TestFrameworkDisplayName.Returns("Test framework display name");
            xunit1.Executor
                  .WhenForAnyArgs(x => x.RunTests(null, null, null))
                  .Do(callInfo =>
                  {
                      var callback = callInfo.Arg<ICallbackEventHandler>();
                      callback.RaiseCallbackEvent("<start name='type1.failing' type='type1' method='failing'/>");
                      callback.RaiseCallbackEvent(string.Format("<test name='type1.failing' type='type1' method='failing' result='Fail' time='0.234'><failure exception-type='{0}'><message>{1}</message><stack-trace><![CDATA[{2}]]></stack-trace></failure></test>", exception.GetType().FullName, GetMessage(exception), GetStackTrace(exception)));
                      callback.RaiseCallbackEvent("<class name='type1' time='1.234' total='1' failed='1' skipped='0'/>");
                  });
            var sink = new SpyMessageSink<ITestAssemblyFinished>();

            xunit1.Run(testCases, sink);
            sink.Finished.WaitOne();

            var testFailed = Assert.Single(sink.Messages.OfType<ITestFailed>());
            Assert.Equal(exception.GetType().FullName, testFailed.ExceptionTypes[0]);
            Assert.Equal(exception.InnerException.GetType().FullName, testFailed.ExceptionTypes[1]);
            Assert.Equal(exception.Message, testFailed.Messages[0]);
            Assert.Equal(exception.InnerException.Message, testFailed.Messages[1]);
            Assert.Equal(exception.StackTrace, testFailed.StackTraces[0]);
            Assert.Equal(exception.InnerException.StackTrace, testFailed.StackTraces[1]);
        }

        [Fact]
        public void ExceptionThrownDuringClassStart_ResultsInErrorMessage()
        {
            var testCollection = new Xunit1TestCollection("AssemblyName.dll");
            var testCases = new[] {
                new Xunit1TestCase("assembly", "type1", "failingclass", "type1.failingclass") { TestCollection = testCollection }
            };
            var exception = new InvalidOperationException("Cannot use a test class as its own fixture data");
            var xunit1 = new TestableXunit1("AssemblyName.dll", "ConfigFile.config");
            xunit1.Executor.TestFrameworkDisplayName.Returns("Test framework display name");
            xunit1.Executor
                  .When(x => x.RunTests("type1", Arg.Any<List<string>>(),
                    Arg.Any<ICallbackEventHandler>()))
                  .Do(callInfo =>
                  {
                      // Ensure the exception has a callstack
                      try { throw exception; } catch{}
                      var callback = callInfo.Arg<ICallbackEventHandler>();
                      callback.RaiseCallbackEvent(string.Format("<class name='type1' time='0.000' total='0' passed='0' failed='1' skipped='0'><failure exception-type='System.InvalidOperationException'><message>Cannot use a test class as its own fixture data</message><stack-trace><![CDATA[{0}]]></stack-trace></failure></class>", exception.StackTrace));
                  });
            var sink = new SpyMessageSink<ITestAssemblyFinished>();

            xunit1.Run(testCases, sink);
            sink.Finished.WaitOne();

            var errorMessage = Assert.Single(sink.Messages.OfType<IErrorMessage>());
            Assert.Equal("System.InvalidOperationException", errorMessage.ExceptionTypes.Single());
            Assert.Equal("Cannot use a test class as its own fixture data", errorMessage.Messages.Single());
            Assert.Equal(exception.StackTrace, errorMessage.StackTraces.Single());
        }

        [Fact]
        public void ExceptionThrownDuringClassFinish_ResultsInErrorMessage()
        {
            var testCollection = new Xunit1TestCollection("AssemblyName.dll");
            var testCases = new[] {
                new Xunit1TestCase("assembly", "failingtype", "passingmethod", "failingtype.passingmethod") { TestCollection = testCollection }
            };
            var exception = new InvalidOperationException("Cannot use a test class as its own fixture data");
            var xunit1 = new TestableXunit1("AssemblyName.dll", "ConfigFile.config");
            xunit1.Executor.TestFrameworkDisplayName.Returns("Test framework display name");
            xunit1.Executor
                  .When(x => x.RunTests("failingtype", Arg.Any<List<string>>(),
                    Arg.Any<ICallbackEventHandler>()))
                  .Do(callInfo =>
                  {
                      // Ensure the exception has a callstack
                      try { throw exception; } catch{}
                      var callback = callInfo.Arg<ICallbackEventHandler>();
                      callback.RaiseCallbackEvent("<start name='failingtype.passingmethod' type='failingtype' method='passingmethod'/>");
                      callback.RaiseCallbackEvent("<test name='failingtype.passingmethod' type='failingtype' method='passingmethod' result='Pass' time='1.000'/>");
                      callback.RaiseCallbackEvent(string.Format("<class name='failingtype' time='0.000' total='0' passed='1' failed='1' skipped='0'><failure exception-type='Xunit.Some.Exception'><message>Cannot use a test class as its own fixture data</message><stack-trace><![CDATA[{0}]]></stack-trace></failure></class>", exception.StackTrace));
                  });
            var sink = new SpyMessageSink<ITestAssemblyFinished>();

            xunit1.Run(testCases, sink);
            sink.Finished.WaitOne();

            var errorMessage = Assert.Single(sink.Messages.OfType<IErrorMessage>());
            Assert.Equal("Xunit.Some.Exception", errorMessage.ExceptionTypes.Single());
            Assert.Equal("Cannot use a test class as its own fixture data", errorMessage.Messages.Single());
            Assert.Equal(exception.StackTrace, errorMessage.StackTraces.Single());
        }

        [Fact]
        public void NestedExceptionsThrownDuringClassStart_ResultsInErrorMessage()
        {
            var testCollection = new Xunit1TestCollection("AssemblyName.dll");
            var testCases = new[] {
                new Xunit1TestCase("assembly", "failingtype", "passingmethod", "failingtype.passingmethod") { TestCollection = testCollection }
            };
            var exception = GetNestedExceptions();
            var xunit1 = new TestableXunit1("AssemblyName.dll", "ConfigFile.config");
            xunit1.Executor.TestFrameworkDisplayName.Returns("Test framework display name");
            xunit1.Executor
                  .When(x => x.RunTests("failingtype", Arg.Any<List<string>>(),
                    Arg.Any<ICallbackEventHandler>()))
                  .Do(callInfo =>
                  {
                      var callback = callInfo.Arg<ICallbackEventHandler>();
                      callback.RaiseCallbackEvent("<start name='failingtype.passingmethod' type='failingtype' method='passingmethod'/>");
                      callback.RaiseCallbackEvent("<test name='failingtype.passingmethod' type='failingtype' method='passingmethod' result='Pass' time='1.000'/>");
                      callback.RaiseCallbackEvent(string.Format("<class name='failingtype' time='0.000' total='0' passed='1' failed='1' skipped='0'><failure exception-type='System.InvalidOperationException'><message>{0}</message><stack-trace><![CDATA[{1}]]></stack-trace></failure></class>", GetMessage(exception), GetStackTrace(exception)));
                  });
            var sink = new SpyMessageSink<ITestAssemblyFinished>();

            xunit1.Run(testCases, sink);
            sink.Finished.WaitOne();

            var errorMessage = Assert.Single(sink.Messages.OfType<IErrorMessage>());
            Assert.Equal(exception.GetType().FullName, errorMessage.ExceptionTypes[0]);
            Assert.Equal(exception.InnerException.GetType().FullName, errorMessage.ExceptionTypes[1]);
            Assert.Equal(exception.Message, errorMessage.Messages[0]);
            Assert.Equal(exception.InnerException.Message, errorMessage.Messages[1]);
            Assert.Equal(exception.StackTrace, errorMessage.StackTraces[0]);
            Assert.Equal(exception.InnerException.StackTrace, errorMessage.StackTraces[1]);
        }

        private Exception GetNestedExceptions()
        {
            try
            {
                ThrowOuterException();
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        private void ThrowOuterException()
        {
            try
            {
                ThrowInnerException();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Message from outer exception", e);
            }
        }

        private void ThrowInnerException()
        {
            throw new DivideByZeroException();
        }

        // From xunit1's ExceptionUtility
        private static string GetMessage(Exception ex)
        {
            return GetMessage(ex, 0);
        }

        static string GetMessage(Exception ex, int level)
        {
            string result = "";

            if (level > 0)
            {
                for (int idx = 0; idx < level; idx++)
                    result += "----";

                result += " ";
            }

            // We won't have any AssertExceptions - we're testing failure in class start/finish
            //if (!(ex is AssertException))
                result += ex.GetType().FullName + " : ";

            result += ex.Message;

            if (ex.InnerException != null)
                result = result + Environment.NewLine + GetMessage(ex.InnerException, level + 1);

            return result;
        }

        const string RETHROW_MARKER = "$$RethrowMarker$$";
        
        private static string GetStackTrace(Exception ex)
        {
            if (ex == null)
                return "";

            string result = ex.StackTrace;

            if (result != null)
            {
                int idx = result.IndexOf(RETHROW_MARKER);
                if (idx >= 0)
                    result = result.Substring(0, idx);
            }

            if (ex.InnerException != null)
                result = result + Environment.NewLine +
                         "----- Inner Stack Trace -----" + Environment.NewLine +
                         GetStackTrace(ex.InnerException);

            return result;
        }
    }

    class TestableXunit1 : Xunit1
    {
        public readonly IXunit1Executor Executor = Substitute.For<IXunit1Executor>();
        public string Executor_TestAssemblyFileName;
        public string Executor_ConfigFileName;
        public bool Executor_ShadowCopy;
        public readonly ISourceInformationProvider SourceInformationProvider;

        public TestableXunit1(string assemblyFileName = null, string configFileName = null, bool shadowCopy = true)
            : this(assemblyFileName ?? @"C:\Path\Assembly.dll", configFileName, shadowCopy, Substitute.For<ISourceInformationProvider>())
        {
        }

        TestableXunit1(string assemblyFileName, string configFileName, bool shadowCopy, ISourceInformationProvider sourceInformationProvider)
            : base(sourceInformationProvider, assemblyFileName, configFileName, shadowCopy)
        {
            SourceInformationProvider = sourceInformationProvider;
        }

        protected override IXunit1Executor CreateExecutor(string testAssemblyFileName, string configFileName, bool shadowCopy)
        {
            Executor_TestAssemblyFileName = testAssemblyFileName;
            Executor_ConfigFileName = configFileName;
            Executor_ShadowCopy = shadowCopy;

            return Executor;
        }
    }
}