using System;
using System.Collections.Generic;
using System.IO;
using Moq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class ExecutorTests
{
    public class Construction
    {
        [Fact]
        public void SuccessfulConstructionReturnNormalizedAssemblyFilename()
        {
            var executor = TestableExecutor.Create("Filename.dll");

            Assert.Equal(Path.GetFullPath("Filename.dll"), executor.AssemblyFileName);
        }

        [Fact]
        public void SuccessfulConstructionLoadsAssembly()
        {
            var executor = TestableExecutor.Create(@"C:\Dummy\Filename.dll");

            executor.AssemblyLoader.Verify(l => l.Load(@"C:\Dummy\Filename.dll"), Times.Once());
        }

        [Fact]
        public void LoadFailureExceptionsArePropagatedBackToCaller()
        {
            var thrown = new FileNotFoundException();
            var loader = new Mock<IAssemblyLoader>();
            loader.Setup(l => l.Load(It.IsAny<string>()))
                  .Throws(thrown);

            var exception = Record.Exception(() => TestableExecutor.Create(@"C:\Dummy\Filename.dll", loader));

            Assert.Same(thrown, exception);
        }
    }

    public class Dispose
    {
        [Fact]
        public void DisposingExecutorDisposesMessageBus()
        {
            var executor = TestableExecutor.Create();

            executor.Dispose();

            executor.MessageBus.Verify(mb => mb.Dispose());
        }
    }

    public class EnumerateTests
    {
        [Fact]
        public void CallsTestFrameworkWithCorrectParameters()
        {
            var executor = TestableExecutor.Create();
            var framework = new MockTestFramework();

            new Executor2.EnumerateTests(executor, includeSourceInformation: true, testFrameworks: new[] { framework.Object });

            executor.AssemblyLoader.Verify(al => al.Load(executor.AssemblyFileName));
            framework.Verify(f => f.Find(It.IsAny<IAssemblyInfo>(), true));
        }

        [Fact]
        public void NoTestMethods()
        {
            var executor = TestableExecutor.Create();
            var framework = new MockTestFramework();

            new Executor2.EnumerateTests(executor, includeSourceInformation: false, testFrameworks: new[] { framework.Object });

            CollectionAssert.Collection(executor.Messages,
                message => Assert.IsAssignableFrom<IDiscoveryCompleteMessage>(message)
            );
        }

        [Fact]
        public void SingleTestMethod()
        {
            var executor = TestableExecutor.Create();
            var testCase = new Mock<ITestCase>();
            var framework = new MockTestFramework(testCase.Object);

            new Executor2.EnumerateTests(executor, includeSourceInformation: false, testFrameworks: new[] { framework.Object });

            CollectionAssert.Collection(executor.Messages,
                message => Assert.Same(testCase.Object, ((ITestCaseDiscoveryMessage)message).TestCase),
                message => Assert.IsAssignableFrom<IDiscoveryCompleteMessage>(message)
            );
        }

        [Fact]
        public void MultipleTestFrameworks_ResultsAreAggregated()
        {
            var executor = TestableExecutor.Create();
            var testCase1 = new Mock<ITestCase>();
            var framework1 = new MockTestFramework(testCase1.Object);
            var testCase2 = new Mock<ITestCase>();
            var framework2 = new MockTestFramework(testCase2.Object);

            new Executor2.EnumerateTests(executor, includeSourceInformation: false, testFrameworks: new[] { framework1.Object, framework2.Object });

            CollectionAssert.Collection(executor.Messages,
                message => Assert.Same(testCase1.Object, ((ITestCaseDiscoveryMessage)message).TestCase),
                message => Assert.Same(testCase2.Object, ((ITestCaseDiscoveryMessage)message).TestCase),
                message => Assert.IsAssignableFrom<IDiscoveryCompleteMessage>(message)
            );
        }

        [Fact]
        public void ExceptionThrownInTestFramework_StopsEnumeration_IssuesErrorMessageAndCompletes()
        {
            var executor = TestableExecutor.Create();
            var testCase1 = new Mock<ITestCase>();
            var thrown = new FileNotFoundException();
            var framework1 = new MockTestFramework(ReturnThenThrow(testCase1.Object, thrown));
            var testCase2 = new Mock<ITestCase>();
            var framework2 = new MockTestFramework(testCase2.Object);

            new Executor2.EnumerateTests(executor, includeSourceInformation: false, testFrameworks: new[] { framework1.Object, framework2.Object });

            CollectionAssert.Collection(executor.Messages,
                message => Assert.Same(testCase1.Object, ((ITestCaseDiscoveryMessage)message).TestCase),
                message => Assert.Same(thrown, ((IErrorMessage)message).Error),
                message => Assert.IsAssignableFrom<IDiscoveryCompleteMessage>(message)
            );
        }

        private static IEnumerable<ITestCase> ReturnThenThrow(ITestCase testCase, Exception ex)
        {
            yield return testCase;
            throw ex;
        }
    }

    class TestableExecutor : Executor2
    {
        private TestableExecutor(string assemblyFileName, MockMessageBus messageBus, Mock<IAssemblyLoader> assemblyLoader)
            : base(assemblyFileName, messageBus.Object, assemblyLoader.Object)
        {
            MessageBus = messageBus;
            AssemblyLoader = assemblyLoader;
        }

        public Mock<IAssemblyLoader> AssemblyLoader { get; private set; }

        public MockMessageBus MessageBus { get; private set; }

        public List<ITestMessage> Messages
        {
            get { return MessageBus.Messages; }
        }

        public static TestableExecutor Create()
        {
            return Create("DummyFilename.dll");
        }

        public static TestableExecutor Create(string assemblyFileName)
        {
            return Create(assemblyFileName, new Mock<IAssemblyLoader>());
        }

        public static TestableExecutor Create(string assemblyFileName, Mock<IAssemblyLoader> assemblyLoader)
        {
            return new TestableExecutor(assemblyFileName, new MockMessageBus(), assemblyLoader);
        }

        internal class MockMessageBus : Mock<IMessageBus>
        {
            public List<ITestMessage> Messages = new List<ITestMessage>();

            public MockMessageBus()
            {
                this.Setup(mb => mb.QueueMessage(It.IsAny<ITestMessage>()))
                    .Callback<ITestMessage>(Messages.Add);
            }
        }
    }

    class MockTestFramework : Mock<ITestFramework>
    {
        public MockTestFramework(params ITestCase[] testCases)
        {
            this.Setup(f => f.Find(It.IsAny<IAssemblyInfo>(), It.IsAny<bool>()))
                .Returns(testCases);
        }

        public MockTestFramework(IEnumerable<ITestCase> testCases)
        {
            this.Setup(f => f.Find(It.IsAny<IAssemblyInfo>(), It.IsAny<bool>()))
                .Returns(testCases);
        }
    }
}
