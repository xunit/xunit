﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class XunitTestAssemblyRunnerTests
{
    public class GetTestFrameworkDisplayName
    {
        [Fact]
        public static void IsXunit2()
        {
            var runner = TestableXunitTestAssemblyRunner.Create();

            var result = runner.GetTestFrameworkDisplayName();

            Assert.StartsWith("xUnit.net 2.", result);
        }
    }

    public class GetTestFrameworkEnvironment
    {
        [Fact]
        public static void Default()
        {
            var runner = TestableXunitTestAssemblyRunner.Create();

            var result = runner.GetTestFrameworkEnvironment();

            Assert.EndsWith("[collection-per-class, parallel]", result);
        }

        [Fact]
        public static void Attribute_NonParallel()
        {
            var attribute = Mocks.CollectionBehaviorAttribute(disableTestParallelization: true);
            var assembly = Mocks.TestAssembly(attributes: new[] { attribute });
            var runner = TestableXunitTestAssemblyRunner.Create(assembly: assembly);

            var result = runner.GetTestFrameworkEnvironment();

            Assert.EndsWith("[collection-per-class, non-parallel]", result);
        }

        [Fact]
        public static void Attribute_MaxThreads()
        {
            var attribute = Mocks.CollectionBehaviorAttribute(maxParallelThreads: 255);
            var assembly = Mocks.TestAssembly(attributes: new[] { attribute });
            var runner = TestableXunitTestAssemblyRunner.Create(assembly: assembly);

            var result = runner.GetTestFrameworkEnvironment();

            Assert.EndsWith("[collection-per-class, parallel (255 threads)]", result);
        }

        [Theory]
        [InlineData(CollectionBehavior.CollectionPerAssembly, "collection-per-assembly")]
        [InlineData(CollectionBehavior.CollectionPerClass, "collection-per-class")]
        public static void Attribute_CollectionBehavior(CollectionBehavior behavior, string expectedDisplayText)
        {
            var attribute = Mocks.CollectionBehaviorAttribute(behavior, disableTestParallelization: true);
            var assembly = Mocks.TestAssembly(attributes: new[] { attribute });
            var runner = TestableXunitTestAssemblyRunner.Create(assembly: assembly);

            var result = runner.GetTestFrameworkEnvironment();

            Assert.EndsWith(String.Format("[{0}, non-parallel]", expectedDisplayText), result);
        }

        [Fact]
        public static void Attribute_CustomCollectionFactory()
        {
            var factoryType = typeof(MyTestCollectionFactory);
            var attr = Mocks.CollectionBehaviorAttribute(factoryType.FullName, factoryType.Assembly.FullName, disableTestParallelization: true);
            var assembly = Mocks.TestAssembly(attributes: new[] { attr });
            var runner = TestableXunitTestAssemblyRunner.Create(assembly: assembly);

            var result = runner.GetTestFrameworkEnvironment();

            Assert.EndsWith("[My Factory, non-parallel]", result);
        }

        class MyTestCollectionFactory : IXunitTestCollectionFactory
        {
            public string DisplayName { get { return "My Factory"; } }

            public MyTestCollectionFactory(ITestAssembly assembly) { }

            public ITestCollection Get(ITypeInfo testClass)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public static void TestOptions_NonParallel()
        {
            var options = new XunitExecutionOptions { DisableParallelization = true };
            var runner = TestableXunitTestAssemblyRunner.Create(options: options);

            var result = runner.GetTestFrameworkEnvironment();

            Assert.EndsWith("[collection-per-class, non-parallel]", result);
        }

        [Fact]
        public static void TestOptions_MaxThreads()
        {
            var options = new XunitExecutionOptions { MaxParallelThreads = 255 };
            var runner = TestableXunitTestAssemblyRunner.Create(options: options);

            var result = runner.GetTestFrameworkEnvironment();

            Assert.EndsWith("[collection-per-class, parallel (255 threads)]", result);
        }

        [Fact]
        public static void TestOptionsOverrideAttribute()
        {
            var attribute = Mocks.CollectionBehaviorAttribute(disableTestParallelization: true, maxParallelThreads: 127);
            var options = new XunitExecutionOptions { DisableParallelization = false, MaxParallelThreads = 255 };
            var assembly = Mocks.TestAssembly(attributes: new[] { attribute });
            var runner = TestableXunitTestAssemblyRunner.Create(assembly: assembly, options: options);

            var result = runner.GetTestFrameworkEnvironment();

            Assert.EndsWith("[collection-per-class, parallel (255 threads)]", result);
        }
    }

    public class RunAsync
    {
        [Fact]
        public static async void Parallel_MultipleThreads()
        {
            var passing = Mocks.XunitTestCase<ClassUnderTest>("Passing");
            var other = Mocks.XunitTestCase<ClassUnderTest>("Other");
            var options = new XunitExecutionOptions { MaxParallelThreads = 2 };
            var runner = TestableXunitTestAssemblyRunner.Create(testCases: new[] { passing, other }, options: options);

            await runner.RunAsync();

            var threadIDs = runner.TestCasesRun.Select(x => x.Item1).ToList();
            Assert.NotEqual(threadIDs[0], threadIDs[1]);
        }

        [Fact]
        public static async void Parallel_SingleThread()
        {
            var passing = Mocks.XunitTestCase<ClassUnderTest>("Passing");
            var other = Mocks.XunitTestCase<ClassUnderTest>("Other");
            var options = new XunitExecutionOptions { MaxParallelThreads = 1 };
            var runner = TestableXunitTestAssemblyRunner.Create(testCases: new[] { passing, other }, options: options);

            await runner.RunAsync();

            var threadIDs = runner.TestCasesRun.Select(x => x.Item1).ToList();
            Assert.Equal(threadIDs[0], threadIDs[1]);
        }

        [Fact]
        public static async void NonParallel()
        {
            var passing = Mocks.XunitTestCase<ClassUnderTest>("Passing");
            var other = Mocks.XunitTestCase<ClassUnderTest>("Other");
            var options = new XunitExecutionOptions { DisableParallelization = true };
            var runner = TestableXunitTestAssemblyRunner.Create(testCases: new[] { passing, other }, options: options);

            await runner.RunAsync();

            var threadIDs = runner.TestCasesRun.Select(x => x.Item1).ToList();
            Assert.Equal(threadIDs[0], threadIDs[1]);
        }
    }

    class ClassUnderTest
    {
        [Fact]
        public void Passing() { }

        [Fact]
        public void Other() { }
    }

    class TestableXunitTestAssemblyRunner : XunitTestAssemblyRunner
    {
        public ConcurrentBag<Tuple<int, IEnumerable<IXunitTestCase>>> TestCasesRun = new ConcurrentBag<Tuple<int, IEnumerable<IXunitTestCase>>>();

        TestableXunitTestAssemblyRunner(ITestAssembly testAssembly,
                                        IEnumerable<IXunitTestCase> testCases,
                                        IMessageSink messageSink,
                                        ITestFrameworkOptions executionOptions)
            : base(testAssembly, testCases, messageSink, executionOptions) { }

        public static TestableXunitTestAssemblyRunner Create(ITestAssembly assembly = null,
                                                             IXunitTestCase[] testCases = null,
                                                             ITestFrameworkOptions options = null)
        {
            if (testCases == null)
                testCases = new[] { Mocks.XunitTestCase<ClassUnderTest>("Passing") };

            return new TestableXunitTestAssemblyRunner(
                assembly ?? testCases.First().TestMethod.TestClass.TestCollection.TestAssembly,
                testCases ?? new IXunitTestCase[0],
                SpyMessageSink.Create(),
                options ?? new TestFrameworkOptions()
            );
        }

        public new string GetTestFrameworkDisplayName()
        {
            return base.GetTestFrameworkDisplayName();
        }

        public new string GetTestFrameworkEnvironment()
        {
            return base.GetTestFrameworkEnvironment();
        }

        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, CancellationTokenSource cancellationTokenSource)
        {
            TestCasesRun.Add(Tuple.Create(Thread.CurrentThread.ManagedThreadId, testCases));
            Thread.Sleep(5); // Hold onto the worker thread long enough to ensure tests all get spread around
            return Task.FromResult(new RunSummary());
        }
    }
}
