using System;
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
        public static void IsXunit()
        {
            var runner = TestableXunitTestAssemblyRunner.Create();

            var result = runner.GetTestFrameworkDisplayName();

            Assert.StartsWith("xUnit.net ", result);
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
            var assembly = Mocks.TestAssembly(new[] { attribute });
            var runner = TestableXunitTestAssemblyRunner.Create(assembly: assembly);

            var result = runner.GetTestFrameworkEnvironment();

            Assert.EndsWith("[collection-per-class, non-parallel]", result);
        }

        [Fact]
        public static void Attribute_MaxThreads()
        {
            var attribute = Mocks.CollectionBehaviorAttribute(maxParallelThreads: 3);
            var assembly = Mocks.TestAssembly(new[] { attribute });
            var runner = TestableXunitTestAssemblyRunner.Create(assembly: assembly);

            var result = runner.GetTestFrameworkEnvironment();

            Assert.EndsWith("[collection-per-class, parallel (3 threads)]", result);
        }

        [Theory]
        [InlineData(CollectionBehavior.CollectionPerAssembly, "collection-per-assembly")]
        [InlineData(CollectionBehavior.CollectionPerClass, "collection-per-class")]
        public static void Attribute_CollectionBehavior(CollectionBehavior behavior, string expectedDisplayText)
        {
            var attribute = Mocks.CollectionBehaviorAttribute(behavior, disableTestParallelization: true);
            var assembly = Mocks.TestAssembly(new[] { attribute });
            var runner = TestableXunitTestAssemblyRunner.Create(assembly: assembly);

            var result = runner.GetTestFrameworkEnvironment();

            Assert.EndsWith(String.Format("[{0}, non-parallel]", expectedDisplayText), result);
        }

        [Fact]
        public static void Attribute_CustomCollectionFactory()
        {
            var factoryType = typeof(MyTestCollectionFactory);
            var attr = Mocks.CollectionBehaviorAttribute(factoryType.FullName, factoryType.Assembly.FullName, disableTestParallelization: true);
            var assembly = Mocks.TestAssembly(new[] { attr });
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
            var options = TestFrameworkOptions.ForExecution();
            options.SetDisableParallelization(true);
            var runner = TestableXunitTestAssemblyRunner.Create(executionOptions: options);

            var result = runner.GetTestFrameworkEnvironment();

            Assert.EndsWith("[collection-per-class, non-parallel]", result);
        }

        [Fact]
        public static void TestOptions_MaxThreads()
        {
            var options = TestFrameworkOptions.ForExecution();
            options.SetMaxParallelThreads(3);
            var runner = TestableXunitTestAssemblyRunner.Create(executionOptions: options);

            var result = runner.GetTestFrameworkEnvironment();

            Assert.EndsWith("[collection-per-class, parallel (3 threads)]", result);
        }

        [Fact]
        public static void TestOptionsOverrideAttribute()
        {
            var attribute = Mocks.CollectionBehaviorAttribute(disableTestParallelization: true, maxParallelThreads: 127);
            var options = TestFrameworkOptions.ForExecution();
            options.SetDisableParallelization(false);
            options.SetMaxParallelThreads(3);
            var assembly = Mocks.TestAssembly(new[] { attribute });
            var runner = TestableXunitTestAssemblyRunner.Create(assembly: assembly, executionOptions: options);

            var result = runner.GetTestFrameworkEnvironment();

            Assert.EndsWith("[collection-per-class, parallel (3 threads)]", result);
        }
    }

    public class RunAsync
    {
        [Fact]
        public static async void Parallel_MultipleThreads()
        {
            var passing = Mocks.XunitTestCase<ClassUnderTest>("Passing");
            var other = Mocks.XunitTestCase<ClassUnderTest>("Other");
            var options = TestFrameworkOptions.ForExecution();
            options.SetMaxParallelThreads(2);
            var runner = TestableXunitTestAssemblyRunner.Create(testCases: new[] { passing, other }, executionOptions: options);

            await runner.RunAsync();

            var threadIDs = runner.TestCasesRun.Select(x => x.Item1).ToList();
            Assert.NotEqual(threadIDs[0], threadIDs[1]);
        }

        [Fact]
        public static async void Parallel_SingleThread()
        {
            var passing = Mocks.XunitTestCase<ClassUnderTest>("Passing");
            var other = Mocks.XunitTestCase<ClassUnderTest>("Other");
            var options = TestFrameworkOptions.ForExecution();
            options.SetMaxParallelThreads(1);
            var runner = TestableXunitTestAssemblyRunner.Create(testCases: new[] { passing, other }, executionOptions: options);

            await runner.RunAsync();

            var threadIDs = runner.TestCasesRun.Select(x => x.Item1).ToList();
            Assert.Equal(threadIDs[0], threadIDs[1]);
        }

        [Fact]
        public static async void NonParallel()
        {
            var passing = Mocks.XunitTestCase<ClassUnderTest>("Passing");
            var other = Mocks.XunitTestCase<ClassUnderTest>("Other");
            var options = TestFrameworkOptions.ForExecution();
            options.SetDisableParallelization(true);
            var runner = TestableXunitTestAssemblyRunner.Create(testCases: new[] { passing, other }, executionOptions: options);

            await runner.RunAsync();

            var threadIDs = runner.TestCasesRun.Select(x => x.Item1).ToList();
            Assert.Equal(threadIDs[0], threadIDs[1]);
        }
    }

    public class TestCaseOrderer
    {
        [Fact]
        public static void CanSetTestCaseOrdererInAssemblyAttribute()
        {
            var ordererAttribute = Mocks.TestCaseOrdererAttribute<MyTestCaseOrderer>();
            var assembly = Mocks.TestAssembly(new[] { ordererAttribute });
            var runner = TestableXunitTestAssemblyRunner.Create(assembly: assembly);

            runner.Initialize();

            Assert.IsType<MyTestCaseOrderer>(runner.TestCaseOrderer);
        }

        class MyTestCaseOrderer : ITestCaseOrderer
        {
            public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
            {
                throw new NotImplementedException();
            }
        }
    }

    public class TestCollectionOrderer
    {
        [Fact]
        public static void CanSetTestCollectionOrdererInAssemblyAttribute()
        {
            var ordererAttribute = Mocks.TestCollectionOrdererAttribute<MyTestCollectionOrderer>();
            var assembly = Mocks.TestAssembly(new[] { ordererAttribute });
            var runner = TestableXunitTestAssemblyRunner.Create(assembly: assembly);

            runner.Initialize();

            Assert.IsType<MyTestCollectionOrderer>(runner.TestCollectionOrderer);
        }

        class MyTestCollectionOrderer : ITestCollectionOrderer
        {
            public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> TestCollections)
            {
                return TestCollections.OrderByDescending(c => c.DisplayName);
            }
        }
    }

    class ClassUnderTest
    {
        [Fact]
        public void Passing() { Thread.Sleep(0); }

        [Fact]
        public void Other() { Thread.Sleep(0); }
    }

    class TestableXunitTestAssemblyRunner : XunitTestAssemblyRunner
    {
        public ConcurrentBag<Tuple<int, IEnumerable<IXunitTestCase>>> TestCasesRun = new ConcurrentBag<Tuple<int, IEnumerable<IXunitTestCase>>>();

        TestableXunitTestAssemblyRunner(ITestAssembly testAssembly,
                                        IEnumerable<IXunitTestCase> testCases,
                                        IMessageSink diagnosticMessageSink,
                                        IMessageSink executionMessageSink,
                                        ITestFrameworkExecutionOptions executionOptions)
            : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions) { }

        public static TestableXunitTestAssemblyRunner Create(ITestAssembly assembly = null,
                                                             IXunitTestCase[] testCases = null,
                                                             ITestFrameworkExecutionOptions executionOptions = null)
        {
            if (testCases == null)
                testCases = new[] { Mocks.XunitTestCase<ClassUnderTest>("Passing") };

            return new TestableXunitTestAssemblyRunner(
                assembly ?? testCases.First().TestMethod.TestClass.TestCollection.TestAssembly,
                testCases ?? new IXunitTestCase[0],
                SpyMessageSink.Create(),
                SpyMessageSink.Create(),
                executionOptions ?? TestFrameworkOptions.ForExecution()
            );
        }

        public new ITestCaseOrderer TestCaseOrderer
        {
            get { return base.TestCaseOrderer; }
        }

        public new ITestCollectionOrderer TestCollectionOrderer
        {
            get { return base.TestCollectionOrderer; }
            set { base.TestCollectionOrderer = value; }
        }

        public new string GetTestFrameworkDisplayName()
        {
            return base.GetTestFrameworkDisplayName();
        }

        public new string GetTestFrameworkEnvironment()
        {
            return base.GetTestFrameworkEnvironment();
        }

        public new void Initialize()
        {
            base.Initialize();
        }

        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, CancellationTokenSource cancellationTokenSource)
        {
            TestCasesRun.Add(Tuple.Create(Thread.CurrentThread.ManagedThreadId, testCases));
            Thread.Sleep(5); // Hold onto the worker thread long enough to ensure tests all get spread around
            return Task.FromResult(new RunSummary());
        }
    }
}
