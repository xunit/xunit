//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Threading;
//using Moq;
//using Xunit;
//using Xunit.Abstractions;
//using Xunit.Sdk;

//public class ExecutorTests
//{
//    public class Construction
//    {
//        [Fact]
//        public void SuccessfulConstructionReturnNormalizedAssemblyFilename()
//        {
//            var executor = TestableExecutor.Create("Filename.dll");

//            Assert.Equal(Path.GetFullPath("Filename.dll"), executor.AssemblyFileName);
//        }

//        [Fact]
//        public void SuccessfulConstructionLoadsAssembly()
//        {
//            var executor = TestableExecutor.Create(@"C:\Dummy\Filename.dll");

//            executor.AssemblyLoader.Verify(l => l.Load(@"C:\Dummy\Filename.dll"), Times.Once());
//        }

//        [Fact]
//        public void LoadFailureExceptionsArePropagatedBackToCaller()
//        {
//            var thrown = new FileNotFoundException();
//            var loader = new Mock<IAssemblyLoader>();
//            loader.Setup(l => l.Load(It.IsAny<string>()))
//                  .Throws(thrown);

//            var exception = Record.Exception(() => TestableExecutor.Create(@"C:\Dummy\Filename.dll", loader));

//            Assert.Same(thrown, exception);
//        }
//    }

//    public class Dispose
//    {
//        [Fact]
//        public void DisposingExecutorDisposesMessageBus()
//        {
//            var executor = TestableExecutor.Create();

//            executor.Dispose();

//            executor.MessageBus.Verify(mb => mb.Dispose());
//        }
//    }

//    public class EnumerateTests
//    {
//        [Fact]
//        public void CallsTestFrameworkWithCorrectParameters()
//        {
//            var executor = TestableExecutor.Create();

//            new Executor2.EnumerateTests(executor, includeSourceInformation: true);

//            executor.AssemblyLoader.Verify(al => al.Load(executor.AssemblyFileName));
//            executor.TestFramework.Verify(f => f.Find(It.IsAny<IAssemblyInfo>(), true));
//        }

//        [Fact]
//        public void NoTestMethods()
//        {
//            var executor = TestableExecutor.Create();

//            new Executor2.EnumerateTests(executor, includeSourceInformation: false);

//            CollectionAssert.Collection(executor.Messages,
//                message => Assert.IsAssignableFrom<IDiscoveryCompleteMessage>(message)
//            );
//        }

//        [Fact]
//        public void SingleTestMethod()
//        {
//            var testCase = new Mock<ITestCase>();
//            var framework = new MockTestFramework(testCase);
//            var executor = TestableExecutor.Create(testFramework: framework);

//            new Executor2.EnumerateTests(executor, includeSourceInformation: false);

//            CollectionAssert.Collection(executor.Messages,
//                message => Assert.Same(testCase.Object, ((ITestCaseDiscoveryMessage)message).TestCase),
//                message => Assert.IsAssignableFrom<IDiscoveryCompleteMessage>(message)
//            );
//        }

//        [Fact]
//        public void ExceptionThrownInTestFramework_StopsEnumeration_IssuesErrorMessageAndCompletes()
//        {
//            var testCase = new Mock<ITestCase>();
//            var thrown = new FileNotFoundException();
//            var framework = new MockTestFramework(ReturnThenThrow(testCase, thrown));
//            var executor = TestableExecutor.Create(testFramework: framework);

//            new Executor2.EnumerateTests(executor, includeSourceInformation: false);

//            CollectionAssert.Collection(executor.Messages,
//                message => Assert.Same(testCase.Object, ((ITestCaseDiscoveryMessage)message).TestCase),
//                message => Assert.Same(thrown, ((IErrorMessage)message).Error),
//                message => Assert.IsAssignableFrom<IDiscoveryCompleteMessage>(message)
//            );
//        }

//        private static IEnumerable<ITestCase> ReturnThenThrow(Mock<ITestCase> testCase, Exception ex)
//        {
//            yield return testCase.Object;
//            throw ex;
//        }
//    }

//    public class RunTests
//    {
//        [Fact]
//        public void NoTests()
//        {
//            var executor = TestableExecutor.Create();
//            var assembly = executor.AssemblyLoader.Object.Load("Dummy");

//            new Executor2.RunTests(executor, new ITestCase[0], default(CancellationToken));

//            CollectionAssert.Collection(executor.Messages,
//                message =>
//                {
//                    var starting = Assert.IsAssignableFrom<ITestAssemblyStarting>(message);
//                    var assemblyInfo = Assert.IsAssignableFrom<IReflectionAssemblyInfo>(starting.Assembly);
//                    Assert.Same(assembly, assemblyInfo.Assembly);
//                },
//                message =>
//                {
//                    var finished = Assert.IsAssignableFrom<ITestAssemblyFinished>(message);
//                    Assert.Equal(0, finished.TestsRun);
//                    Assert.Equal(0, finished.TestsFailed);
//                    Assert.Equal(0, finished.TestsSkipped);
//                    Assert.Equal(0M, finished.ExecutionTime);

//                    var assemblyInfo = Assert.IsAssignableFrom<IReflectionAssemblyInfo>(finished.Assembly);
//                    Assert.Same(assembly, assemblyInfo.Assembly);
//                }
//            );
//        }

//        [Fact]
//        public void SingleTest()
//        {
//            var test = new Mock<ITestCase>();
//            var testFramework = new MockTestFramework(test);
//            var executor = TestableExecutor.Create(testFramework: testFramework);
//            var assembly = executor.AssemblyLoader.Object.Load("Dummy");

//            new Executor2.RunTests(executor, new ITestCase[0], default(CancellationToken));

//            CollectionAssert.Collection(executor.Messages,
//                message =>
//                {
//                    var starting = Assert.IsAssignableFrom<ITestAssemblyStarting>(message);
//                    var assemblyInfo = Assert.IsAssignableFrom<IReflectionAssemblyInfo>(starting.Assembly);
//                    Assert.Same(assembly, assemblyInfo.Assembly);
//                },
//                message =>
//                {
//                    var finished = Assert.IsAssignableFrom<ITestAssemblyFinished>(message);
//                    Assert.Equal(0, finished.TestsRun);
//                    Assert.Equal(0, finished.TestsFailed);
//                    Assert.Equal(0, finished.TestsSkipped);
//                    Assert.Equal(0M, finished.ExecutionTime);

//                    var assemblyInfo = Assert.IsAssignableFrom<IReflectionAssemblyInfo>(finished.Assembly);
//                    Assert.Same(assembly, assemblyInfo.Assembly);
//                }
//            );
//        }
//    }

//    class TestableExecutor : Executor2
//    {
//        private TestableExecutor(string assemblyFileName, MockMessageBus messageBus, Mock<IAssemblyLoader> assemblyLoader, Mock<ITestFramework> testFramework)
//            : base(assemblyFileName, messageBus.Object, assemblyLoader.Object, testFramework.Object)
//        {
//            MessageBus = messageBus;
//            AssemblyLoader = assemblyLoader;
//            TestFramework = testFramework;
//        }

//        public Mock<IAssemblyLoader> AssemblyLoader { get; private set; }

//        public MockMessageBus MessageBus { get; private set; }

//        public List<ITestMessage> Messages
//        {
//            get { return MessageBus.Messages; }
//        }

//        public Mock<ITestFramework> TestFramework { get; private set; }

//        public static TestableExecutor Create(string assemblyFileName = "DummyFilename.dll", Mock<IAssemblyLoader> assemblyLoader = null, Mock<ITestFramework> testFramework = null)
//        {
//            return new TestableExecutor(assemblyFileName, new MockMessageBus(), assemblyLoader ?? new MockAssemblyLoader(), testFramework ?? new MockTestFramework());
//        }

//        internal class MockMessageBus : Mock<IMessageBus>
//        {
//            public List<ITestMessage> Messages = new List<ITestMessage>();

//            public MockMessageBus()
//            {
//                this.Setup(mb => mb.QueueMessage(It.IsAny<ITestMessage>()))
//                    .Callback<ITestMessage>(Messages.Add);
//            }
//        }
//    }

//    class MockAssemblyLoader : Mock<IAssemblyLoader>
//    {
//        public MockAssemblyLoader()
//        {
//            this.Setup(al => al.Load(It.IsAny<string>()))
//                .Returns(Assembly.GetExecutingAssembly());
//        }
//    }

//    class MockTestFramework : Mock<ITestFramework>
//    {
//        public MockTestFramework(params ITestCase[] testCases)
//        {
//            this.Setup(f => f.Find(It.IsAny<IAssemblyInfo>(), It.IsAny<bool>()))
//                .Returns(testCases);
//        }

//        public MockTestFramework(params Mock<ITestCase>[] testCases)
//        {
//            this.Setup(f => f.Find(It.IsAny<IAssemblyInfo>(), It.IsAny<bool>()))
//                .Returns(testCases.Select(tc => tc.Object));
//        }

//        public MockTestFramework(IEnumerable<ITestCase> testCases)
//        {
//            this.Setup(f => f.Find(It.IsAny<IAssemblyInfo>(), It.IsAny<bool>()))
//                .Returns(testCases);
//        }
//    }
//}
